using AuctionService.Entities;

namespace AuctionService.UnitTests;

public class AuctionEntityTests
{
    private readonly Fixture _fixture;

    public AuctionEntityTests()
    {
        _fixture = new Fixture();

        _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
            .ForEach(b => _fixture.Behaviors.Remove(b));

        _fixture.Behaviors.Add(new OmitOnRecursionBehavior());
    }

    [Fact]
    public void HasReservePrice_ShouldReturnTrue_WhenReservePriceIsMoreThanZero()
    {
        // Arrange
        var auction = _fixture.Build<Auction>()
            .With(a => a.ReservePrice, 100)
            .Create();

        // Act
        var result = auction.HasReservePrice();

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void HasReservePrice_ShouldReturnFalse_WhenReservePriceIsZero()
    {
        // Arrange
        var auction = _fixture.Build<Auction>()
            .With(a => a.ReservePrice, 0)
            .Create();

        // Act
        var result = auction.HasReservePrice();

        // Assert
        Assert.False(result);
    }
}