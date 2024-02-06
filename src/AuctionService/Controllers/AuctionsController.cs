﻿using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly AuctionDbContext _context;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(AuctionDbContext context, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _context = context;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
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

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateAuction(CreateAuctionDTO createAuctionDTO)
    {
        var auction = _mapper.Map<Auction>(createAuctionDTO);

        if (User == null)
        {
            return Forbid();
        }

        if (User.Identity?.Name == null)
        {
            return Forbid();
        }
        else
        {
            auction.Seller = User.Identity.Name;
        }

        _context.Auctions.Add(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return CreatedAtAction(nameof(GetAuctionById), new { id = auction.Id }, _mapper.Map<AuctionDTO>(auction));
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuction(Guid id, UpdateAuctionDTO updateAuctionDTO)
    {
        var auction = await _context.Auctions
            .Include(a => a.Item)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (auction == null) return NotFound();

        if (User == null || User.Identity?.Name == null || auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        auction.Item.Make = updateAuctionDTO.Make ?? auction.Item.Make;
        auction.Item.Model = updateAuctionDTO.Model ?? auction.Item.Model;
        auction.Item.Color = updateAuctionDTO.Color ?? auction.Item.Color;
        auction.Item.Mileage = updateAuctionDTO.Mileage <= 1 ? auction.Item.Mileage : updateAuctionDTO.Mileage;
        auction.Item.Year = updateAuctionDTO.Year < auction.Item.Year ? auction.Item.Year : updateAuctionDTO.Year;

        await _publishEndpoint.Publish(_mapper.Map<AuctionUpdated>(auction));

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }

    [Authorize]
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

        if (User == null || User.Identity?.Name == null || auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        _context.Auctions.Remove(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _context.SaveChangesAsync() > 0;

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }
}
