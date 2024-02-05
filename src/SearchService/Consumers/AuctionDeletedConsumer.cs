using Contracts;
using MassTransit;
using MongoDB.Entities;
using SearchService.Models;

namespace SearchService.Consumers;

public class AuctionDeletedConsumer : IConsumer<AuctionDeleted>
{
    public async Task Consume(ConsumeContext<AuctionDeleted> context)
    {
        Console.WriteLine($"AuctionDeletedConsumer: {context.Message.Id}");

        var result = await DB.DeleteAsync<Item>(context.Message.Id);

        if (!result.IsAcknowledged)
        {
            Console.WriteLine($"AuctionDeletedConsumer: Failed to delete item {context.Message.Id}");
        }
        else
        {
            Console.WriteLine($"AuctionDeletedConsumer: Deleted item {context.Message.Id}");
        }
    }
}
