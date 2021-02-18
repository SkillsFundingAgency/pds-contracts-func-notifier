using FluentAssertions;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ServiceBusMessagingServiceTests
    {
        private const string TestMessageType = "TestMessageType";

        [TestMethod]
        public void SendAsBinaryXmlMessageAsync_SendsMessage()
        {
            // Arrange
            Message actual = null;
            var sender = Mock.Of<IMessageSender>();
            Mock.Get(sender)
                .Setup(p => p.SendAsync(It.IsAny<Message>()))
                .Returns(Task.CompletedTask)
                .Callback((Message msg) => { actual = msg; });

            ContractReminderMessage reminder = CreateContractReminderMessage();
            var properties = CreateMessageProperties();

            ServiceBusMessagingService sbMessagingService = new ServiceBusMessagingService(sender);

            // Act
            Func<Task> act = async () => await sbMessagingService.SendAsBinaryXmlMessageAsync(reminder, properties);

            // Assert
            act.Should().NotThrow();
            actual.UserProperties["messageType"].Should().Be(TestMessageType);
            Mock.Get(sender).VerifyAll();
        }

        [TestMethod]
        public void SendAsBinaryXmlMessageAsync_DoesNotSupressExceptions()
        {
            // Arrange
            var sender = Mock.Of<IMessageSender>();
            Mock.Get(sender)
                .Setup(p => p.SendAsync(It.IsAny<Message>()))
                .Throws<InvalidOperationException>();

            ContractReminderMessage reminder = CreateContractReminderMessage();
            var properties = CreateMessageProperties();

            ServiceBusMessagingService sbMessagingService = new ServiceBusMessagingService(sender);

            // Act
            Func<Task> act = async () => await sbMessagingService.SendAsBinaryXmlMessageAsync(reminder, properties);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            Mock.Get(sender).VerifyAll();
        }

        [TestMethod]
        public void SendAsBinaryXmlMessageAsync_OmittingProperties_DoesNotCauseException()
        {
            // Arrange
            var sender = Mock.Of<IMessageSender>();
            Mock.Get(sender)
                .Setup(p => p.SendAsync(It.IsAny<Message>()))
                .Throws<InvalidOperationException>();

            ContractReminderMessage reminder = CreateContractReminderMessage();

            ServiceBusMessagingService sbMessagingService = new ServiceBusMessagingService(sender);

            // Act
            Func<Task> act = async () => await sbMessagingService.SendAsBinaryXmlMessageAsync(reminder, null);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            Mock.Get(sender).VerifyAll();
        }

        private static ContractReminderMessage CreateContractReminderMessage()
        {
            return new ContractReminderMessage() { ContractId = 123 };
        }

        private static IDictionary<string, string> CreateMessageProperties()
        {
            return new Dictionary<string, string>()
            {
                { "messageType", TestMessageType }
            };
        }
    }
}
