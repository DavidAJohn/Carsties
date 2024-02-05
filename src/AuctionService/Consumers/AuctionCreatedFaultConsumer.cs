using Contracts;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Consumers;

public class AuctionCreatedFaultConsumer : IConsumer<Fault<AuctionCreated>>
{
    public async Task Consume(ConsumeContext<Fault<AuctionCreated>> context)
    {
        Console.WriteLine($"AuctionCreatedFaultConsumer: {context.Message.Message}");

        var exception = context.Message.Exceptions.First();

        if (exception is DbUpdateException dbUpdateException)
        {
            Console.WriteLine($"DbUpdate Exception: {dbUpdateException.Message}");
            await context.Publish(context.Message.Message);
        }
        else if (exception is ArgumentException argException)
        {
            Console.WriteLine($"Argument Exception: {argException.Message}");
            await context.Publish(context.Message.Message);
        }
        else
        {
            Console.WriteLine($"Unhandled exception: {exception.Message}");
        }
    }
}
