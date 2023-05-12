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
    private readonly string _databaseName;
    private readonly string _collectionName;


    private readonly IMongoCollection<Article> _articles;
    private readonly IConfiguration _config;

    public ArticleServiceController(ILogger<ArticleServiceController> logger, IConfiguration config)
    {
        _logger = logger;
        _config = config;

        _secret = config["Secret"] ?? "Secret missing";
        _issuer = config["Issuer"] ?? "Issue'er missing";
        _connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";
        _databaseName = config["DatabaseName"] ?? "DatabaseName missing";
        _collectionName = config["CollectionName"] ?? "CollectionName missing";



        _logger.LogInformation($"ArticleService variables: Secret: {_secret}, Issuer: {_issuer}, ConnectionURI: {_connectionURI}, DatabaseName: {_databaseName}, CollectionName: {_collectionName}");

        try
        {
            // Client
            var mongoClient = new MongoClient(_connectionURI);
            _logger.LogInformation($"[*] CONNECTION_URI: {_connectionURI}");

            // Database
            var database = mongoClient.GetDatabase(_databaseName);
            _logger.LogInformation($"[*] DATABASE: {_databaseName}");

            // Collection
            _articles = database.GetCollection<Article>(_collectionName);
            _logger.LogInformation($"[*] COLLECTION: {_collectionName}");

        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved oprettelse af forbindelse: {ex.Message}");
            throw;
        }
    }

    //POST - Adds a new article
    [HttpPost("addArticle")]
    public async Task AddArticle(ArticleDTO article)
    {
        //_logger.LogInformation("\nMetoden: AddArticle(Article article) kaldt klokken {DT}", DateTime.UtcNow.ToLongTimeString());

        Article effekt = new Article
        {
            ArticleID = article.ObjectId.GenerateNewId().ToString(),
            Name = article.Name,
            NoReserve = article.NoReserve,
            EstimatedPrice = article.EstimatedPrice,
            Description = article.Description,
            Category = article.Category,
            Sold = article.Sold,
            Auctionhouse = article.Auctionhouse.AuctionhouseID,
            MinPrice = article.MinPrice
        };

        await _articles.InsertOneAsync(effekt);

        return;
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
