using System;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArticleServiceAPI.Service
{
	public interface IArticleRepository
	{
        /// <summary>
        /// Adds a new article to the database
        /// </summary>
        /// <param name="articleDTO"></param>
        /// <returns>The article that's been created</returns>
        public Task<Article> AddNewArticle(ArticleDTO articleDTO);


        /// <summary>
        /// Deletes a selected article from the database based on the provided article ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>The article that's been deleted</returns>
        public Task<Article> DeleteArticleByID(string id);

        /// <summary>
        /// Creates an URI for an image provided in a .JPEG og .IMG format
        /// </summary>
        /// <param name="formFile"></param>
        /// <returns>A list containing an URI for the image</returns>
        public List<Uri> ImageHandler(IFormFile formFile);

        /// <summary>
        /// Adds an image to an article. Requires the image's URI and the article ID 
        /// </summary>
        /// <param name="images"></param>
        /// <param name="id"></param>
        /// <returns>A list containing an URI for the image</returns>
        public List<Uri> AddImageToArticle(List<Uri> images, string id);

        /// <summary>
        /// Removes an image from an article. Requires the ID and the image to be deleted and the ID of the article that it's going to be deleted from 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="image_id"></param>
        /// <returns>The article from which the image was removed</returns>
        public Task<Article> RemoveImageFromArticle(string id, string image_id);

        /// <summary>
        /// Get a list of all articles in the database
        /// </summary>
        /// <returns>A list containing all articles</returns>
        public Task<List<Article>> GetAllArticles();

        /// <summary>
        /// Get a specific article based on a provided article ID
        /// </summary>
        /// <param name="id"></param>
        /// <returns>An article with a matching ID</returns>
        public Task<Article> GetArticleByID(string id);

        /// <summary>
        /// Updates the estimated price of an article. Requires an article ID and the new estimated price 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="price"></param>
        /// <returns>The updated estimated price of the article</returns>
        public Task<string> UpdateEstimatedPrice(string id, double price);

        /// <summary>
        /// Updates the sold status of an article. If it's status=true it changed to status=false and vice versa
        /// </summary>
        /// <param name="id"></param>
        /// <returns>A string describing the new sold status of the article</returns>
        public Task<string> UpdateSoldStatus(string id);
    }

}

