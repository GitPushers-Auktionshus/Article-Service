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
    [HttpPost("addArticle")]
    public async Task<Article> AddArticle(ArticleDTO articleDTO)
    {
        return await _service.AddNewArticle(articleDTO);
    }


    //DELETE - Removes an article
    [HttpDelete("deleteArticle/{id}")]
    public async Task<Article> DeleteArticle(string id)
    {
        return await _service.DeleteArticleByID(id);
    }

    //GET - Gets an article by ID
    [HttpGet("getArticle/{id}")]
    public async Task<Article> GetArticle(string id)
    {
        return await _service.GetArticleByID(id);
    }

    //GET - Return a list of all articles
    [HttpGet("getAll")]
    public async Task<List<Article>> GetAll()
    {
        return await _service.GetAllArticles();
    }

    //POST - Adds a new image
    [HttpPut("addArticleImage/{id}"), DisableRequestSizeLimit]
    public List<Uri> AddArticleImage(string id)
    {
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

        return _service.AddImageToArticle(images, id);

    }

    //DELETE - Removes an image
    [HttpPut("removeImage/{id}/{image_id}")]
    public async Task<Article> RemoveImage(string id, string image_id)
    {
        return await _service.RemoveImageFromArticle(id, image_id);
    }

    //PUT - Updates estimated price of an article
    [HttpPut("updatePrice/{id}/{price}")]
    public async Task<string> UpdatePrice(string id, double price)
    {
        return await _service.UpdateEstimatedPrice(id, price);
    }

    //PUT - Marks an article as sold
    [HttpPut("updateSold/{id}")]
    public async Task<string> UpdateSold(string id)
    {
        return await _service.UpdateSoldStatus(id);
    }
}
