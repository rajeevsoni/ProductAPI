using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProductAPI.Models;
using ProductAPI.Services;
using ProductAPI.Services.Interfaces;
using ProductAPI.Utilities;
using Products.Data.DBContext;
using Products.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace ProductAPI.UnitTest.Services
{
    [TestClass]
    public class ProductsServiceTest
    {
        private Mock<IDbContextFactory<ProductDBContext>> _dbContextFactory;
        private Mock<ILogger<ProductsService>> _logger;
        private Mock<IHttpClientFactory> _httpClientFactory;
        private Mock<IQueueService> _queueService;
        private IConfiguration _configuration;
        private IMemoryCache _memoryCache;
        private ProductsService _productsService;
        private Dictionary<string,string> inMemorySettings = new Dictionary<string, string> {{ "UseMockAPIData", "true"}, { "EnableNotifier", "false" } };


        [TestInitialize]
        public void TestInitialize()
        {
            _dbContextFactory = new Mock<IDbContextFactory<ProductDBContext>>();
            _logger = new Mock<ILogger<ProductsService>>();
            _httpClientFactory = new Mock<IHttpClientFactory>();
            _queueService = new Mock<IQueueService>();
            _configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _memoryCache = new MemoryCache(new MemoryCacheOptions {}); ;
            _productsService = new ProductsService(_configuration, _logger.Object,
                _dbContextFactory.Object,
                _httpClientFactory.Object,
                _memoryCache,
                _queueService.Object);
        }

        [TestMethod]
        public async Task ValidateCreateProduct_ForOnlineChannel()
        {
            //setup
            _dbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new ProductDBContext(new DbContextOptionsBuilder<ProductDBContext>()
                .UseInMemoryDatabase("InMemoryProductDb").Options));

            ProductRequest productRequest = CreateRequestForProduct(Channel.Online);
            
            //execute
            await _productsService.CreateProduct(productRequest,"dummyUser");

            //validate
            using (var dbContext = _dbContextFactory.Object.CreateDbContext())
            {
                Product product = await dbContext.Products.Where(x => x.ProductId == productRequest.ProductId).SingleOrDefaultAsync();
                product.Should().NotBeNull();
                product.ProductCode.Length.Should().Be(6);
            }
        }

        [TestMethod]
        public async Task ValidateCreateProduct_ForStoreChannel()
        {
            //setup
            _dbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new ProductDBContext(new DbContextOptionsBuilder<ProductDBContext>()
                .UseInMemoryDatabase("InMemoryProductDb").Options));

            ProductRequest productRequest = CreateRequestForProduct(Channel.Store);
            
            //execute
            await _productsService.CreateProduct(productRequest, "dummyUser");

            //validate
            using (var dbContext = _dbContextFactory.Object.CreateDbContext())
            {
                Product product = await dbContext.Products.Where(x => x.ProductId == productRequest.ProductId).SingleOrDefaultAsync();
                product.Should().NotBeNull();
                product.ProductCode.Length.Should().Be(7);
            }
        }

        [TestMethod]
        public async Task ValidateCreateProduct_ForAllChannel()
        {
            //setup
            _dbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new ProductDBContext(new DbContextOptionsBuilder<ProductDBContext>()
                .UseInMemoryDatabase("InMemoryProductDb").Options));
            SetupNextSequenceInDb();
            ProductRequest productRequest = CreateRequestForProduct(Channel.All);

            //execute
            await _productsService.CreateProduct(productRequest, "dummyUser");

            //validate
            using (var dbContext = _dbContextFactory.Object.CreateDbContext())
            {
                Product product = await dbContext.Products.Where(x => x.ProductId == productRequest.ProductId).SingleOrDefaultAsync();
                product.Should().NotBeNull();
                int.TryParse(product.ProductCode,out int val);
                val.Should().BeGreaterThanOrEqualTo(10000000);
            }
        }

        [TestMethod]
        public async Task ValidateGetProduct()
        {
            //setup
            _dbContextFactory.Setup(f => f.CreateDbContext())
                .Returns(() => new ProductDBContext(new DbContextOptionsBuilder<ProductDBContext>()
                .UseInMemoryDatabase("InMemoryProductDb").Options));

            ProductRequest productRequest = CreateRequestForProduct(Channel.Store);
            await _productsService.CreateProduct(productRequest, "dummyUser");

            //execute
            var productResponse = await _productsService.GetProduct(productRequest.ProductId);

            //validate
            productResponse.Should().NotBeNull();
            productResponse.ProductId.Should().Be(productRequest.ProductId);
            productResponse.Articles.Should().HaveCount(1);
            productResponse.Articles[0].ColourName.Should().NotBeEmpty();
            productResponse.Articles[0].ArticleName.Should().Be($"{productResponse.ProductName} - {productResponse.Articles[0].ColourCode}");
        }

        private ProductRequest CreateRequestForProduct(Channel channelId)
        {
            ProductRequest request = new ProductRequest();
            request.ProductId = Guid.NewGuid();
            request.ProductName = "Jacket";
            request.ProductYear = 2021;
            request.ChannelId = channelId;
            request.SizeScaleId = Guid.NewGuid();
            ArticleRequest articleRequest = new ArticleRequest();
            articleRequest.ArticleId = Guid.NewGuid();
            articleRequest.ColourId = Guid.Parse("fb8ce6b2-fa0a-4bfd-89ed-81affa0fe859");
            request.Articles.Add(articleRequest);
            return request;
        }

        private void SetupNextSequenceInDb()
        {
            using (var dbContext = _dbContextFactory.Object.CreateDbContext())
            {
                NextSequence next = new NextSequence();
                next.NextSequenceNumber = 10000000;
                dbContext.NextSequence.Add(next);
                dbContext.SaveChanges();
            }
        }
    }
}
