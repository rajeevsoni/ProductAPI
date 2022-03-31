using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductAPI.Filters;
using ProductAPI.Models;
using ProductAPI.Services.Interfaces;

namespace ProductAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("v1/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly ILogger<ProductsController> _logger;
        private readonly IProductsService _productsService;

        public ProductsController(ILogger<ProductsController> logger, IProductsService productsService)
        {
            _logger = logger;
            _productsService = productsService;
        }

        [HttpGet]
        [Route("{productId}")]
        public async Task<IActionResult> Get([FromRoute] Guid productId)
        {
            _logger.LogInformation($"Get Request initiated for productId: {productId} ");
            if(productId == Guid.Empty)
            {
                return BadRequest("Invaalid productId");
            }

            ProductResponse productResponse = await _productsService.GetProduct(productId);
            if(productResponse == null)
            {
                return NotFound();
            }

            _logger.LogInformation($"Get request Successful for productId: {productId} ");
            return Ok(productResponse);
        }

        [HttpPost]
        [ProductRequestValidation]
        public async Task<IActionResult> Create([FromBody] ProductRequest productRequest)
        {
            _logger.LogInformation($"Post Request initiated for productId: {productRequest.ProductId} ");
            var creator = HttpContext?.User?.Claims?.FirstOrDefault(x => x.Type == "name")?.Value ?? String.Empty;
            await _productsService.CreateProduct(productRequest, creator);
            _logger.LogInformation($"Post request Successful for productId: {productRequest.ProductId} ");
            return Created("/products","Product Successfuly Created");
        }
    }
}