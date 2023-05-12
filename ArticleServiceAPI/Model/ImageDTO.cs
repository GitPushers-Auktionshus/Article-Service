using System.Globalization;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ArticleServiceAPI.Model
{
    public class ImageDTO
    {
        [BsonId]
        [BsonElement(elementName: "_id")]
        public ObjectId ImageID { get; set; }

        public ImageDTO()
        {

        }

    }
}