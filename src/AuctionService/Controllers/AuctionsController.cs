using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;

    public AuctionsController(AuctionDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions(string? date)
    {
        var query = _context.Auctions
            .OrderBy(a => a.Item.Make)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(date))
        {
            query = query.Where(a => a.UpdatedAt.CompareTo(DateTime.Parse(date).ToUniversalTime()) > 0);
        }

        return Ok(await query.ProjectTo<AuctionDTO>(_mapper.ConfigurationProvider).ToListAsync());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        return Ok(_mapper.Map<AuctionDTO>(auction));
    }

    [HttpPost]
    public async Task<IActionResult> CreateAuction(CreateAuctionDTO createAuctionDTO)
    {
        var auction = _mapper.Map<Auction>(createAuctionDTO);
        auction.Seller = "test"; // temp value until Identity is implemented

        _context.Auctions.Add(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDTO>(auction));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDTO.Mileage <= 1 ? auction.Item.Mileage : updateAuctionDTO.Mileage;
        auction.Item.Year = updateAuctionDTO.Year < auction.Item.Year ? auction.Item.Year : updateAuctionDTO.Year;

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuction(Guid id)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null)
        {
            return NotFound();
        }

        _context.Auctions.Remove(auction);
        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }
}
