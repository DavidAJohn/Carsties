using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.IntegrationTests.Fixtures;
using AuctionService.IntegrationTests.Util;
using Contracts;
using MassTransit.Testing;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using System.Net;
using AuctionService.Entities;
using Newtonsoft.Json;
using MassTransit;

namespace AuctionService.IntegrationTests;

public class AuctionBusTests : IClassFixture<CustomWebAppFactory>, IAsyncLifetime
{
    private readonly CustomWebAppFactory _factory;
    private readonly HttpClient _httpClient;
    private readonly ITestHarness _testHarness;

    public AuctionBusTests(CustomWebAppFactory factory)
    {
        _factory = factory;
        _httpClient = factory.CreateClient();
        _testHarness = factory.Services.GetTestHarness();
    }

    [Fact]
    public async Task CreateAuction_ShouldPublishAuctionCreated_WhenAuctionCreated()
    {
        // Arrange
        var auctionCreationDTO = GetCreateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auctionCreationDTO);

        // Assert
        response.EnsureSuccessStatusCode();
        Assert.True(await _testHarness.Published.Any<AuctionCreated>());
    }

    [Fact]
    public async Task UpdateAuction_ShouldPublishAuctionUpdated_WhenAuctionIsUpdated()
    {
        // Arrange
        var auctionCreationDTO = GetCreateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auctionCreationDTO);
        response.EnsureSuccessStatusCode();

        var auction = await response.Content.ReadFromJsonAsync<AuctionDTO>();
        auction.Color = "Blue";

        // Act
        response = await _httpClient.PutAsJsonAsync($"api/auctions/{auction.Id}", auction);
        response.EnsureSuccessStatusCode();

        // Assert
        Assert.True(await _testHarness.Published.Any<AuctionUpdated>());
    }

    [Fact]
    public async Task DeleteAuction_ShouldPublishAuctionDeleted_WithAuctionWasDeleted()
    {
        // Arrange
        var auctionCreationDTO = GetCreateAuctionDTO();
        _httpClient.SetFakeJwtBearerToken(AuthHelper.GetBearerForUser("test-user"));

        // Act
        var response = await _httpClient.PostAsJsonAsync("api/auctions", auctionCreationDTO);
        response.EnsureSuccessStatusCode();

        var createdAuction = await response.Content.ReadFromJsonAsync<AuctionDTO>();
        var auctionId = createdAuction!.Id;

        await _httpClient.DeleteAsync($"api/auctions/{auctionId}");

        // assert
        Assert.True(await _testHarness.Published.Any<AuctionDeleted>());
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
