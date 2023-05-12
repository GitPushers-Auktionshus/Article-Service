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

        _secret = config["Secret"] ?? "Secret missing";
        _issuer = config["Issuer"] ?? "Issue'er missing";
        _connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";

        // User database and collections
        _usersDatabase = config["UsersDatabase"] ?? "Userdatabase missing";
        _userCollectionName = config["UserCollection"] ?? "Usercollection name missing";
        _auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "Auctionhousecollection name missing";

        // Inventory database and collection
        _inventoryDatabase = config["InventoryDatabase"] ?? "Invetorydatabase missing";
        _articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

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
            _auctionHouseCollection = inventoryDatabase.GetCollection<Auctionhouse>(_auctionHouseCollectionName);

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
        _logger.LogInformation($"POST: addArticle kaldt, Name: {articleDTO.Name}, NoReserve: {articleDTO.NoReserve}, EstimatedPrice: {articleDTO.EstimatedPrice}, Description: {articleDTO.Description}, Category: {articleDTO.Category}, Sold: {articleDTO.Sold}, AuctionhouseID: {articleDTO.AuctionhouseID}, SellerID: {articleDTO.SellerID}, MinPrice: {articleDTO.MinPrice}, BuyerID: {articleDTO.BuyerID}");

        User buyer = new User();
        buyer = await _userCollection.Find(x => x.UserID == articleDTO.BuyerID).FirstOrDefaultAsync<User>();

        User seller = new User();
        seller = await _userCollection.Find(x => x.UserID == articleDTO.SellerID).FirstOrDefaultAsync<User>();

        Auctionhouse auctionhouse = new Auctionhouse();
        auctionhouse = await _auctionHouseCollection.Find(x => x.AuctionhouseID == articleDTO.AuctionhouseID).FirstOrDefaultAsync<Auctionhouse>();


        Article article = new Article
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


        await _articleCollection.InsertOneAsync(article);

        return Ok(article);
    }


    //DELETE - Removes an article


    //POST - Adds a new image


    //DELETE - Removes an image


    //GET - Gets a specific article by ID


    //GET - Lists information for a specifik article


    //PUT - Updates estimated price of an article


    //DELETE - Removes estimated price of an article


    //PUT - Marks an article as sold

}
