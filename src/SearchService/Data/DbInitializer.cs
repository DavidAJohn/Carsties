using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.Services;

namespace SearchService.Data;

public class DbInitializer
{
    public static async Task InitDb(WebApplication app)
    {
        await DB.InitAsync("SearchDb", 
                    MongoClientSettings.FromConnectionString(app.Configuration.GetConnectionString("MongoDbConnection")));

        await DB.Index<Item>()
            .Key(x => x.Make, KeyType.Text)
            .Key(x => x.Model, KeyType.Text)
            .Key(x => x.Color, KeyType.Text)
            .CreateAsync();

        var count = await DB.Queryable<Item>().CountAsync();

        if (count == 0)
        {
            using var scope = app.Services.CreateScope();
            var svc = scope.ServiceProvider.GetRequiredService<AuctionService>();

            var items = await svc.GetItemsForSearchDb();

            Console.WriteLine($"Items from AuctionSvc: {items.Count}");

            if (items.Count > 0)
            {
                await DB.SaveAsync(items);
            }
        }
    }
}
