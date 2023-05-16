using System;
using ArticleServiceAPI.Controllers;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ArticleServiceAPI.Service
{
    public class MongoRepository : IArticleRepository
    {
        private readonly ILogger<ArticleServiceController> _logger;
        private readonly IConfiguration _config;

        private readonly string _connectionURI;

        private readonly string _usersDatabase;
        private readonly string _inventoryDatabase;

        private readonly string _userCollectionName;
        private readonly string _articleCollectionName;
        private readonly string _auctionHouseCollectionName;

        private readonly string _imagePath;

        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Auctionhouse> _auctionHouseCollection;
        private readonly IMongoCollection<Article> _articleCollection;

        private readonly MongoClient mongoClient;




        public MongoRepository(ILogger<ArticleServiceController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            _connectionURI = "mongodb://admin:1234@localhost:27018/";

            // User database and collections
            _usersDatabase = "Users";
            _userCollectionName = "user";
            _auctionHouseCollectionName = "auctionhouse";

            // Inventory database and collection
            _inventoryDatabase = "Inventory";
            _articleCollectionName = "article";

            _imagePath = "/Users/jacobkaae/Downloads/";

            _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}");
            _logger.LogInformation($"ArticleService Database and Collections: Userdatabase: {_usersDatabase}, Inventorydatabase: {_inventoryDatabase}, UserCollection: {_userCollectionName}, AuctionHouseCollection: {_auctionHouseCollectionName}, ArticleCollection: {_articleCollectionName}");

            // Client
            mongoClient = new MongoClient(_connectionURI);
        }

        public Task<IActionResult> AddImageToArticle(object x)
        {

            throw new NotImplementedException();
        }

        public async Task<Article> AddNewArticle(ArticleDTO articleDTO)
        {
            try
            {
                // Databases
                var userDatabase = mongoClient.GetDatabase(_usersDatabase);
                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);

                // Collections
                IMongoCollection<User> _userCollection = userDatabase.GetCollection<User>(_userCollectionName);
                IMongoCollection<Article> _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);
                IMongoCollection<Auctionhouse> _auctionHouseCollection = userDatabase.GetCollection<Auctionhouse>(_auctionHouseCollectionName);

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved AddNewArticle forbindelse til mongoDB: {ex.Message}");

                throw;
            }

            try
            {
                _logger.LogInformation($"POST: addArticle kaldt, Name: {articleDTO.Name}, NoReserve: {articleDTO.NoReserve}, EstimatedPrice: {articleDTO.EstimatedPrice}, Description: {articleDTO.Description}, Category: {articleDTO.Category}, Sold: {articleDTO.Sold}, AuctionhouseID: {articleDTO.AuctionhouseID}, SellerID: {articleDTO.SellerID}, MinPrice: {articleDTO.MinPrice}, BuyerID: {articleDTO.BuyerID}");

                User buyer = new User();
                buyer = await _userCollection.Find(x => x.UserID == articleDTO.BuyerID).FirstOrDefaultAsync<User>();

                User seller = new User();
                seller = await _userCollection.Find(x => x.UserID == articleDTO.SellerID).FirstOrDefaultAsync<User>();

                Auctionhouse auctionhouse = new Auctionhouse();
                auctionhouse = await _auctionHouseCollection.Find(x => x.AuctionhouseID == articleDTO.AuctionhouseID).FirstOrDefaultAsync<Auctionhouse>();

                Article addArticle = new Article
                {
                    ArticleID = ObjectId.GenerateNewId().ToString(),
                    Name = articleDTO.Name,
                    NoReserve = articleDTO.NoReserve,
                    EstimatedPrice = articleDTO.EstimatedPrice,
                    Description = articleDTO.Description,
                    Images = new List<Image>(),
                    Category = articleDTO.Category,
                    Sold = articleDTO.Sold,
                    Auctionhouse = auctionhouse,
                    Seller = seller,
                    MinPrice = articleDTO.MinPrice,
                    Buyer = buyer
                };


                await _articleCollection.InsertOneAsync(addArticle);

                return addArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved addArticle: {ex.Message}");

                throw;
            }
        }

        public Task<IActionResult> DeleteArticleByID(object x)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> GetAllArticles(object x)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> RemoveImageFromArticle(object x)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateEstimatedPrice(object x)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateSoldStatus(object x)
        {
            throw new NotImplementedException();
        }
    }
}

