using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Notifications.Services.Implementations;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Integration
{
    [TestClass]
    public class ExampleHttpFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task Example_ReturnsHelloResultFromExampleService()
        {
            // Arrange
            var expected = "Hello, world!";

            var exampleService = new ExampleService();

            var function = new ExampleHttpFunction(exampleService);

            // Act
            var actual = await function.Example(null, null);

            // Assert
            actual.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expected);
        }
    }
}