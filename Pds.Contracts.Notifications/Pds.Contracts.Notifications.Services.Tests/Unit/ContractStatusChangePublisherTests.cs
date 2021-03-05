using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Tests.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SFS = Sfa.Sfs.Contracts.Messaging;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractStatusChangePublisherTests
    {
        private readonly Contract _dummyContract;
        private readonly IServiceBusMessagingService _mockServiceBusMessagingService;
        private readonly IAuditService _mockAuditService;
        private readonly ILogger<IContractStatusChangePublisher> _mockLogger;

        public ContractStatusChangePublisherTests()
        {
            _dummyContract = new Models.Contract { Id = 1, ContractNumber = "Test", ContractVersion = 1, Ukprn = 123456 };
            _mockServiceBusMessagingService = Mock.Of<IServiceBusMessagingService>(MockBehavior.Strict);
            _mockAuditService = Mock.Of<IAuditService>(MockBehavior.Strict);
            _mockLogger = Mock.Of<ILogger<Interfaces.IContractStatusChangePublisher>>(MockBehavior.Strict);

            SetupNotifyAsyncPrivateMethodCalls();
            Mock.Get(_mockLogger)
                .Setup(l => l.Log(LogLevel.Information, 0, It.IsAny<It.IsAnyType>(), null, It.IsAny<Func<It.IsAnyType, Exception, string>>()));
        }

        [TestMethod]
        public async Task NotifyContractApprovedTestAsync()
        {
            // Arrange
            _dummyContract.Status = ContractStatus.Approved;
            var expectedMessage = new SFS.ContractApprovedMessage { ContractId = _dummyContract.Id };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractApprovedAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockServiceBusMessagingService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractApprovedAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage);
        }

        [TestMethod]
        public async Task NotifyContractChangesAreReadyForReviewTestAsync()
        {
            // Arrange
            _dummyContract.Status = ContractStatus.Approved;
            var expectedMessage = new SFS.ContractReadyToReviewEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractChangesAreReadyForReviewAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockServiceBusMessagingService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractChangesAreReadyForReviewAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage);
        }

        [TestMethod]
        public async Task NotifyContractIsReadyToSignTestAsync()
        {
            // Arrange
            _dummyContract.Status = ContractStatus.PublishedToProvider;
            var expectedMessage = new SFS.ContractReadyToSignEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractIsReadyToSignAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{_dummyContract.Status}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockServiceBusMessagingService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractIsReadyToSignAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage);
        }

        [DataTestMethod]
        [DataRow(ContractStatus.WithdrawnByAgency)]
        [DataRow(ContractStatus.WithdrawnByProvider)]
        public async Task NotifyContractWithdrawnTestAsync(ContractStatus withdrawnStatus)
        {
            // Arrange
            _dummyContract.Status = withdrawnStatus;
            var expectedMessage = new SFS.ContractWithdrawnEmailMessage { ContractNumber = _dummyContract.ContractNumber, Ukprn = _dummyContract.Ukprn, VersionNumber = _dummyContract.ContractVersion };
            var expectedAuditMessage = new Audit.Api.Client.Models.Audit
            {
                Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                Message = $"A contract notification has been forwarded to the [MessageProcessor] by [{nameof(ContractStatusChangePublisher.NotifyContractWithdrawnAsync)}] for contract [{_dummyContract.ContractNumber}] Version [{_dummyContract.ContractVersion}] with the status of [{withdrawnStatus}]",
                Severity = 0,
                Ukprn = _dummyContract.Ukprn,
                User = "System_NotificationFunction"
            };

            var publisher = new ContractStatusChangePublisher(_mockServiceBusMessagingService, _mockAuditService, _mockLogger);

            // Act
            await publisher.NotifyContractWithdrawnAsync(_dummyContract);

            // Assert
            VerifyAllMocks(expectedMessage, expectedAuditMessage);
        }

        private void SetupNotifyAsyncPrivateMethodCalls()
        {
            Mock.Get(_mockServiceBusMessagingService)
                .Setup(sbs => sbs.SendAsBinaryXmlMessageAsync(It.IsAny<It.IsAnyType>(), It.IsAny<IDictionary<string, string>>()))
                .Returns(Task.CompletedTask);

            Mock.Get(_mockAuditService)
                .Setup(a => a.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask);
        }

        private void VerifyAllMocks<T>(T message, Audit.Api.Client.Models.Audit auditMessage)
        {
            Mock.Get(_mockLogger).VerifyAll();
            Mock.Get(_mockServiceBusMessagingService)
                .Verify(sbs => sbs.SendAsBinaryXmlMessageAsync(ItIs.EquivalentTo(message), It.Is<IDictionary<string, string>>(m => m["messageType"] == typeof(T).FullName)), Times.Once);

            Mock.Get(_mockAuditService)
                .Verify(a => a.AuditAsync(ItIs.EquivalentTo(auditMessage)), Times.Once);
        }
    }
}