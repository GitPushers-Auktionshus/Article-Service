using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArticleServiceAPI.Model
{
    public class BuyerDTO
    {
        public User BuyerID { get; set; }

        public BuyerDTO()
        {
        }
    }
}

