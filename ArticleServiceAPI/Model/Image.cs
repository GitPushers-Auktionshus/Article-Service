using System.Globalization;
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;

namespace ArticleServiceAPI.Model
{
    public class Image
    {
        [BsonId]
        public string ImageID { get; set; }
        public string FileName { get; set; }
        public string ImagePath { get; set; }
        [BsonElement]
        public DateTime? Date { get; set; } = DateTime.UtcNow;

        public Image()
        {

        }

    }
}
