using System;
using ArticleServiceAPI.Model;
using Microsoft.AspNetCore.Mvc;

namespace ArticleServiceAPI.Service
{
	public interface IArticleRepository
	{
		public Task<Article> AddNewArticle(ArticleDTO articleDTO);

        public Task<IActionResult> DeleteArticleByID(Object x);

        public Task<IActionResult> AddImageToArticle(Object x);

        public Task<IActionResult> RemoveImageFromArticle(Object x);

        public Task<IActionResult> GetAllArticles(Object x);

        public Task<Article> GetArticleByID(string id);

        public Task<IActionResult> UpdateEstimatedPrice(Object x);

        public Task<IActionResult> UpdateSoldStatus(Object x);
    }

}

