using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractChangeNotificationsServiceBusFunctionTests
    {
        [TestMethod]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var dummyContract = new Contract();
            var mockExampleService = new Mock<IContractNotificationService>();
            mockExampleService
                .Setup(e => e.NotifyContractChanges(dummyContract))
                .Verifiable();

            var function = new ContractChangeNotificationsServiceBusFunction(mockExampleService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(dummyContract, null); };

            // Assert
            act.Should().NotThrow();
            mockExampleService.Verify();
        }
    }
}