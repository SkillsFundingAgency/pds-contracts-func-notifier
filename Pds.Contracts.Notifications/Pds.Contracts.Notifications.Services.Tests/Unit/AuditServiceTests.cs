using FluentAssertions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using RichardSzalay.MockHttp;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Tests.Unit
{
    [TestClass, TestCategory("Unit")]
    public class AuditServiceTests
    {
        private const string TestBaseAddress = "http://test-api-address";

        private const string TestApiEndpoint = "/test/api/endpoint";

        private const string TestFakeAccessToken = "AccessToken";

        private readonly MockHttpMessageHandler _mockHttp
            = new MockHttpMessageHandler();

        private readonly ILoggerAdapter<AuditService> _auditLogger
            = Mock.Of<ILoggerAdapter<AuditService>>(MockBehavior.Strict);

        [TestMethod]
        public void CreateAudit_Post_CallsExpectedEndpoint()
        {
            // Arrange
            Mock.Get(_auditLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            var config = GetAuditApiServicesConfiguration();
            _mockHttp
                .Expect(TestBaseAddress + config.CreateAuditEntryEndpoint.Endpoint)
                .Respond(HttpStatusCode.OK);

            AuditService auditService = CreateAuditService();

            var auditData = new Audit() { Action = 0, Message = "message", Severity = 0, Ukprn = null, User = "user" };

            // Act
            Func<Task> act = async () => await auditService.CreateAudit(auditData);

            // Assert
            act.Should().NotThrowAsync();
            VerifyAll();
        }

        [TestMethod]
        public void CreateAudit_Post_OnError_ExceptionIsRaised_And_ErrorIsLogged()
        {
            // Arrange
            Mock.Get(_auditLogger)
                .Setup(p => p.LogInformation(It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();
            Mock.Get(_auditLogger)
                .Setup(p => p.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object[]>()))
                .Verifiable();

            var config = GetAuditApiServicesConfiguration();
            _mockHttp
                .Expect(TestBaseAddress + config.CreateAuditEntryEndpoint.Endpoint)
                .Respond(HttpStatusCode.InternalServerError);

            AuditService auditService = CreateAuditService();

            var auditData = new Audit() { Action = 0, Message = "message", Severity = 0, Ukprn = null, User = "user" };

            // Act
            Func<Task> act = async () => await auditService.CreateAudit(auditData);

            // Assert
            act.Should().Throw<ApiGeneralException>();
            VerifyAll();
        }

        #region Setup Helpers

        private AuditApiConfiguration GetAuditApiServicesConfiguration()
            => new AuditApiConfiguration()
            {
                ApiBaseAddress = TestBaseAddress,
                CreateAuditEntryEndpoint = new EndpointConfiguration()
                {
                    Endpoint = TestApiEndpoint
                }
            };

        private Mock<IAuthenticationService<T>> GetMockAuthenticationService<T>()
            where T : BaseApiClientConfiguration
        {
            var mockAuthenticationService = new Mock<IAuthenticationService<T>>(MockBehavior.Strict);
            mockAuthenticationService.Setup(x => x.GetAccessTokenForAAD()).Returns(
                Task.FromResult(TestFakeAccessToken));
            return mockAuthenticationService;
        }

        private AuditService CreateAuditService()
        {
            var auditConfiguration = Options.Create(GetAuditApiServicesConfiguration());
            var mockAuthentication = GetMockAuthenticationService<AuditApiConfiguration>();
            var httpClient = _mockHttp.ToHttpClient();
            var auditService = new AuditService(mockAuthentication.Object, httpClient, auditConfiguration, _auditLogger);
            return auditService;
        }

        #endregion


        #region Verify Helpers

        private void VerifyAll()
        {
            _mockHttp.VerifyNoOutstandingExpectation();
            Mock.Get(_auditLogger).Verify();
        }

        #endregion
    }
}
