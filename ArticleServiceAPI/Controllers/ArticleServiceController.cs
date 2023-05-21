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
using ArticleServiceAPI.Service;

namespace ArticleServiceAPI.Controllers;

[ApiController]
[Route("[controller]")]
public class ArticleServiceController : ControllerBase
{
    private readonly ILogger<ArticleServiceController> _logger;

    private readonly IConfiguration _config;

    private readonly IArticleRepository _service;


    public ArticleServiceController(ILogger<ArticleServiceController> logger, IConfiguration config, IArticleRepository service)
    {
        _logger = logger;
        _config = config;
        _service = service;
    }

    //POST - Adds a new article
    [Authorize]
    [HttpPost("addArticle")]
    public async Task<Article> AddArticle(ArticleDTO articleDTO)
    {
        _logger.LogInformation($"[POST] addArticle endpoint reached");

        return await _service.AddNewArticle(articleDTO);
    }


    //DELETE - Removes an article
    [Authorize]
    [HttpDelete("deleteArticle/{articleId}")]
    public async Task<Article> DeleteArticle(string articleId)
    {
        _logger.LogInformation($"[DELETE] deleteArticle/{articleId} endpoint reached");

        return await _service.DeleteArticleByID(articleId);
    }

    //GET - Gets an article by ID
    [HttpGet("getArticle/{articleId}")]
    public async Task<Article> GetArticle(string articleId)
    {
        _logger.LogInformation($"[GET] getArticle/{articleId} endpoint reached");

        return await _service.GetArticleByID(articleId);
    }

    //GET - Return a list of all articles
    [HttpGet("getAll")]
    public async Task<List<Article>> GetAll()
    {
        _logger.LogInformation($"[GET] getAll endpoint reached");

        return await _service.GetAllArticles();
    }

    //POST - Adds a new image
    [Authorize]
    [HttpPut("addArticleImage/{articleId}"), DisableRequestSizeLimit]
    public List<Uri> AddArticleImage(string articleId)
    {
        _logger.LogInformation($"[PUT] addArticleImage/{articleId} endpoint reached");

        List<Uri> images = new List<Uri>();

        try
        {
            foreach (var formFile in Request.Form.Files)
            {
                images = _service.ImageHandler(formFile);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Fejl ved AddArticleImage i controller: {ex.Message}");

            throw;
        }

        return _service.AddImageToArticle(images, articleId);

    }

    //DELETE - Removes an image
    [Authorize]
    [HttpPut("removeImage/{articleId}/{imageId}")]
    public async Task<Article> RemoveImage(string articleId, string imageId)
    {
        _logger.LogInformation($"[PUT] removeImage/{articleId}/{imageId} endpoint reached");

        return await _service.RemoveImageFromArticle(articleId, imageId);
    }

    //PUT - Updates estimated price of an article
    [Authorize]
    [HttpPut("updatePrice/{articleId}/{price}")]
    public async Task<string> UpdatePrice(string articleId, double price)
    {
        _logger.LogInformation($"[PUT] updatePrice/{articleId}/{price} endpoint reached");

        return await _service.UpdateEstimatedPrice(articleId, price);
    }

    //PUT - Marks an article as sold
    [Authorize]
    [HttpPut("updateSold/{articleId}")]
    public async Task<string> UpdateSold(string articleId)
    {
        _logger.LogInformation($"[PUT] updateSold/{articleId} endpoint reached");

        return await _service.UpdateSoldStatus(articleId);
    }
}
