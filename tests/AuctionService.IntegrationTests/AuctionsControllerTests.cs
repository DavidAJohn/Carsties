using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace AuctionService.IntegrationTests;

public class AuctionsControllerTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;

    public AuctionsControllerTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = _factory.CreateClient();
    }

    [Fact]
    public async Task GetAuctions_ShouldReturnAuctions_WhenAuctionsExist()
    {
        // Arrange
        var response = await _httpClient.GetAsync("/api/auctions");
        response.EnsureSuccessStatusCode();

        // Act
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();

        // Assert
        Assert.Equal(3, auctions!.Count);
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnAuction_WhenAuctionExists()
    {
        // Arrange
        var response = await _httpClient.GetAsync("/api/auctions");
        response.EnsureSuccessStatusCode();
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();

        // Act
        var auctionId = auctions!.First().Id;
        response = await _httpClient.GetAsync($"/api/auctions/{auctionId}");
        response.EnsureSuccessStatusCode();
        var auction = await response.Content.ReadFromJsonAsync<AuctionDTO>();

        // Assert
        Assert.Equal(auctionId, auction!.Id);
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnNotFound_WhenAuctionDoesNotExist()
    {
        // Arrange

        // Act
        var auctionId = Guid.NewGuid();
        var response = await _httpClient.GetAsync($"/api/auctions/{auctionId}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetAuctionById_ShouldReturnBadRequest_WhenInvalidGuidSupplied()
    {
        // Arrange

        // Act
        var response = await _httpClient.GetAsync("/api/auctions/not-a-valid-guid");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var createAuctionDTO = GetCreateAuctionDTO();

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", createAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnCreated_WhenAuctionCreated()
    {
        // Arrange
        var createAuctionDTO = GetCreateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", createAuctionDTO);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDTO>();
        Assert.NotNull(createdAuction);
        Assert.Equal("test-user", createdAuction!.Seller);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnBadRequest_WhenInvalidAuctionSupplied()
    {
        // Arrange
        var createAuctionDTO = GetCreateAuctionDTO();
        createAuctionDTO.Make = null!;

        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", createAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateAuction_ShouldReturnForbidden_WhenUserIsInvalid()
    {
        // Arrange
        var createAuctionDTO = GetCreateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(" "));

        // Act
        var response = await _httpClient.PostAsJsonAsync("/api/auctions", createAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnNotFound_WhenAuctionDoesNotExist()
    {
        // Arrange
        var updateAuctionDTO = new UpdateAuctionDTO { Model = "Test", Make = "Updated", Color = "Red" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{Guid.NewGuid()}", updateAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnUnauthorized_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var updateAuctionDTO = new UpdateAuctionDTO { Model = "Test", Make = "Updated", Color = "Red" };

        // Act
        var response = await _httpClient.PutAsJsonAsync($"/api/auctions/{Guid.NewGuid()}", updateAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnForbidden_WhenUserIsInvalid()
    {
        // Arrange
        var response = await _httpClient.GetAsync("/api/auctions");
        response.EnsureSuccessStatusCode();
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();
        var auctionId = auctions!.First().Id;

        var updateAuctionDTO = new UpdateAuctionDTO { Model = "Test", Make = "Updated", Color = "Red" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(" "));

        // Act
        response = await _httpClient.PutAsJsonAsync($"/api/auctions/{auctionId}", updateAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnForbidden_WhenSellerIsDifferentToUser()
    {
        // Arrange
        var response = await _httpClient.GetAsync("/api/auctions");
        response.EnsureSuccessStatusCode();
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();
        var auctionId = auctions!.First().Id;

        var updateAuctionDTO = new UpdateAuctionDTO { Model = "Test", Make = "Updated", Color = "Red" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        response = await _httpClient.PutAsJsonAsync($"/api/auctions/{auctionId}", updateAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task UpdateAuction_ShouldReturnNoContent_WhenAuctionIsUpdated()
    {
        // Arrange
        var response = await _httpClient.GetAsync("/api/auctions");
        response.EnsureSuccessStatusCode();
        var auctions = await response.Content.ReadFromJsonAsync<List<AuctionDTO>>();
        var auctionId = auctions!.First().Id;

        var updateAuctionDTO = new UpdateAuctionDTO { Model = "Test", Make = "Updated", Color = "Red" };
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser(auctions!.First().Seller));

        // Act
        response = await _httpClient.PutAsJsonAsync($"/api/auctions/{auctionId}", updateAuctionDTO);

        // Assert
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public Task DisposeAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AuctionDbContext>();
        DbHelper.ReinitDbForTests(db);

        return Task.CompletedTask;
    }

    private static CreateAuctionDTO GetCreateAuctionDTO()
    {
        return new CreateAuctionDTO
        {
            AuctionEnd = DateTime.UtcNow.AddDays(1),
            Color = "Red",
            ImageUrl = "http://image.com",
            Make = "Ford",
            Mileage = 10000,
            Model = "Tester",
            ReservePrice = 5000,
            Year = 2020
        };
    }
}