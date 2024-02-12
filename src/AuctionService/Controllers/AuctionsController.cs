using AuctionService.Data;
using AuctionService.DTOs;
using AuctionService.Entities;
using AutoMapper;
using Contracts;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuctionService.Controllers;

[ApiController]
[Route("api/auctions")]
public class AuctionsController : ControllerBase
{
    private readonly IAuctionRepository _repository;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public AuctionsController(IAuctionRepository repository, IMapper mapper, IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet]
    public async Task<IActionResult> GetAuctions(string? date)
    {
        return Ok(await _repository.GetAuctionsAsync(date));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetAuctionById(Guid id)
    {
        var auction = await _repository.GetAuctionByIdAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        return Ok(auction);
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

        _repository.CreateAuctionAsync(auction);

        var newAuction = _mapper.Map<AuctionDTO>(auction);
        await _publishEndpoint.Publish(_mapper.Map<AuctionCreated>(newAuction));

        var result = await _repository.SaveChangesAsync();

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
        var auction = await _repository.GetAuctionEntityByIdAsync(id);

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

        var result = await _repository.SaveChangesAsync();

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
        var auction = await _repository.GetAuctionEntityByIdAsync(id);

        if (auction == null)
        {
            return NotFound();
        }

        if (User == null || User.Identity?.Name == null || auction.Seller != User.Identity?.Name)
        {
            return Forbid();
        }

        _repository.DeleteAuctionAsync(auction);

        await _publishEndpoint.Publish<AuctionDeleted>(new { Id = auction.Id.ToString() });

        var result = await _repository.SaveChangesAsync();

        if (!result)
        {
            return BadRequest();
        }

        return NoContent();
    }
}
