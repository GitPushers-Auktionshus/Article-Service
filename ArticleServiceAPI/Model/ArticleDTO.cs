﻿using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Runtime.ConstrainedExecution;

namespace ArticleServiceAPI.Model
{
    public class ArticleDTO
    {
        public string? Name { get; set; }
        public bool NoReserve { get; set; }
        public double EstimatedPrice { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool Sold { get; set; }
        public string AuctionhouseID { get; set; }
        public string SellerID { get; set; }
        public double MinPrice { get; set; }
        public string BuyerID { get; set; }

        public ArticleDTO()
        {
        }

    }
}

