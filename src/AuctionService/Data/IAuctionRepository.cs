using AuctionService.DTOs;
using AuctionService.Entities;

namespace AuctionService.Data;

public interface IAuctionRepository
{
    Task<IEnumerable<AuctionDTO>> GetAuctionsAsync(string? date);
    Task<AuctionDTO> GetAuctionByIdAsync(Guid id);
    Task<Auction> GetAuctionEntityByIdAsync(Guid id);
    void CreateAuctionAsync(Auction auction);
    void DeleteAuctionAsync(Auction auction);
    Task<bool> SaveChangesAsync();
}
