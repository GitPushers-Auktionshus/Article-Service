using System;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArticleServiceAPI.Service
{
	public interface IArticleRepository
	{
		public Task<ArticleDTO> AddNewArticle(ArticleDTO articleDTO);

        public Task<Article> DeleteArticleByID(string id);

        public List<Uri> ImageHandler(IFormFile formFile);

        public List<Uri> AddImageToArticle(List<Uri> images, string id);

        public Task<Article> RemoveImageFromArticle(string id, string image_id);

        public Task<List<Article>> GetAllArticles();

        public Task<Article> GetArticleByID(string id);

        public Task<string> UpdateEstimatedPrice(string id, double price);

        public Task<string> UpdateSoldStatus(string id);
    }

}

