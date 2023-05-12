using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArticleServiceAPI.Model
{
    public class AuctionhouseDTO
    {
        [BsonId]
        [BsonElement(elementName: "_id")]
        public ObjectId AuctionhouseID { get; set; }

        public AuctionhouseDTO()
        {
        }
    }
}

