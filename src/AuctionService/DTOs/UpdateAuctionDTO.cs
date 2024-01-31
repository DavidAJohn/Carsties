namespace AuctionService.DTOs;

public class UpdateAuctionDTO
{
    public required string Make { get; set; }
    public required string Model { get; set; }
    public int Year { get; set; }
    public required string Color { get; set; }
    public int Mileage { get; set; }
}
