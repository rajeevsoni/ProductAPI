using ProductAPI.Models;

namespace ProductAPI.Services.Interfaces
{
    public interface IProductsService
    {
        Task<ProductResponse> GetProduct(Guid productId);
        Task CreateProduct(ProductRequest productRequest, string creator);
    }
}
