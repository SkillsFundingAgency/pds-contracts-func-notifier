using Microsoft.Extensions.Options;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Service wrapper to allow calls to be made to the Audit API.
    /// </summary>
    public class AuditService : BaseApiClient<AuditApiConfiguration>, IAuditService
    {
        private readonly ILoggerAdapter<AuditService> _logger;

        private readonly AuditApiConfiguration _auditApiConfiguration;

        /// <summary>
        /// Initializes a new instance of the <see cref="AuditService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        public AuditService(
            IAuthenticationService<AuditApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<AuditApiConfiguration> configurationOptions,
            ILoggerAdapter<AuditService> logger)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _logger = logger;
            _auditApiConfiguration = configurationOptions.Value;
        }

        /// <inheritdoc/>
        public async Task CreateAudit(Audit audit)
        {
            _logger.LogInformation($"Creating an audit entry for action [{audit.Action}] : {audit.Message}");

            await PostWithAADAuth(_auditApiConfiguration.CreateAuditEntryEndpoint.Endpoint, audit);
        }

        /// <inheritdoc/>
        protected override Action<ApiGeneralException> FailureAction
            => exception =>
            {
                _logger.LogError(exception, exception.Message);
                throw exception;
            };
    }
}
