using MongoDB.Entities;
using SearchService.Models;
using System.Text.Json;

namespace SearchService.Services;

public class AuctionService
{
    private readonly IHttpClientFactory _httpClient;

    public AuctionService(IHttpClientFactory httpClient)
    {
        _httpClient = httpClient;
    }
    
    public async Task<List<Item>> GetItemsForSearchDb()
    {
        var lastUpdated = await DB.Find<Item, string>()
            .Sort(x => x.Descending(x => x.UpdatedAt))
            .Project(x => x.UpdatedAt.ToString())
            .ExecuteFirstAsync();
        
        var client = _httpClient.CreateClient("AuctionSvc");
        var response = await client.GetAsync($"/auctions?date={lastUpdated}");

        if (response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();

            if (content is null)
            {
                return null!;
            }

            var items = JsonSerializer.Deserialize<List<Item>>(content, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (items is not null)
            {
                return items;
            }

            return null!;
        }

        return null!;
    }
}
