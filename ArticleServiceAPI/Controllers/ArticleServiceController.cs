using System.Threading;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Text;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Authorization;

namespace ArticleServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleServiceController : ControllerBase
{
    private readonly ILogger<ArticleServiceController> _logger;

    private readonly string _secret;
    private readonly string _issuer;
    private readonly string _connectionURI;

    private readonly string _usersDatabase;
    private readonly string _inventoryDatabase;

    private readonly string _userCollectionName;
    private readonly string _articleCollectionName;
    private readonly string _auctionHouseCollectionName;


    private readonly IMongoCollection<User> _userCollection;
    private readonly IMongoCollection<Auctionhouse> _auctionHouseCollection;
    private readonly IMongoCollection<Article> _articleCollection;
    private readonly IConfiguration _config;

    public ArticleServiceController(ILogger<ArticleServiceController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        //_secret = config["Secret"] ?? "Secret missing";
        //_issuer = config["Issuer"] ?? "Issue'er missing";
        //_connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";

        //// User database and collections
        //_usersDatabase = config["UsersDatabase"] ?? "Userdatabase missing";
        //_userCollectionName = config["UserCollection"] ?? "Usercollection name missing";
        //_auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "Auctionhousecollection name missing";

        //// Inventory database and collection
        //_inventoryDatabase = config["InventoryDatabase"] ?? "Invetorydatabase missing";
        //_articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";



        _connectionURI = "mongodb://admin:1234@localhost:27018/";

        // User database and collections
        _usersDatabase = "Users";
        _userCollectionName = "user";
        _auctionHouseCollectionName = "auctionhouse";

        // Inventory database and collection
        _inventoryDatabase = "Inventory";
        _articleCollectionName = "article";

        _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}");
        _logger.LogInformation($"ArticleService Database and Collections: Userdatabase: {_usersDatabase}, Inventorydatabase: {_inventoryDatabase}, UserCollection: {_userCollectionName}, AuctionHouseCollection: {_auctionHouseCollectionName}, ArticleCollection: {_articleCollectionName}");

        try
        {
            // Client
            var mongoClient = new MongoClient(_connectionURI);

            // Databases
            var userDatabase = mongoClient.GetDatabase(_usersDatabase);
            var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);

            // Collections
            _userCollection = userDatabase.GetCollection<User>(_userCollectionName);
            _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);
            _auctionHouseCollection = userDatabase.GetCollection<Auctionhouse>(_auctionHouseCollectionName);

        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved oprettelse af forbindelse: {ex.Message}");
            throw;
        }
    }

    //POST - Adds a new article
    [HttpPost("addArticle")]
    public async Task<IActionResult> AddArticle(ArticleDTO articleDTO)
    {
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

            return Ok(addArticle);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved addArticle: {ex.Message}");

            throw;
        }

    }


    //DELETE - Removes an article
    [HttpDelete("deleteArticle/{id}")]
    public async Task<IActionResult> DeleteArticle(string id)
    {
        try
        {
            _logger.LogInformation($"DELETE article kaldt med id: {id}");

            Article deleteArticle = new Article();

            deleteArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

            if (DeleteArticle != null)
            {
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

                await _articleCollection.DeleteOneAsync(filter);

                return Ok(deleteArticle);
            }
            else
            {
                throw new Exception("Article not found");
            }


        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved deleteArticle: {ex.Message}");

            throw;
        }

    }

    //POST - Adds a new image


    //DELETE - Removes an image


    //GET - Gets a specific article by ID
    [HttpGet("getArticle/{id}")]
    public async Task<IActionResult> GetAuctionHouse(string id)
    {
        _logger.LogInformation($"getArticle kaldt med ID= {id}");

        try
        {
            Article getArticle = new Article();

            getArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstOrDefaultAsync<Article>();

            return Ok(getArticle);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved addArticle: {ex.Message}");

            throw;
        }

    }


    //GET - Return a list of all articles
    [HttpGet("getAll")]
    public async Task<IActionResult> GetAllArticles()
    {
        _logger.LogInformation($"getAll endpoint kaldt");

        try
        {
            List<Article> allArticles = new List<Article>();
                
            allArticles = await _articleCollection.Find(_ => true).ToListAsync<Article>();

            return Ok(allArticles);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved getAllArticles: {ex.Message}");

            throw;
        }

    }

    //PUT - Updates estimated price of an article
    [HttpPut("updatePrice/{id}/{price}")]
    public async Task<IActionResult> UpdateEstimatedPrice(string id, double price)
    {
        _logger.LogInformation($"updatePrice endpoint kaldt");

        try
        {
            Article updateArticle = new Article();

            updateArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

            if (updateArticle != null)
            {
                updateArticle.EstimatedPrice = price;

                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

                await _articleCollection.ReplaceOneAsync(filter, updateArticle);

                return Ok($"Article with ID: {id} updated with estimated price: {price}");
            }
            else
            {
                throw new Exception("Article not found");
            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved updatePrice: {ex.Message}");

            throw;
        }

    }

    //DELETE - Removes estimated price of an article


    //PUT - Marks an article as sold

}
