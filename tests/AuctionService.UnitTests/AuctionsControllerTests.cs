using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AuctionService.RequestHelpers;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;

namespace AuctionService.UnitTests;

public class AuctionsControllerTests
{
    private readonly Mock<IAuctionRepository> _repository;
    private readonly Mock<IPublishEndpoint> _publishEndpoint;
    private readonly Fixture _fixture;
    private readonly AuctionsController _controller;
    private readonly IMapper _mapper;

    public AuctionsControllerTests()
    {
        _fixture = new Fixture();
        _repository = new Mock<IAuctionRepository>();
        _publishEndpoint = new Mock<IPublishEndpoint>();

        var mapper = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<MappingProfiles>();
        }).CreateMapper();

        _mapper = mapper;

        _controller = new AuctionsController(_repository.Object, _mapper, _publishEndpoint.Object);
    }

    [Fact]
    public async Task GetAuctions_ShouldReturnAuctions_WhenNoParamsSupplied()
    {
        // Arrange
        var auctions = _fixture.CreateMany<AuctionDTO>(10).ToList();
        _repository.Setup(x => x.GetAuctionsAsync(null)).ReturnsAsync(auctions);

        // Act
        var result = await _controller.GetAuctions(null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<AuctionDTO>>(okResult.Value);
        Assert.Equal(auctions.Count, model.Count());
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnAuction_WhenIdIsValid()
    {
        // Arrange
        var auction = _fixture.Create<AuctionDTO>();
        _repository.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // Act
        var result = await _controller.GetAuctionById(auction.Id);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var model = Assert.IsType<AuctionDTO>(okResult.Value);
        Assert.Equal(auction.Id, model.Id);
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnNotFound_WhenIdIsNotValid()
    {
        // Arrange
        _repository.Setup(x => x.GetAuctionByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // Act
        var result = await _controller.GetAuctionById(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnCreatedAtActionResult_WhenCreateAuctionDTOIsValid()
    {
        // Arrange
        var auction = _fixture.Create<CreateAuctionDTO>();
        _repository.Setup(repo => repo.CreateAuctionAsync(It.IsAny<Auction>()));
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, "testuser")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.CreateAuction(auction);
        var createdResult = result as CreatedAtActionResult;

        // Assert
        Assert.NotNull(createdResult);
        Assert.Equal("GetAuctionById", createdResult.ActionName);
        Assert.IsType<AuctionDTO>(createdResult.Value);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturn400BadRequest_WhenSaveFails()
    {
        // Arrange
        var auctionDto = _fixture.Create<CreateAuctionDTO>();
        _repository.Setup(repo => repo.CreateAuctionAsync(It.IsAny<Auction>()));
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, "testuser")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.CreateAuction(auctionDto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturn403Forbidden_WhenUserNameIsEmpty()
    {
        // Arrange
        var auctionDto = _fixture.Create<CreateAuctionDTO>();
        _repository.Setup(repo => repo.CreateAuctionAsync(It.IsAny<Auction>()));
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, " ")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.CreateAuction(auctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturn403Forbidden_WhenUserIsNull()
    {
        // Arrange
        var auctionDto = _fixture.Create<CreateAuctionDTO>();
        _repository.Setup(repo => repo.CreateAuctionAsync(It.IsAny<Auction>()));
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        // Act
        var result = await _controller.CreateAuction(auctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnNoContentResult_WhenAuctionIsUpdated()
    {
        // Arrange
        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, auction.Seller)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.UpdateAuction(auction.Id, auctionDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnNotFound_WhenAuctionIsNotFound()
    {
        // Arrange
        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // Act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), auctionDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturn403Forbidden_WhenUserIsNull()
    {
        // Arrange
        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // Act
        var result = await _controller.UpdateAuction(Guid.NewGuid(), auctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturn403Forbidden_WhenUserNameIsEmpty()
    {
        // Arrange
        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, " ")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.UpdateAuction(auction.Id, auctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturn403Forbidden_WhenUserNameIsDifferentToSeller()
    {
        // Arrange
        string testUser = "testUser";
        string differentUser = "differentUser";

        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = differentUser;

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, testUser)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.UpdateAuction(auction.Id, auctionDto);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnBadRequest_WhenAuctionUpdateFailed()
    {
        // Arrange
        var auctionDto = _fixture.Create<UpdateAuctionDTO>();
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, auction.Seller)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.UpdateAuction(auction.Id, auctionDto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturnNoContentResult_WhenAuctionIsDeleted()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, auction.Seller)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.DeleteAuction(auction.Id);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturnNotFound_WhenAuctionIsNotFound()
    {
        // Arrange
        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(value: null);

        // Act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturn403Forbidden_WhenUserIsNull()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);

        // Act
        var result = await _controller.DeleteAuction(Guid.NewGuid());

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturn403Forbidden_WhenUserNameIsEmpty()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, " ")
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.DeleteAuction(auction.Id);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturn403Forbidden_WhenUserNameIsDifferentToSeller()
    {
        // Arrange
        string testUser = "testUser";
        string differentUser = "differentUser";

        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();
        auction.Seller = differentUser;

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(true);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, testUser)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.DeleteAuction(auction.Id);

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public async Task DeleteAuction_ShouldReturnBadRequest_WhenAuctionUpdateFailed()
    {
        // Arrange
        var auction = _fixture.Build<Auction>().Without(x => x.Item).Create();
        auction.Item = _fixture.Build<Item>().Without(x => x.Auction).Create();

        _repository.Setup(repo => repo.GetAuctionEntityByIdAsync(It.IsAny<Guid>())).ReturnsAsync(auction);
        _repository.Setup(repo => repo.SaveChangesAsync()).ReturnsAsync(false);

        var user = _fixture.Create<ClaimsPrincipal>();
        user.AddIdentity(new ClaimsIdentity(new Claim[]
        {
            new(ClaimTypes.Name, auction.Seller)
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };

        // Act
        var result = await _controller.DeleteAuction(auction.Id);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }
}
