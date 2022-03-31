using ProductAPI.Models;
using Products.Data.Entities;

namespace ProductAPI.Utilities
{
    public static class MapperExtensions
    {
        public static Product ConvertToProduct(this ProductRequest productRequest, string productCode, string createdBy)
        {
            Product product = new Product();
            product.ProductId = productRequest.ProductId;
            product.ProductName = productRequest.ProductName;
            product.ProductYear = productRequest.ProductYear;
            product.ChannelId = productRequest.ChannelId;
            product.ProductCode = productCode;
            product.CreateDate = DateTime.UtcNow;
            product.CreatedBy = createdBy;
            product.SizeScaleId = productRequest.SizeScaleId;
            foreach(ArticleRequest articleRequest in productRequest.Articles)
            {
                product.Articles.Add(articleRequest.ConvertToArticle());
            }
            return product;
        }

        public static Article ConvertToArticle(this ArticleRequest articleRequest)
        {
            Article article = new Article();
            article.ArticleId = articleRequest.ArticleId;
            article.ColourId = articleRequest.ColourId;
            return article;
        }

        public static ProductResponse ConvertToProductResponse(this Product product)
        {
            ProductResponse productResponse = new ProductResponse();
            productResponse.ProductId = product.ProductId;
            productResponse.ProductName = product.ProductName;
            productResponse.ProductCode = product.ProductCode;
            productResponse.CreateDate = product.CreateDate;
            productResponse.CreatedBy = product.CreatedBy;
            productResponse.ChannelId = product.ChannelId;
            productResponse.SizeScaleId = product.SizeScaleId;
            foreach(Article article in product.Articles)
            {
                productResponse.Articles.Add(ConvertToArticleResponse(article));
            }
            return productResponse;
        }

        public static ArticleResponse ConvertToArticleResponse(this Article article)
        {
            ArticleResponse articleResponse = new ArticleResponse();
            articleResponse.ArticleId = article.ArticleId;
            articleResponse.ColourId = article.ColourId;
            return articleResponse;
        }
    }
}
