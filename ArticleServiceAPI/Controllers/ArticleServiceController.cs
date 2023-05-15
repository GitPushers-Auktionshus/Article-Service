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
using System.Security.Cryptography;
using MongoDB.Bson.Serialization.Attributes;
using System.IO.Pipelines;
using System.IO;


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

    private readonly string _imagePath;

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

        _imagePath = "/Users/jacobkaae/Downloads/";

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

            FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

            await _articleCollection.DeleteOneAsync(filter);

            return Ok(deleteArticle);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved deleteArticle: {ex.Message}");

            throw;
        }

    }

    //POST - Adds a new image
    [HttpPost("addArticleImage/{id}"), DisableRequestSizeLimit]
    public async Task<IActionResult> AddArticleImage(string id)
    {
        _logger.LogInformation("AddImage kaldt");

        List<Uri> images = new List<Uri>();

        try
        {
            foreach (var formFile in Request.Form.Files)
            {
                // Validate file type and size

                if (formFile.ContentType != "image/jpeg" && formFile.ContentType != "image/png")
                {
                    return BadRequest($"Invalid file type for file {formFile.FileName}. Only JPEG and PNG files are allowed.");
                }
                if (formFile.Length > 1048576) // 1MB
                {
                    return BadRequest($"File {formFile.FileName} is too large. Maximum file size is 1MB.");
                }
                if (formFile.Length > 0)
                {
                    var fileName = "image-" + Guid.NewGuid().ToString() + ".jpg";
                    var fullPath = _imagePath + Path.DirectorySeparatorChar + fileName;
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        formFile.CopyTo(stream);
                    }
                    var imageURI = new Uri(fileName, UriKind.RelativeOrAbsolute);
                    images.Add(imageURI);
                }
                else
                {
                    return BadRequest("Empty file submited.");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message);
            return StatusCode(500, $"Internal server error.");
        }

        var filter = Builders<Article>.Filter.Eq("ArticleID", id); // Find the document to update

        var update = Builders<Article>.Update.Push("Images", new Image
        {
            ImageID = ObjectId.GenerateNewId().ToString(),
            FileName = images[0].ToString(),
            ImagePath = _imagePath,
            Date = DateTime.UtcNow
        });

    // Insert the new element into the array

    var result = _articleCollection.UpdateOne(filter, update);

        Console.WriteLine($"{result.ModifiedCount} document(s) updated.");
        return Ok(images);
    }

    //DELETE - Removes an image
    [HttpPut("removeImage/{id}/{image_id}")]
    public async Task<IActionResult> RemoveImage(string id, string image_id)
    {
        _logger.LogInformation($"RemoveImage kaldt med Art. ID = {id} og Image ID = {image_id}");

        try
        {
            Article getArticle = new Article();

            getArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstOrDefaultAsync<Article>();

            Image removedImage = new Image();

            removedImage = getArticle.Images.Find(x => x.ImageID == image_id);

            getArticle.Images.Remove(removedImage);

            var filter = Builders<Article>.Filter.Eq("ArticleID", id); // Find the document to update

            var update = Builders<Article>.Update.Set("Images", getArticle.Images);

            await _articleCollection.UpdateOneAsync(filter, update);

            string fullPath = _imagePath + Path.DirectorySeparatorChar + removedImage.FileName;

            System.IO.File.Delete(fullPath);

            return Ok(getArticle);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved removeImage: {ex.Message}");

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
            FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

            var update = Builders<Article>.Update.Set("EstimatedPrice", price);

            await _articleCollection.UpdateOneAsync(filter, update);

            return Ok($"Article with ID: {id} updated with estimated price: {price}");

        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved updatePrice: {ex.Message}");

            throw;
        }
    }

    //PUT - Marks an article as sold
    [HttpPut("updateSold/{id}")]
    public async Task<IActionResult> UpdateSold(string id)
    {
        _logger.LogInformation($"updatePrice endpoint kaldt");

        try
        {
            Article updateArticle = new Article();

            updateArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

            FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);


            if (updateArticle.Sold == true)
            {
                var update = Builders<Article>.Update.Set("Sold", false);

                await _articleCollection.UpdateOneAsync(filter, update);

                return Ok($"Article with ID: {id} updated as sold: false");

            }
            else
            {
                var update = Builders<Article>.Update.Set("Sold", true);

                await _articleCollection.UpdateOneAsync(filter, update);

                return Ok($"Article with ID: {id} updated as sold: true");

            }

        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved updateSold: {ex.Message}");

            throw;
        }
    }
}
