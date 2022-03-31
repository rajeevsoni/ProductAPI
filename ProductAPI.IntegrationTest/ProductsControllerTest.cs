using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.DependencyInjection;
using ProductAPI.Models;
using ProductAPI.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Net;
using Microsoft.AspNetCore.TestHost;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using Products.Data.DBContext;
using Microsoft.EntityFrameworkCore;

namespace ProductAPI.IntegrationTest
{
    [TestClass]
    public class ProductsControllerTest
    {
        private static TestContext _testContext;
        private static WebApplicationFactory<Program> _factory;
        private static string DefaultUserId { get; set; } = "1";

        [ClassInitialize]
        public static void ClassInit(TestContext testContext)
        {
            _testContext = testContext;
            _factory = new WebApplicationFactory<Program>();
            _factory = _factory.WithWebHostBuilder(builder =>
            {
                builder.UseSetting("https_port", "5001").UseEnvironment("Testing");
                builder.ConfigureServices(services =>
                {
                    services.AddScoped<IProductsService, MockProductsService>();
                    services.AddPooledDbContextFactory<ProductDBContext>(
                    o => o.UseInMemoryDatabase(Guid.NewGuid().ToString()));
                });
                builder.ConfigureTestServices(services =>
                {
                    services.Configure<TestAuthHandlerOptions>(options => options.DefaultUserId = DefaultUserId);
                    services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                        .AddScheme<TestAuthHandlerOptions, TestAuthHandler>(TestAuthHandler.AuthenticationScheme, options => { });
                });
            });

        }

        [TestMethod]
        public async Task GetProductTest_ForOk()
        {
            Console.WriteLine(_testContext.TestName);

            var productId = Guid.NewGuid().ToString();
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"v1/products/{productId}");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var jsonString = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<ProductResponse>(jsonString);
            Assert.IsNotNull(result);
            Assert.AreEqual(result.ProductId, Guid.Parse(productId));
        }

        [TestMethod]
        public async Task GetProductTest_ForBadRequest()
        {
            Console.WriteLine(_testContext.TestName);

            var productId = "00000000-0000-0000-0000-000000000000";
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"v1/products/{productId}");

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProductTest_ForNotFound()
        {
            Console.WriteLine(_testContext.TestName);

            var productId = "53697b70-8059-4324-a33f-f56e1b1209eb";
            var client = _factory.CreateClient();
            var response = await client.GetAsync($"v1/products/{productId}");

            Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateProductTest_ForInvalidProductNameLength()
        {
            Console.WriteLine(_testContext.TestName);

            var productId = "53697b70-8059-4324-a33f-f56e1b1209eb";
            var client = _factory.CreateClient();
            var productRequest = CreateProductRequest();
            var json = JsonConvert.SerializeObject(productRequest);
            var stringContent = new StringContent(json, UnicodeEncoding.UTF8, "application/json");
            var response = await client.PostAsync("v1/products", stringContent);

            Assert.AreEqual(HttpStatusCode.NotAcceptable, response.StatusCode);
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            _factory.Dispose();
        }

        private ProductRequest CreateProductRequest()
        {
            ProductRequest request = new ProductRequest();
            request.ProductId = Guid.NewGuid();
            request.ProductName = "abcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeabcdeaa";
            request.ProductYear = 2021;
            request.ChannelId = Products.Data.Entities.Channel.Store;
            request.SizeScaleId = Guid.NewGuid();
            request.Articles.Add(new ArticleRequest() { ArticleId = Guid.NewGuid(), ColourId = Guid.NewGuid() });
            return request;
        }
    }

    public class MockProductsService : IProductsService
    {
        public Task CreateProduct(ProductRequest productRequest, string creator)
        {
            return Task.CompletedTask;
        }

        public Task<ProductResponse> GetProduct(Guid productId)
        {
            ProductResponse productResponse = null;

            if (!productId.Equals(Guid.Parse("53697b70-8059-4324-a33f-f56e1b1209eb")))
            {
                productResponse = new ProductResponse();
                productResponse.ProductId = productId;
                productResponse.ProductName = "Jacket";
                productResponse.ProductCode = "2021123";
                productResponse.ChannelId = Products.Data.Entities.Channel.Store;
                productResponse.CreatedBy = "dummyUser";
                productResponse.CreateDate = DateTime.UtcNow.AddDays(-1);
                productResponse.SizeScaleId = Guid.NewGuid();
                productResponse.Articles.Add(new ArticleResponse()
                {
                    ArticleId = Guid.NewGuid(),
                    ArticleName = "Jacket - #0000",
                    ColourId = Guid.NewGuid(),
                    ColourCode = "#000000",
                    ColourName = "Black"
                });
            }
            return Task.FromResult(productResponse);

        }

    }
}