using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Contracts.Notifications.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractsReminderTimerFunctionTests
    {
        [TestMethod]
        public void Run_DoesNotThrowException()
        {
            // Arrange
            var mockExampleService = new Mock<IContractNotificationService>();

            mockExampleService
                .Setup(e => e.RemindContractsReadyForSigning())
                .Verifiable();

            var function = new ContractsReminderTimerFunction(mockExampleService.Object);

            // Act
            Func<Task> act = async () => { await function.Run(null, null); };

            // Assert
            act.Should().NotThrow();
            mockExampleService.Verify();
        }
    }
}