﻿using System;

namespace Shared.Contracts.Commands
{
    public class PlaceBuyOfferCommandDto : ICommand
    {
        public Guid BuyerId { get; set; }
        public string Stock { get; set; }
        public int Amount { get; set; }
        public decimal Price { get; set; }
    }
}