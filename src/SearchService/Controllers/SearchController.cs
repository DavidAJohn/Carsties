using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelpers;

namespace SearchService.Controllers;

[ApiController]
[Route("api/search")]
public class SearchController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> SearchItems([FromQuery] SearchParams searchParams)
    {
        var query= DB.PagedSearch<Item, Item>();

        if (string.IsNullOrWhiteSpace(searchParams.SearchTerm))
        {
            if (string.IsNullOrWhiteSpace(searchParams.OrderBy)) // only add a default sort if there's also no order by
            {
                query.Sort(x => x.Ascending(a => a.AuctionEnd));
            }
        }
        else
        {
            query.Match(Search.Full, searchParams.SearchTerm).SortByTextScore();
        }

        if (!string.IsNullOrWhiteSpace(searchParams.OrderBy))
        {
            query = searchParams.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "model" => query.Sort(x => x.Ascending(a => a.Model)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };
        }

        if (!string.IsNullOrWhiteSpace(searchParams.FilterBy))
        {
            query = searchParams.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6) && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Seller))
        {
            query.Match(x => x.Seller == searchParams.Seller);
        }

        if (!string.IsNullOrWhiteSpace(searchParams.Winner))
        {
            query.Match(x => x.Winner == searchParams.Winner);
        }

        query.PageNumber(searchParams.PageNumber).PageSize(searchParams.PageSize);
        
        var result = await query.ExecuteAsync();

        return Ok(new
        {
            results = result.Results,
            pageCount = result.PageCount,
            totalCount = result.TotalCount
        });
    }
}
