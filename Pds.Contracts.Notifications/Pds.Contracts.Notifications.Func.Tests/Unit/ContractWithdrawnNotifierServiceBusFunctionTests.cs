﻿using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractWithdrawnNotifierServiceBusFunctionTests
    {
        [TestMethod]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var mockService = new Mock<IContractStatusChangePublisher>(MockBehavior.Strict);

            mockService
                .Setup(e => e.NotifyContractWithdrawnAsync(It.IsAny<Contract>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            var function = new ContractWithdrawnNotifierServiceBusFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.RunAsync(new Contract(), null); };

            // Assert
            act.Should().NotThrow();
            mockService.Verify();
        }

        [TestMethod]
        public void Run_ThrowsArgumentNullException()
        {
            // Arrange
            var mockService = new Mock<IContractStatusChangePublisher>(MockBehavior.Strict);
            var function = new ContractWithdrawnNotifierServiceBusFunction(mockService.Object);

            // Act
            Func<Task> act = async () => { await function.RunAsync(null, null); };

            // Assert
            act.Should().Throw<ArgumentNullException>();
            mockService.VerifyNoOtherCalls();
        }
    }
}