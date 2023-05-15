using System;
using Microsoft.AspNetCore.Mvc;

namespace ArticleServiceAPI.Service
{
	public interface IArticleRepository
	{
		public Task<IActionResult> AddArticle(Object x);

        public Task<IActionResult> DeleteArticle(Object x);


    }

    public class MongoRepository : IArticleRepository
    {
        public Task<IActionResult> AddArticle(object x)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> DeleteArticle(object x)
        {
            throw new NotImplementedException();
        }
    }
}

