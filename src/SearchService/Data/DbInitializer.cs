using MongoDB.Driver;
using MongoDB.Driver.Linq;
using MongoDB.Entities;
using SearchService.Models;
using System.Text.Json;

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
            Console.WriteLine("Seeding database...");

            var itemData = await File.ReadAllTextAsync("Data/auctions.json");

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            var items = JsonSerializer.Deserialize<List<Item>>(itemData, options);

            if (items is not null)
            {
                foreach (var item in items)
                {
                    item.CreatedAt = DateTime.UtcNow;
                    item.UpdatedAt = DateTime.UtcNow;
                    item.AuctionEnd = DateTime.UtcNow.AddDays(14);
                }

                await DB.SaveAsync(items);
            }
            else
            {
                Console.WriteLine("Failed to seed database.");
            }
        }
    }
}
