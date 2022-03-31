using Castle.Core.Configuration;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using ProductAPI.Controllers;
using ProductAPI.Models;
using ProductAPI.Services.Interfaces;
using ProductAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductAPI.UnitTest.Controllers
{
    [TestClass]
    public class ProductsControllerTest
    {
        private Mock<IProductsService> _productService;
        private Mock<ILogger<ProductsController>> _logger;
        private ProductsController _productsController;

        [TestInitialize]
        public void TestInitialize()
        {
            _productService = new Mock<IProductsService>();
            _logger = new Mock<ILogger<ProductsController>>();
            _productsController = new ProductsController(_logger.Object, _productService.Object);
        }

        [TestMethod]
        public async Task ValidateGetProduct_OkResult()
        {
            _productService.Setup(x => x.GetProduct(It.IsAny<Guid>())).ReturnsAsync(new ProductResponse());
            var response = await _productsController.Get(Guid.NewGuid());
            response.Should().BeOfType<OkObjectResult>();
        }

        [TestMethod]
        public async Task ValidateGetProduct_NotFoundResult()
        {
            ProductResponse productResponse = null;
            _productService.Setup(x => x.GetProduct(It.IsAny<Guid>())).ReturnsAsync(productResponse);
            var response = await _productsController.Get(Guid.NewGuid());
            response.Should().BeOfType<NotFoundResult>();
        }

        [TestMethod]
        public async Task ValidateCreateProduct_Success()
        {
            ProductRequest productRequest = new ProductRequest();
            _productService.Setup(x => x.CreateProduct(It.IsAny<ProductRequest>(),It.IsAny<string>())).Returns(Task.CompletedTask);
            var response = await _productsController.Create(productRequest);
            response.Should().BeOfType<CreatedResult>();
        }
    }
}
