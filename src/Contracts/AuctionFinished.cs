﻿namespace Contracts;

public class AuctionFinished
{
    public bool ItemSold { get; set; }
    public required string AuctionId { get; set; }
    public required string Winner { get; set; }
    public required string Seller { get; set; }
    public int Amount { get; set; }
}
