using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProductAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProductAPI.UnitTest.Utilities
{
    [TestClass]
    public class RandomGeneratorTest
    {
        [TestMethod]
        public async Task Validate_GetRandomAlphaNumericValue()
        {
            var result = RandomGenerator.GetRandomAlphaNumericValue(6);
            result.Length.Should().Be(6);
        }

        [TestMethod]
        public async Task Validate_GetRandomIntegerValue()
        {
            var result = RandomGenerator.GetRandomIntegerValue(3);
            result.ToString().Length.Should().Be(3);
        }
    }
}
