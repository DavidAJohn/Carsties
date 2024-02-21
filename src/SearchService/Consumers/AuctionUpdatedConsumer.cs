using AutoMapper;
using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionUpdatedConsumer : IConsumer<AuctionUpdated>
{
    private readonly IMapper _mapper;

    public AuctionUpdatedConsumer(IMapper mapper)
    {
        _mapper = mapper;
    }

    public async Task Consume(ConsumeContext<AuctionUpdated> context)
    {
        Console.WriteLine($"AuctionUpdatedConsumer: {context.Message.Id}");

        var item = _mapper.Map<Item>(context.Message);

        var result = await DB.Update<Item>()
            .MatchID(item.ID)
            .ModifyOnly(x => new
            {
                x.Make,
                x.Model,
                x.Color,
                x.Year,
                x.Mileage
            }, item)
            .ExecuteAsync();

        if (!result.IsAcknowledged)
        {
            Console.WriteLine($"AuctionUpdatedConsumer: Failed to update item {item.ID}");
        }
        else
        {
            Console.WriteLine($"AuctionUpdatedConsumer: Updated item {item.ID}");
        }
    }
}
