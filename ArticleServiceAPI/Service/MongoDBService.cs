using System;
using ArticleServiceAPI.Controllers;
using ArticleServiceAPI.Model;
using AuthServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using static System.Net.Mime.MediaTypeNames;
using Image = ArticleServiceAPI.Model.Image;


namespace ArticleServiceAPI.Service
{
    // Inherits from our interface - can be changed to eg. a SQL database
    public class MongoDBService : IArticleRepository
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

        public MongoDBService(ILogger<ArticleServiceController> logger, IConfiguration config, EnvVariables vaultSecrets)
        {
            _logger = logger;
            _config = config;

            try
            {
                // Retrieves enviroment variables from program.cs, from injected EnvVariables class
                _connectionURI = vaultSecrets.dictionary["ConnectionURI"];

                // Retrieves User database and collections
                _usersDatabase = config["UsersDatabase"] ?? "UsersDatabase missing";
                _userCollectionName = config["UserCollection"] ?? "UserCollection name missing";
                _auctionHouseCollectionName = config["AuctionHouseCollection"] ?? "AuctionHouseCollection name missing";

                // Retrieves Inventory database and collection
                _inventoryDatabase = config["InventoryDatabase"] ?? "InvetoryDatabase missing";
                _articleCollectionName = config["ArticleCollection"] ?? "ArticleCollection name missing";

                // Retrieve Image Path to store images
                _imagePath = config["ImagePath"] ?? "ImagePath missing";

                _logger.LogInformation($"ArticleService secrets: ConnectionURI: {_connectionURI}");
                _logger.LogInformation($"ImagePath: {_imagePath}");
                _logger.LogInformation($"ArticleService Database and Collections: Userdatabase: {_usersDatabase}, Inventorydatabase: {_inventoryDatabase}, UserCollection: {_userCollectionName}, AuctionHouseCollection: {_auctionHouseCollectionName}, ArticleCollection: {_articleCollectionName}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error retrieving enviroment variables");

                throw;
            }

            try
            {
                // Sets MongoDB client
                var mongoClient = new MongoClient(_connectionURI);
                _logger.LogInformation($"[*] CONNECTION_URI: {_connectionURI}");


                // Sets MongoDB Database
                var userDatabase = mongoClient.GetDatabase(_usersDatabase);
                _logger.LogInformation($"[*] DATABASE: {_usersDatabase}");

                var inventoryDatabase = mongoClient.GetDatabase(_inventoryDatabase);
                _logger.LogInformation($"[*] DATABASE: {_inventoryDatabase}");


                // Sets MongoDB Collection
                _userCollection = userDatabase.GetCollection<User>(_userCollectionName);
                _logger.LogInformation($"[*] COLLECTION: {_userCollectionName}");

                _articleCollection = inventoryDatabase.GetCollection<Article>(_articleCollectionName);
                _logger.LogInformation($"[*] COLLECTION: {_articleCollectionName}");

                _auctionHouseCollection = userDatabase.GetCollection<Auctionhouse>(_auctionHouseCollectionName);
                _logger.LogInformation($"[*] COLLECTION: {_auctionHouseCollectionName}");


            }
            catch (Exception ex)
            {
                _logger.LogError($"Error trying to connect to database: {ex.Message}");

                throw;
            }

        }

        // Adds an image to a specified article
        public List<Uri> AddImageToArticle(List<Uri> images, string articleId)
        {
            try
            {
                _logger.LogInformation("[*] AddImageToArticle(List<Uri> images, string articleId) called: Adding image to the article");

                // Find the document to update
                var filter = Builders<Article>.Filter.Eq("ArticleID", articleId);

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
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Creates an URI for the image received in the POST HTTP request and adds it to the Image list that the function returns
        public List<Uri> ImageHandler(IFormFile formFile)
        {
            try
            {
                _logger.LogInformation("[*] ImageHandler(<IFormFile formfile) called: Creating an URI for the image and adding it to the list");

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
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }

        }

        // Adds new article document to the database
        public async Task<Article> AddNewArticle(ArticleDTO articleDTO)
        {
            try
            {
                _logger.LogInformation($"[*] AddNewArticle(ArticleDTO articleDTO) called: Adding a new Article document to the database\nName: {articleDTO.Name}\nNoReserve: {articleDTO.NoReserve}\nEstimatedPrice: {articleDTO.EstimatedPrice}\nDescription: {articleDTO.Description}\nCategory: {articleDTO.Category}\nSold: {articleDTO.Sold}\nAuctionhouseID: {articleDTO.AuctionhouseID}\nSellerID: {articleDTO.SellerID}\nMinPrice: {articleDTO.MinPrice}\nBuyerID: {articleDTO.BuyerID}");
                
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
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Deletes a specific article from the database using an ID
        public async Task<Article> DeleteArticleByID(string articleId)
        {
            try
            {
                _logger.LogInformation($"[*] DeleteArticleByID(string articleId) called: Deleting article with articleId {articleId}");

                Article deleteArticle = new Article();

                // Finds the article to be deleted using an ID
                deleteArticle = await _articleCollection.Find(x => x.ArticleID == articleId).FirstAsync<Article>();

                if (deleteArticle.Images != null)
                {
                    // Deletes all images from the article in the volume
                    foreach (var image in deleteArticle.Images)
                    {
                        string fullPath = _imagePath + Path.DirectorySeparatorChar + image.FileName;

                        // Deletes the image file from our volume
                        System.IO.File.Delete(fullPath);
                    }
                }

                // Creates filter for a specific article using an ID
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", articleId);

                // Deletes the article from article collection
                await _articleCollection.DeleteOneAsync(filter);

                return deleteArticle;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Returns a list of all articles in the database
        public async Task<List<Article>> GetAllArticles()
        {
            _logger.LogInformation($"[*] GetAllArticles() called: Fetching all articles in the database");

            try
            {
                List<Article> allArticles = new List<Article>();

                // Adds all documents in the article collection to a list
                allArticles = await _articleCollection.Find(_ => true).ToListAsync<Article>();

                return allArticles;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Returns a specific article from the database using an ID
        public async Task<Article> GetArticleByID(string articleId)
        {
            try
            {
                _logger.LogInformation($"[*] GetArticleByID(string articleId) called: Fetching article with articleId: {articleId}");

                Article article = new Article();

                // Finds the article using an ID
                article = await _articleCollection.Find(x => x.ArticleID == articleId).FirstAsync<Article>();

                return article;
            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Deletes an image from a list of images for a specific article 
        public async Task<Article> RemoveImageFromArticle(string articleId, string imageId)
        {
            _logger.LogInformation($"[*] RemoveImageFromArticle(string articleId, string imageId) called: Deleting an image with imageId {imageId} from the article with articleId {articleId}");

            try
            {
                Article getArticle = new Article();

                // Finds the article where the image is going to be deleted from 
                getArticle = await _articleCollection.Find(x => x.ArticleID == articleId).FirstOrDefaultAsync<Article>();

                Image removedImage = new Image();

                // Finds the image in the list to be deleted
                removedImage = getArticle.Images.Find(x => x.ImageID == imageId);

                // Deletes the image
                getArticle.Images.Remove(removedImage);

                // Creates filter for a specific article using an ID
                var filter = Builders<Article>.Filter.Eq("ArticleID", articleId); 

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
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Updates the estimated price of an article
        public async Task<string> UpdateEstimatedPrice(string articleId, double price)
        {
            _logger.LogInformation($"[*] UpdateEstimatedPrice(string articleId, double price) called: Updating the estimated price of the article with articleId {articleId}");

            try
            {
                // Creates filter for a specific article using an ID
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", articleId);

                // Stages the update of document property "Estimated" with the new price
                var update = Builders<Article>.Update.Set("EstimatedPrice", price);

                // Updates the article in our database
                await _articleCollection.UpdateOneAsync(filter, update);

                return $"Article with ID: {articleId} updated with estimated price: {price}";

            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }

        // Updates the "Sold" status of an article 
        public async Task<string> UpdateSoldStatus(string articleId)
        {
            _logger.LogInformation($"[*] UpdateSoldStatus(string articleId) called: Updating the status of the article {articleId}");

            try
            {
                Article updateArticle = new Article();

                // Finds the document to be updated using the ID
                updateArticle = await _articleCollection.Find(x => x.ArticleID == articleId).FirstAsync<Article>();

                // Creates filter for a specific article using an ID
                FilterDefinition<Article> filter = Builders<Article>.Filter.Eq("ArticleID", articleId);

                // Logic to determine whether to change the bool to true or false
                if (updateArticle.Sold == true)
                {
                    // Stages the update of document property "Sold" to false
                    var update = Builders<Article>.Update.Set("Sold", false);

                    // Updates the document
                    await _articleCollection.UpdateOneAsync(filter, update);

                    return $"Article with ID: {articleId} updated as sold: false";

                }
                else
                {
                    // Stages the update of document property "Sold" to true
                    var update = Builders<Article>.Update.Set("Sold", true);

                    // Updates the document
                    await _articleCollection.UpdateOneAsync(filter, update);

                    return $"Article with ID: {articleId} updated as sold: true";

                }

            }
            catch (Exception ex)
            {
                _logger.LogError($"EXCEPTION CAUGHT: {ex.Message}");

                throw;
            }
        }
    }
}

