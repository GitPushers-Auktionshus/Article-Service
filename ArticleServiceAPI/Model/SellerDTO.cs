using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace ArticleServiceAPI.Model
{
	public class SellerDTO
	{
        public User SellerID { get; set; }

        public SellerDTO()
		{
		}
	}
}

