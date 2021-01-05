using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Pds.Contracts.Notifications.Services.Implementations;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Integration
{
    [TestClass]
    public class ExampleServiceBusFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var exampleService = new ExampleService();

            var function = new ExampleServiceBusFunction(exampleService);

            // Act
            Func<Task> act = async () => { await function.Run(null, null); };

            // Assert
            act.Should().NotThrow();
        }
    }
}