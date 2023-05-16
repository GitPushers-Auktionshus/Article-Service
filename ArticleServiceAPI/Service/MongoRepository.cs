using System;
using ArticleServiceAPI.Controllers;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;
using Image = ArticleServiceAPI.Model.Image;

namespace ArticleServiceAPI.Service
{
    // Inherits from our interface - can be changed to eg. a SQL database
    public class MongoRepository : IArticleRepository
    {
        private readonly ILogger<ArticleServiceController> _logger;
        private readonly IConfiguration _config;

        // Initializes enviroment variables
        private readonly string _connectionURI;

        private readonly string _usersDatabase;
        private readonly string _inventoryDatabase;

        private readonly string _userCollectionName;
        private readonly string _articleCollectionName;
        private readonly string _auctionHouseCollectionName;

        private readonly string _imagePath;

        // Initializes MongoDB database collection
        private readonly IMongoCollection<User> _userCollection;
        private readonly IMongoCollection<Auctionhouse> _auctionHouseCollection;
        private readonly IMongoCollection<Article> _articleCollection;

        public MongoRepository(ILogger<ArticleServiceController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnviromentVariables class 
                //_secret = config["Secret"] ?? "Secret missing";
                //_issuer = config["Issuer"] ?? "Issue'er missing";
                //_connectionURI = config["ConnectionURI"] ?? "ConnectionURI missing";

                //// Retrieves User database and collections
                //_usersDatabase = config["UsersDatabase"] ?? "Userdatabase missing";
                //_userCollectionName = config["UserCollection"] ?? "Usercollection name missing";
                //_auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "Auctionhousecollection name missing";

                //// Retrieves Inventory database and collection
                //_inventoryDatabase = config["InventoryDatabase"] ?? "Invetorydatabase missing";
                //_articleCollectionName = config["ArticleCollection"] ?? "Articlecollection name missing";

            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

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
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);

                // Sets MongoDB Database
                var userDatabase = mongoClient.GetDatabase(_usersDatabase);
                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);

                // Sets MongoDB Collection
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

        // Adds an image to a specified article
        public List<Uri> AddImageToArticle(List<Uri> images, string id)
        {
            try
            {
                _logger.LogInformation("AddImageToArticle kaldt");

                // Find the document to update
                var filter = Builders<Article>.Filter.Eq("ArticleID", id);

                // Pushes the image to the article
                var update = Builders<Article>.Update.Push("Images", new Image
                {
                    ImageID = ObjectId.GenerateNewId().ToString(),
                    FileName = images[0].ToString(),
                    ImagePath = _imagePath,
                    Date = DateTime.UtcNow
                });

                // Updates the document with the new image
                var result = _articleCollection.UpdateOne(filter, update);

                Console.WriteLine($"{result.ModifiedCount} document(s) updated.");

                return images;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved AddImageToArticle: {ex.Message}");

                throw;
            }
        }

        // Creates an URI for the image received in the POST HTTP request and adds it to the Image list that the function returns
        public List<Uri> ImageHandler(IFormFile formFile)
        {
            try
            {
                _logger.LogInformation("ImageHandler kaldt");

                List<Uri> images = new List<Uri>();

                // Validate file type and size
                if (formFile.ContentType != "image/jpeg" && formFile.ContentType != "image/png")
                {
                    throw new Exception($"Invalid file type for file {formFile.FileName}. Only JPEG and PNG files are allowed.");
                }
                if (formFile.Length > 1048576) // 1MB
                {
                    throw new Exception($"File {formFile.FileName} is too large. Maximum file size is 1MB.");
                }
                if (formFile.Length > 0)
                {
                    // Generates the URI
                    var fileName = "image-" + Guid.NewGuid().ToString() + ".jpg";
                    var fullPath = _imagePath + Path.DirectorySeparatorChar + fileName;
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        formFile.CopyTo(stream);
                    }
                    var imageURI = new Uri(fileName, UriKind.RelativeOrAbsolute);
                    images.Add(imageURI);

                    return images;
                }
                else
                {
                    throw new Exception("Empty file submited.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved ImageHandler: {ex.Message}");

                throw;
            }

        }

        // Adds new article document to the database
        public async Task<Article> AddNewArticle(ArticleDTO articleDTO)
        {
            try
            {
                _logger.LogInformation($"POST: addArticle kaldt, Name: {articleDTO.Name}, NoReserve: {articleDTO.NoReserve}, EstimatedPrice: {articleDTO.EstimatedPrice}, Description: {articleDTO.Description}, Category: {articleDTO.Category}, Sold: {articleDTO.Sold}, AuctionhouseID: {articleDTO.AuctionhouseID}, SellerID: {articleDTO.SellerID}, MinPrice: {articleDTO.MinPrice}, BuyerID: {articleDTO.BuyerID}");

                // Finds the seller, buyer and auctionhouse in our database using ID's from the articleDTO

                User buyer = new User();
                buyer = await _userCollection.Find(x => x.UserID == articleDTO.BuyerID).FirstOrDefaultAsync<User>();

                User seller = new User();
                seller = await _userCollection.Find(x => x.UserID == articleDTO.SellerID).FirstOrDefaultAsync<User>();

                Auctionhouse auctionhouse = new Auctionhouse();
                auctionhouse = await _auctionHouseCollection.Find(x => x.AuctionhouseID == articleDTO.AuctionhouseID).FirstOrDefaultAsync<Auctionhouse>();

                // Creates an article to be added to our database
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

                // Adds article to the article collection
                await _articleCollection.InsertOneAsync(addArticle);

                return addArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved addArticle: {ex.Message}");

                throw;
            }
        }

        // Deletes a specific article from the database using an ID
        public async Task<Article> DeleteArticleByID(string id)
        {
            try
            {
                _logger.LogInformation($"DELETE article kaldt med id: {id}");

                Article deleteArticle = new Article();

                // Finds the article to be deleted using an ID
                deleteArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

                // Deletes the article from article collection
                await _articleCollection.DeleteOneAsync(filter);

                return deleteArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved deleteArticle: {ex.Message}");

                throw;
            }
        }

        // Returns a list of all articles in the database
        public async Task<List<Article>> GetAllArticles()
        {
            _logger.LogInformation($"getAll endpoint kaldt");

            try
            {
                List<Article> allArticles = new List<Article>();

                // Adds all documents in the article collection to a list
                allArticles = await _articleCollection.Find(_ => true).ToListAsync<Article>();

                return allArticles;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved getAllArticles: {ex.Message}");

                throw;
            }
        }

        // Returns a specific article from the database using an ID
        public async Task<Article> GetArticleByID(string id)
        {
            try
            {
                _logger.LogInformation($"GetArticleByID kaldt med id: {id}");

                Article article = new Article();

                // Finds the article using an ID
                article = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved GetArticleByID: {ex.Message}");

                throw;
            }
        }

        // Deletes an image from a list of images for a specific article 
        public async Task<Article> RemoveImageFromArticle(string id, string image_id)
        {
            _logger.LogInformation($"RemoveImage kaldt med Art. ID = {id} og Image ID = {image_id}");

            try
            {
                Article getArticle = new Article();

                // Finds the article where the image is going to be deleted from 
                getArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstOrDefaultAsync<Article>();

                Image removedImage = new Image();

                // Finds the image in the list to be deleted
                removedImage = getArticle.Images.Find(x => x.ImageID == image_id);

                // Deletes the image
                getArticle.Images.Remove(removedImage);

                // Creates filter for a specific article using an ID
                var filter = Builders<Article>.Filter.Eq("ArticleID", id); 

                // Stages the update of document property "Images" without the removes image
                var update = Builders<Article>.Update.Set("Images", getArticle.Images);

                // Updates the article in our database
                await _articleCollection.UpdateOneAsync(filter, update);

                string fullPath = _imagePath + Path.DirectorySeparatorChar + removedImage.FileName;

                // Deletes the image file from our volume
                System.IO.File.Delete(fullPath);

                return getArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved removeImage: {ex.Message}");

                throw;
            }
        }

        // Updates the estimated price of an article
        public async Task<string> UpdateEstimatedPrice(string id, double price)
        {
            _logger.LogInformation($"updatePrice endpoint kaldt");

            try
            {
                // Creates filter for a specific article using an ID
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

                // Stages the update of document property "Estimated" with the new price
                var update = Builders<Article>.Update.Set("EstimatedPrice", price);

                // Updates the article in our database
                await _articleCollection.UpdateOneAsync(filter, update);

                return $"Article with ID: {id} updated with estimated price: {price}";

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved updatePrice: {ex.Message}");

                throw;
            }
        }

        // Updates the "Sold" status of an article 
        public async Task<string> UpdateSoldStatus(string id)
        {
            _logger.LogInformation($"updateSold endpoint kaldt");

            try
            {
                Article updateArticle = new Article();

                // Finds the document to be updated using the ID
                updateArticle = await _articleCollection.Find(x => x.ArticleID == id).FirstAsync<Article>();

                // Creates filter for a specific article using an ID
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", id);

                // Logic to determine whether to change the bool to true or false
                if (updateArticle.Sold == true)
                {
                    // Stages the update of document property "Sold" to false
                    var update = Builders<Article>.Update.Set("Sold", false);

                    // Updates the document
                    await _articleCollection.UpdateOneAsync(filter, update);

                    return $"Article with ID: {id} updated as sold: false";

                }
                else
                {
                    // Stages the update of document property "Sold" to true
                    var update = Builders<Article>.Update.Set("Sold", true);

                    // Updates the document
                    await _articleCollection.UpdateOneAsync(filter, update);

                    return $"Article with ID: {id} updated as sold: true";

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"Fejl ved updateSold: {ex.Message}");

                throw;
            }
        }
    }
}

