using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Models;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ServiceBusMessagingServiceTests
    {
        [TestMethod]
        public void SendMessageAsync_SendsMessage()
        {
            // Arrange
            var sender = Mock.Of<IMessageSender>();
            Mock.Get(sender)
                .Setup(p => p.SendAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask);

            Contract contract = CreateContract();

            ServiceBusMessagingService sbMessagingService = new ServiceBusMessagingService(sender);

            // Act
            Func<Task> act = async () => await sbMessagingService.SendMessageAsync<Contract>(contract);

            // Assert
            act.Should().NotThrow();
            Mock.Get(sender).VerifyAll();
        }

        [TestMethod]
        public void SendMessageAsync_DoesNotSupressExceptions()
        {
            // Arrange
            var sender = Mock.Of<IMessageSender>();
            Mock.Get(sender)
                .Setup(p => p.SendAsync(It.IsAny<Message>()))
                .Throws<InvalidOperationException>();

            Contract contract = CreateContract();

            ServiceBusMessagingService sbMessagingService = new ServiceBusMessagingService(sender);

            // Act
            Func<Task> act = async () => await sbMessagingService.SendMessageAsync<Contract>(contract);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            Mock.Get(sender).VerifyAll();
        }

        private static Contract CreateContract()
        {
            return new Contract() { ContractNumber = "123", ContractVersion = 234, Id = 345, Ukprn = 456 };
        }
    }
}
