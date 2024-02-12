using AuctionService.Controllers;
using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.RequestHelpers;
using AutoMapper;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Moq;

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
}
