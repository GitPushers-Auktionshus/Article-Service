using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ArticleServiceAPI.Controllers;
using ArticleServiceAPI.Model;
using ArticleServiceAPI.Service;
using Moq;
using Microsoft.AspNetCore.Mvc;

namespace ArticleServiceAPI.Test;

public class Tests
{

    private ILogger<ArticleServiceController> _logger = null!;
    private IConfiguration _configuration = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<ArticleServiceController>>().Object;

        var myConfiguration = new Dictionary<string, string?>
        {
            {"ArticleServiceBrokerHost", "http://testhost.local"}
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(myConfiguration)
            .Build();
    }

    // Tests the AddNewArticle method when a valid articleDTO is posted
    [Test]
    public async Task TestAddArticleEndpoint_valid_dto()
    {
        // Arrange
        var articleDTO = CreateArticleDTO("Test Article");
        var article = CreateArticle("Test Article ID");


        var stubRepo = new Mock<IArticleRepository>();

        stubRepo.Setup(svc => svc.AddNewArticle(articleDTO))
            .Returns(Task.FromResult<Article?>(article));

        var controller = new ArticleServiceController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddArticle(articleDTO);

        // Assert
        Assert.That(result, Is.TypeOf<CreatedAtActionResult>());
        Assert.That((result as CreatedAtActionResult)?.Value, Is.TypeOf<Article>());
    }

    // Tests the AddArticle method when an error is thrown in the AddNewArticle method from our repository
    [Test]
    public async Task TestAddArticleEndpoint_failure_posting()
    {
        // Arrange
        var articleDTO = CreateArticleDTO("Test Article");

        var stubRepo = new Mock<IArticleRepository>();

        stubRepo.Setup(svc => svc.AddNewArticle(articleDTO))
            .ThrowsAsync(new Exception());

        var controller = new ArticleServiceController(_logger, _configuration, stubRepo.Object);

        // Act        
        var result = await controller.AddArticle(articleDTO);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequestResult>());
    }

    /// <summary>
    /// Helper method for creating ArticleDTO instance.
    /// </summary>
    /// <param name="articleName"></param>
    /// <returns></returns>
    private ArticleDTO CreateArticleDTO(string articleName)
    {
        var articleDTO = new ArticleDTO()
        {
            Name = articleName,
            NoReserve = true,
            EstimatedPrice = 100,
            Description = "Test Description",
            Category = "TT",
            Sold = false,
            AuctionhouseID = "TestAuctionHouseID",
            SellerID = "Test SellerID",
            MinPrice = 50,
            BuyerID = "Test BuyerID"
        };

        return articleDTO;

    }

    /// <summary>
    /// Helper method for creating Article instances.
    /// </summary>
    /// <param name="articleID"></param>
    /// <returns></returns>
    private Article CreateArticle(string articleID)
    {
        var article = new Article()
        {
            ArticleID = "1",
            Name = "Test Name",
            NoReserve = true,
            EstimatedPrice = 100,
            Description = "Test Description",
            Images = new List<Image>(),
            Category = "TT",
            Sold = false,
            Auctionhouse = new Auctionhouse(),
            Seller = new User(),
            MinPrice = 50,
            Buyer = new User()
        };

        return article;

    }
}