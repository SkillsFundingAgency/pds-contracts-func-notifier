using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.ApiClient;
using Pds.Core.ApiClient.Exceptions;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Contract reminder service.
    /// </summary>
    public class ContractNotificationService : BaseApiClient<ContractsDataApiConfiguration>, IContractNotificationService, IHttpApiClientPatch
    {
        /// <summary>
        /// The audit user.
        /// </summary>
        public const string Audit_User_System = "System";

        private readonly ILoggerAdapter<ContractNotificationService> _logger;

        private readonly IAuditService _auditService;

        private readonly ContractsDataApiConfiguration _dataApiConfiguration;

        private readonly IServiceBusMessagingService _sbMessagingService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractNotificationService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="serviceBusMessagingService">The messaging service to use for queuing reminders.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        /// <param name="auditService">The audit service.</param>
        public ContractNotificationService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            HttpClient httpClient,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            IServiceBusMessagingService serviceBusMessagingService,
            ILoggerAdapter<ContractNotificationService> logger,
            IAuditService auditService)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _logger = logger;
            _sbMessagingService = serviceBusMessagingService;
            _auditService = auditService;
            _dataApiConfiguration = configurationOptions.Value;
        }

        /// <inheritdoc/>
        public async Task<ContractReminders> GetOverdueContracts()
        {
            string querystring = CreateQueryString(_dataApiConfiguration.ContractReminderEndpoint);
            _logger.LogInformation($"Requesting a list of contracts with overdue reminders with querystring {querystring}.");
            return await GetWithAADAuth<ContractReminders>(querystring);
        }

        /// <inheritdoc/>
        public async Task QueueContractEmailReminderMessage(Contract contract)
        {
            _logger.LogInformation($"Queuing email reminder for contract [{contract.ContractNumber}].");

            var message = new ContractReminderMessage() { ContractId = contract.Id };
            await _sbMessagingService.SendMessageAsync(message);

            await _auditService.AuditAsync(
                new Pds.Audit.Api.Client.Models.Audit
                {
                    Severity = 0,
                    Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                    Ukprn = contract.Ukprn,
                    Message = $"Email reminder has been queued for contract with Id [{contract.Id}].",
                    User = Audit_User_System
                });
        }

        /// <inheritdoc/>
        public async Task NotifyContractReminderSent(Contract contract)
        {
            var updateRequest = new ContractUpdateRequest
            {
                Id = contract.Id,
                ContractNumber = contract.ContractNumber,
                ContractVersion = contract.ContractVersion
            };

            _logger.LogInformation($"Updating the last reminder date on contract with id [{contract.ContractNumber}].");
            await PatchWithAADAuth(_dataApiConfiguration.ContractReminderPatchEndpoint.Endpoint, updateRequest);

            await _auditService.AuditAsync(
                new Pds.Audit.Api.Client.Models.Audit
                {
                    Severity = 0,
                    Action = Audit.Api.Client.Enumerations.ActionType.ContractEmailReminderQueued,
                    Ukprn = contract.Ukprn,
                    Message = $"Updated 'Last email reminder sent date' for contract with Id {contract.Id}",
                    User = Audit_User_System
                });
        }

        /// <inheritdoc/>
        protected override Action<ApiGeneralException> FailureAction
            => exception =>
            {
                _logger.LogError(exception, exception.Message);
                throw exception;
            };

        #region HTTP PATCH

        /// <summary>
        /// Performs a HTTP PATCH request to given URI, with the payload of type <typeparamref name="TRequest"/>.
        /// </summary>
        /// <typeparam name="TRequest">The payload type of this request.</typeparam>
        /// <param name="requestUri">The request URI to call.</param>
        /// <param name="requestBody">The payload of this request.</param>
        /// <param name="setAccessTokenAction">The delegate to set the access token.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task Patch<TRequest>(string requestUri, TRequest requestBody, Func<Task> setAccessTokenAction = null)
        {
            ApiGeneralException apiException;

            try
            {
                var json = JsonConvert.SerializeObject(requestBody);

                using (var stringContent = new StringContent(json, Encoding.UTF8, "application/json"))
                {
                    if (setAccessTokenAction != null)
                    {
                        await setAccessTokenAction();
                    }

                    var response = await HttpClient.PatchAsync(requestUri, stringContent);

                    if (response.IsSuccessStatusCode)
                    {
                        return;
                    }

                    var responseContent = await response?.Content?.ReadAsStringAsync();
                    apiException = new ApiGeneralException(response.StatusCode, responseContent);
                }
            }
            catch (Exception caughtException)
            {
                apiException = new ApiGeneralException(caughtException);
            }

            FailureAction(apiException);
        }

        /// <summary>
        /// Performs an authenticated HTTP PATCH request to the given URI, with a payload of type <typeparamref name="TRequest"></typeparamref>.
        /// </summary>
        /// <typeparam name="TRequest">The payload type of this request.</typeparam>
        /// <param name="requestUri">The request URI to call.</param>
        /// <param name="requestBody">The payload of this request.</param>
        /// <returns>A <see cref="Task"/> representing the result of the asynchronous operation.</returns>
        public async Task PatchWithAADAuth<TRequest>(string requestUri, TRequest requestBody)
        {
            await Patch<TRequest>(requestUri, requestBody, async () => await SetAADAccessTokenHeader());
        }

        #endregion


        #region Helper Functions

        private static string CreateQueryString(QuerystringEndpointConfiguration endpointConfiguration)
        {
            string querystring = $"{endpointConfiguration.Endpoint}?";
            IList<string> query = new List<string>();

            foreach (var item in endpointConfiguration.QueryParameters.Keys)
            {
                query.Add($"{item}={endpointConfiguration.QueryParameters[item]}");
            }

            querystring += string.Join('&', query);
            return querystring;
        }

        #endregion
    }
}