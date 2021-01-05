using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit
{
    [TestClass]
    public class ExampleHttpFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public async Task Example_ReturnsHelloResultFromExampleService()
        {
            // Arrange
            var expected = "Hello, world!";

            var mockExampleService = new Mock<IExampleService>();

            mockExampleService
                .Setup(e => e.Hello())
                .ReturnsAsync(expected)
                .Verifiable();

            var function = new ExampleHttpFunction(mockExampleService.Object);

            // Act
            var actual = await function.Example(null, null);

            // Assert
            actual.Should().BeOfType<OkObjectResult>()
                .Which.Value.Should().Be(expected);
            mockExampleService.Verify();
        }
    }
}