using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using ProductAPI.Models;
using Products.Data.DBContext;
using System.Text.RegularExpressions;

namespace ProductAPI.Filters
{
    public class ProductRequestValidationAttribute : TypeFilterAttribute
    {
        public ProductRequestValidationAttribute() : base(typeof(ProductRequestValidatorAttribute))
        {
        }

        private class ProductRequestValidatorAttribute : ActionFilterAttribute
        {
            private readonly IDbContextFactory<ProductDBContext> _dbContextFactory;

            public ProductRequestValidatorAttribute(IDbContextFactory<ProductDBContext> dbContextFactory)
            {
                _dbContextFactory = dbContextFactory;
            }

            public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
            {
                if (!IsValidProductName(context))
                {
                    var response = new ObjectResult("Product Name should be more than 0 and less than 100 characters");
                    response.StatusCode = 406;
                    context.Result = response;
                }
                else if (!IsValidProductDate(context))
                {
                    context.Result = new BadRequestObjectResult("Invalid product request. ProductDate is not valid. it should be between 2000-2999");
                }
                else if(!await IsValidArticleIds(context))
                {
                    context.Result = new BadRequestObjectResult("Invalid product request. One of the Provided ArticleIds are already linked to existing Products in System");
                }
                else
                {
                    await next();
                }
            }

            private bool IsValidProductDate(ActionExecutingContext context)
            {
                ProductRequest productRequest = context.ActionArguments["productRequest"] as ProductRequest;
                Regex rgx = new Regex(@"^(2)\d{3}$");
                return rgx.IsMatch(productRequest.ProductYear.ToString());
            }

            private bool IsValidProductName(ActionExecutingContext context)
            {
                ProductRequest productRequest = context.ActionArguments["productRequest"] as ProductRequest;
                return productRequest.ProductName.Length > 0 && productRequest.ProductName.Length <= 100;
            }

            private async Task<bool> IsValidArticleIds(ActionExecutingContext context)
            {
                ProductRequest productRequest = context.ActionArguments["productRequest"] as ProductRequest;
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    List<Guid> newArticleIds = productRequest.Articles.Select(x => x.ArticleId).ToList();
                    List<Guid> allArticleIds = await dbContext.Articles.Select(x => x.ArticleId).ToListAsync();
                    IEnumerable<Guid> remaining = allArticleIds.Except(newArticleIds);
                    return remaining.Count() == allArticleIds.Count;
                    
                }
            }
        }
    }
}
