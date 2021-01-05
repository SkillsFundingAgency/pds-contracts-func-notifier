﻿using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit
{
    [TestClass]
    public class ExampleTimerFunctionTests
    {
        [TestMethod, TestCategory("Integration")]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var mockExampleService = new Mock<IExampleService>();

            mockExampleService
                .Setup(e => e.Hello())
                .ReturnsAsync(string.Empty)
                .Verifiable();

            var function = new ExampleTimerFunction(mockExampleService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(null, null); };

            // Assert
            act.Should().NotThrow();
            mockExampleService.Verify();
        }
    }
}