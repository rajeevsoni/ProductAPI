using Azure.Storage.Queues;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using ProductAPI.Models;
using ProductAPI.Models.Constants;
using ProductAPI.Services.Interfaces;
using ProductAPI.Utilities;
using Products.Data.DBContext;
using Products.Data.Entities;
using System.Diagnostics.Eventing.Reader;
using System.Text.Json.Serialization;

namespace ProductAPI.Services
{
    public class ProductsService : IProductsService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ProductsService> _logger;
        private readonly IDbContextFactory<ProductDBContext> _dbContextFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IMemoryCache _memoryCache;
        private readonly IQueueService _queueService;

        public ProductsService(IConfiguration configuration, ILogger<ProductsService> logger, IDbContextFactory<ProductDBContext> dbContextFactory, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache, IQueueService queueService)
        {
            _configuration = configuration;
            _logger = logger;
            _dbContextFactory = dbContextFactory;
            _httpClientFactory = httpClientFactory;
            _memoryCache = memoryCache;
            _queueService = queueService;
        }

        public async Task<ProductResponse> GetProduct(Guid productId)
        {
            _logger.LogInformation($"Fetching product: {productId} from database");
            if (!_memoryCache.TryGetValue(productId, out ProductResponse cachedProductResponse))
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    Product product = await dbContext.Products.Include(x => x.Articles).SingleOrDefaultAsync(x => x.ProductId == productId);
                    ProductResponse productResponse = null;
                    if (product != null)
                    {
                        productResponse = product.ConvertToProductResponse();
                        await PopulateColourInfo(productResponse);
                        await PopulateSizeInfo(productResponse);
                        _memoryCache.Set(productId, productResponse, TimeSpan.FromMinutes(30));
                    }
                    return productResponse;
                }
            }
            _logger.LogInformation($"Successfully Fetched product: {productId} from database");
            return cachedProductResponse;
        }

        public async Task CreateProduct(ProductRequest productRequest, string creator)
        {
            _logger.LogInformation($"Adding product: {productRequest.ProductId} to database");
            if (!_memoryCache.TryGetValue(productRequest.ProductId, out ProductResponse cachedProductResponse))
            {
                using (var dbContext = _dbContextFactory.CreateDbContext())
                {
                    if (!await dbContext.Products.AnyAsync(x => x.ProductId == productRequest.ProductId))
                    {
                        string nextSequenceNumber = string.Empty;
                        if (productRequest.ChannelId == Channel.All)
                        {
                            NextSequence nextSequence = await dbContext.NextSequence.SingleOrDefaultAsync();
                            nextSequenceNumber = nextSequence.NextSequenceNumber.ToString();
                            nextSequence.NextSequenceNumber++;
                        }
                        string productCode = GenerateProductCode(productRequest, nextSequenceNumber);

                        Product product = productRequest.ConvertToProduct(productCode, creator);
                        dbContext.Products.Add(product);
                        await dbContext.SaveChangesAsync();
                        await PushProductCreatedEventToQueue(product);
                    }
                }
            }
            _logger.LogInformation($"Successfully added product: {productRequest.ProductId} to database");
        }

        private string GenerateProductCode(ProductRequest productRequest, string nextSequenceNumber)
        {
            string productCode = string.Empty;

            switch (productRequest.ChannelId)
            {
                case Channel.Store:
                    productCode = $"{productRequest.ProductYear}{RandomGenerator.GetRandomIntegerValue(3)}";
                    break;
                case Channel.Online:
                    productCode = $"{RandomGenerator.GetRandomAlphaNumericValue(6)}";
                    break;
                case Channel.All:
                    productCode = nextSequenceNumber;
                    break;
            }
            return productCode;
        }

        private async Task PopulateColourInfo(ProductResponse productResponse)
        {
            if(!_memoryCache.TryGetValue(HttpClientConstants.ColorInfos, out IList<ColourResponse> cachedColourInfoList))
            {
                bool useMockData = _configuration.GetValue<bool>("UseMockAPIData");
                if (useMockData)
                {
                    cachedColourInfoList = MockDataProvider.GetColourInfos();
                }
                else
                {
                    using (var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.ColourAPI))
                    {
                        var response = await httpClient.GetAsync("");
                        var jsonString = await response.Content.ReadAsStringAsync();
                        cachedColourInfoList = JsonConvert.DeserializeObject<IList<ColourResponse>>(jsonString);
                    }
                }
                _memoryCache.Set(HttpClientConstants.ColorInfos, cachedColourInfoList, TimeSpan.FromMinutes(30));
            }

            foreach (ArticleResponse articleResponse in productResponse.Articles)
            {
                articleResponse.ColourCode = cachedColourInfoList?.FirstOrDefault(x => x.ColourId == articleResponse.ColourId)?.ColourCode ?? string.Empty;
                articleResponse.ColourName = cachedColourInfoList?.FirstOrDefault(x => x.ColourId == articleResponse.ColourId)?.ColourName ?? string.Empty;
                articleResponse.ArticleName = $"{productResponse.ProductName} - {articleResponse.ColourCode}";
            }
        }

        private async Task PopulateSizeInfo(ProductResponse productResponse)
        {
            if (!_memoryCache.TryGetValue(productResponse.SizeScaleId, out IList<SizeResponse> cachedSizeInfoList))
            {
                bool useMockData = _configuration.GetValue<bool>("UseMockAPIData");
                if (useMockData)
                {
                    cachedSizeInfoList = MockDataProvider.GetSizeInfos();
                }
                else
                {
                    using (var httpClient = _httpClientFactory.CreateClient(HttpClientConstants.SizeAPI))
                    {
                        var response = await httpClient.GetAsync($"/{productResponse.SizeScaleId}");
                        var jsonString = await response.Content.ReadAsStringAsync();
                        cachedSizeInfoList = JsonConvert.DeserializeObject<IList<SizeResponse>>(jsonString);
                        _memoryCache.Set(productResponse.SizeScaleId, cachedSizeInfoList, TimeSpan.FromMinutes(30));
                    }
                }
            }
            productResponse.Sizes = cachedSizeInfoList;
        }

        private async Task PushProductCreatedEventToQueue(Product product)
        {
            if (_configuration.GetValue<bool>("EnableNotifier"))
            {
                ProductCreatedEvent productCreatedEvent = new ProductCreatedEvent();
                productCreatedEvent.ProductId = product.ProductId.ToString();
                productCreatedEvent.ProductName = product.ProductName;
                productCreatedEvent.RecipientEmail = "dummy@gmail.com";
                await _queueService.EnqueueProductCreatedEvent(productCreatedEvent);
            }
        }
    }
}
