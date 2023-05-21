using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArticleServiceAPI.Model
{
	public class Auctionhouse
	{
        [BsonId]
        public string AuctionhouseID { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public string CvrNumber { get; set; }

        public Auctionhouse()
		{
		}
	}
}

