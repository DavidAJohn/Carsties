using AuctionService.Data;
using AuctionService.protos;
using Grpc.Core;

namespace AuctionService.Services;

public class GrpcAuctionService : GrpcAuction.GrpcAuctionBase
{
    private readonly AuctionDbContext _auctionDbContext;

    public GrpcAuctionService(AuctionDbContext auctionDbContext)
    {
        _auctionDbContext = auctionDbContext;
    }

    public override async Task<GrpcAuctionResponse> GetAuction(GetAuctionRequest request, ServerCallContext context)
    {
        Console.WriteLine("==> Received Grpc request for auction");

        var auction = await _auctionDbContext.Auctions.FindAsync(Guid.Parse(request.Id))
            ?? throw new RpcException(new Status(StatusCode.NotFound, "Auction not found"));

        var response = new GrpcAuctionResponse
        {
            Auction = new GrpcAuctionModel
            {
                Id = auction.Id.ToString(),
                Seller = auction.Seller,
                AuctionEnd = auction.AuctionEnd.ToString(),
                ReservePrice = auction.ReservePrice
            }
        };

        return response;
    }
}
