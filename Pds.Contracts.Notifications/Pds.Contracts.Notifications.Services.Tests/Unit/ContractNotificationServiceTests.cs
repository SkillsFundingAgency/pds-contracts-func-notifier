using FluentAssertions;
using FluentAssertions.Primitives;
using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using RichardSzalay.MockHttp;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class ContractNotificationServiceTests
    {
        private const string TestBaseAddress = "http://test-api-endpoint";

        private const string TestContractGetEndpoint = "/test/get/contract";
        private const string TestContractPatchEndpoint = "/test/patch/contract";
        private const string TestPatchEndpoint = "/test/patch/operation";

        private const string TestFakeAccessToken = "AccessToken";
        private const string TestServiceBusExceptionMessage = "Test Exception Error";

        private readonly MockHttpMessageHandler _mockHttp
            = new MockHttpMessageHandler();

        private readonly ILoggerAdapter<ContractNotificationService> _contractsLogger
            = Mock.Of<ILoggerAdapter<ContractNotificationService>>(MockBehavior.Strict);

        private readonly IAuditService _auditService
            = Mock.Of<IAuditService>(MockBehavior.Strict);

        private readonly IServiceBusMessagingService _sbMessagingService
            = Mock.Of<IServiceBusMessagingService>(MockBehavior.Strict);

        private readonly Contract _contract
            = new Contract() { ContractNumber = "123", ContractVersion = 234, Id = 345, Ukprn = 456 };

        private readonly ContractReminderMessage _contractReminderMessage
            = new ContractReminderMessage { ContractId = 345 };

        #region GetContracts

        [TestMethod]
        public async Task GetContracts_Returns_ExpectedDataAsync()
        {
            // Arrange
            var queryParams = CreateQueryParameters(2);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var contracts = CreateContractReminders();
            var stringContent = new StringContent(JsonConvert.SerializeObject(contracts));

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + config.ContractReminderEndpoint.Endpoint)
                .WithQueryString(queryParams)
                .Respond(HttpStatusCode.OK, stringContent);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            var result = await service.GetOverdueContracts();

            // Assert
            result.Should().BeEquivalentTo(contracts);
            VerifyAll();
        }

        [TestMethod]
        public void GetContracts_OnFailure_ThrowsException()
        {
            // Arrange
            var expectedStatusCode = HttpStatusCode.InternalServerError;
            var queryParams = CreateQueryParameters(2);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var stringContent = new StringContent(string.Empty);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + config.ContractReminderEndpoint.Endpoint)
                .WithQueryString(queryParams)
                .Respond(expectedStatusCode, stringContent);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.GetOverdueContracts();

            // Assert
            act.Should().Throw<ApiGeneralException>()
                .Where(p => p.ResponseStatusCode == expectedStatusCode);
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
        }

        [TestMethod]
        [DataRow(2)]
        [DataRow(3)]
        [DataRow(4)]
        [DataRow(5)]
        public void GetContracts_Verify_QueryString_IncludesAllConfiguredValues(int numOfParameters)
        {
            // Arrange
            var queryParams = CreateQueryParameters(numOfParameters);
            var config = CreateContractsDataApiConfiguration(queryParams);

            var stringContent = new StringContent(string.Empty);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Get, config.ApiBaseAddress + config.ContractReminderEndpoint.Endpoint)
                .WithQueryString(queryParams)
                .Respond(HttpStatusCode.OK, stringContent);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.GetOverdueContracts();

            // Assert
            act.Should().NotThrow();
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
        }

        #endregion


        #region Queue Reminder Email Tests

        [TestMethod]
        public void QueueContractEmailReminderMessage_DoesNotThrowExceptions()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_sbMessagingService)
                .Setup(p => p.SendMessageAsync(_contractReminderMessage))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            act.Should().NotThrow();
            VerifyAll();
        }


        [TestMethod]
        public void QueueContractEmailReminderMessage_FailureToQueue_AllowsExceptionToBubble()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_sbMessagingService)
                .Setup(p => p.SendMessageAsync(_contractReminderMessage))
                .Throws(new ServiceBusTimeoutException(TestServiceBusExceptionMessage))
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            act
                .Should().Throw<ServiceBusTimeoutException>()
                .WithMessage(TestServiceBusExceptionMessage);

            VerifyAll();
        }

        [TestMethod]
        public void QueueContractEmailReminderMessage_FailureToLogAudit_AllowsExceptionToBubble()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_sbMessagingService)
                .Setup(p => p.SendMessageAsync(_contractReminderMessage))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            act.Should().Throw<InvalidOperationException>();

            VerifyAll();
        }

        [TestMethod]
        public void QueueContractEmailReminderMessage_VerifyAuditEntry()
        {
            // Arrange
            var expectedAudit = new Audit.Api.Client.Models.Audit()
            {
                Severity = 0,
                Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                Ukprn = _contract.Ukprn,
                Message = $"Email reminder has been queued for contract with Id [{_contract.Id}].",
                User = ContractNotificationService.Audit_User_System
            };

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_sbMessagingService)
                .Setup(p => p.SendMessageAsync(_contractReminderMessage))
                .Returns(Task.CompletedTask)
                .Verifiable();

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.QueueContractEmailReminderMessage(_contract);

            // Assert
            act.Should().NotThrow();
            VerifyAll();
        }

        #endregion


        #region NotifyContractReminderSent Tests

        [TestMethod]
        public void NotifyContractReminderSent_DoesNotThrowExceptions()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + config.ContractReminderPatchEndpoint.Endpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.OK);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            act.Should().NotThrow();
            VerifyAll();
        }

        [TestMethod]
        public void NotifyContractReminderSent_OnContractApiFailure_ExceptionIsRaised()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()));

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + config.ContractReminderPatchEndpoint.Endpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.InternalServerError, new StringContent(string.Empty));

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            act.Should().Throw<ApiGeneralException>();
            VerifyAll();
        }

        [TestMethod]
        public void NotifyContractReminderSent_OnAuditApiFailure_ExceptionIsRaised()
        {
            // Arrange
            string updateContent = GetUpdateRequestContents();

            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());

            _mockHttp
                .Expect(HttpMethod.Patch, TestBaseAddress + config.ContractReminderPatchEndpoint.Endpoint)
                .WithContent(updateContent)
                .Respond(HttpStatusCode.OK);

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()));

            Mock.Get(_auditService)
                .Setup(p => p.AuditAsync(It.IsAny<Audit.Api.Client.Models.Audit>()))
                .Throws<InvalidOperationException>()
                .Verifiable();

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.NotifyContractReminderSent(_contract);

            // Assert
            act.Should().Throw<InvalidOperationException>();
            VerifyAll();
        }

        #endregion


        #region Http Patch Tests

        [TestMethod]
        public void HttpPatchWithAADAuth_Does_NotThrowException()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());
            var updateRequest = new ContractUpdateRequest()
            {
                ContractNumber = "123",
                ContractVersion = 234
            };
            var updateRequestCollection = new List<ContractUpdateRequest>() { updateRequest };

            _mockHttp
                .Expect(TestBaseAddress + TestPatchEndpoint)
                .WithHeaders("Authorization", "Bearer " + TestFakeAccessToken)
                .Respond(HttpStatusCode.OK);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.PatchWithAADAuth(TestPatchEndpoint, updateRequestCollection);

            // Assert
            act.Should().NotThrow();
            VerifyAll();
        }

        [TestMethod]
        public void HttpPatchWithAADAuth_OnError_ThrowsException()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());
            var updateRequest = new ContractUpdateRequest()
            {
                ContractNumber = "123",
                ContractVersion = 234
            };
            var updateRequestCollection = new List<ContractUpdateRequest>() { updateRequest };

            _mockHttp
                .Expect(TestBaseAddress + TestPatchEndpoint)
                .Respond(HttpStatusCode.InternalServerError, new StringContent("Error"));

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()));

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.PatchWithAADAuth(TestPatchEndpoint, updateRequestCollection);

            // Assert
            act.Should().Throw<ApiGeneralException>();
            VerifyAll();
        }

        [TestMethod]
        public void HttpPatch_DoesNot_ThrowException()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());
            var updateRequest = new ContractUpdateRequest()
            {
                ContractNumber = "123",
                ContractVersion = 234
            };
            var updateRequestCollection = new List<ContractUpdateRequest>() { updateRequest };

            _mockHttp
                .Expect(TestBaseAddress + TestPatchEndpoint)
                .Respond(HttpStatusCode.OK);

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.Patch(TestPatchEndpoint, updateRequestCollection);

            // Assert
            act.Should().NotThrow();
            VerifyAll();
        }

        [TestMethod]
        public void HttpPatch_OnError_ThrowsException()
        {
            // Arrange
            var config = CreateContractsDataApiConfiguration(new Dictionary<string, string>());
            var updateRequest = new ContractUpdateRequest()
            {
                ContractNumber = "123",
                ContractVersion = 234
            };
            var updateRequestCollection = new List<ContractUpdateRequest>() { updateRequest };

            _mockHttp
                .Expect(TestBaseAddress + TestPatchEndpoint)
                .Respond(HttpStatusCode.InternalServerError, new StringContent("Error"));

            Mock.Get(_contractsLogger)
                .Setup(p => p.LogError(It.IsAny<ApiGeneralException>(), It.IsAny<string>(), It.IsAny<object[]>()));

            ContractNotificationService service = CreateContractNotificationService(config);

            // Act
            Func<Task> act = async () => await service.Patch(TestPatchEndpoint, updateRequestCollection);

            // Assert
            act.Should().Throw<ApiGeneralException>();
            VerifyAll();
        }

        #endregion


        #region Setup Helpers

        private ContractNotificationService CreateContractNotificationService(ContractsDataApiConfiguration contractsConfig)
        {
            var authService = CreateAuthenticationService();
            var httpClient = _mockHttp.ToHttpClient();

            var rtn = new ContractNotificationService(
                authService,
                httpClient,
                Options.Create(contractsConfig),
                _sbMessagingService,
                _contractsLogger,
                _auditService);

            return rtn;
        }

        private ContractNotificationService CreateMockContractNotifcationService(ContractsDataApiConfiguration contractsConfig)
        {
            var authService = CreateAuthenticationService();
            var httpClient = _mockHttp.ToHttpClient();

            var rtn = new Mock<ContractNotificationService>(
                MockBehavior.Strict,
                authService,
                httpClient,
                Options.Create(contractsConfig),
                _sbMessagingService,
                _contractsLogger,
                _auditService);

            return rtn.Object;
        }

        private ContractReminders CreateContractReminders()
        {
            ContractReminders rtn = new ContractReminders()
            {
                Contracts = new List<Contract>()
                {
                    new Contract()
                    {
                        ContractNumber = "One123",
                        ContractVersion = 234,
                        Id = 345,
                        Ukprn = 456
                    },
                    new Contract()
                    {
                        ContractNumber = "Two123",
                        ContractVersion = 234,
                        Id = 345,
                        Ukprn = 456
                    }
                }
            };

            return rtn;
        }

        private IDictionary<string, string> CreateQueryParameters(int numOfParameters)
        {
            IDictionary<string, string> rtn = new Dictionary<string, string>();

            for (int i = 0; i < numOfParameters; i++)
            {
                rtn.Add("keytest" + i, "valuetest" + i);
            }

            return rtn;
        }

        private string GetUpdateRequestContents()
        {
            var update = new ContractUpdateRequest()
            {
                Id = _contract.Id,
                ContractNumber = _contract.ContractNumber,
                ContractVersion = _contract.ContractVersion
            };

            return JsonConvert.SerializeObject(update);
        }

        private IAuthenticationService<ContractsDataApiConfiguration> CreateAuthenticationService()
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<ContractsDataApiConfiguration>>(MockBehavior.Strict);
            mockAuthenticationService
                .Setup(x => x.GetAccessTokenForAAD())
                .Returns(Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService.Object;
        }

        private ContractsDataApiConfiguration CreateContractsDataApiConfiguration(IDictionary<string, string> queryParameters)
            => new ContractsDataApiConfiguration()
            {
                ApiBaseAddress = TestBaseAddress,
                ContractReminderEndpoint = new QuerystringEndpointConfiguration()
                {
                    Endpoint = TestContractGetEndpoint,
                    QueryParameters = queryParameters
                },
                ContractReminderPatchEndpoint = new EndpointConfiguration()
                {
                    Endpoint = TestContractPatchEndpoint
                }
            };

        #endregion

        #region Verify Helpers

        private void VerifyAll()
        {
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_contractsLogger).VerifyAll();
            Mock.Get(_auditService).VerifyAll();
            Mock.Get(_sbMessagingService).VerifyAll();
        }

        #endregion
    }
}
