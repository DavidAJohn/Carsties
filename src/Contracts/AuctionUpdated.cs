namespace Contracts;

public class AuctionUpdated
{
    public required string Id { get; set; }
    public required string Make { get; set; }
    public required string Model { get; set; }
    public required int Year { get; set; }
    public required string Color { get; set; }
    public required int Mileage { get; set; }
}
