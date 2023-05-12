using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArticleServiceAPI.Model
{
	public class Auctionhouse
	{
        [BsonId]
        [BsonElement(elementName: "_id")]
        public ObjectId AuctionhouseID { get; set; }
        public string? Name { get; set; }
        public string? Address { get; set; }
        public int CvrNumber { get; set; }

        public Auctionhouse()
		{
		}
	}
}

