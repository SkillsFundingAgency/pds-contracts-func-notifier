﻿using Microsoft.Extensions.Options;
using Pds.Audit.Api.Client.Enumerations;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Extensions;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Contracts.Notifications.Services.Utilities;
using Pds.Contracts.Notifications.Services.Utilities.HttpClientDataApiProvider;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.DfESignIn.Interfaces;
using Pds.Core.Identity.Claims.Enums;
using Pds.Core.Logging;
using Pds.Core.Notification.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations.Contracts
{
    /// <summary>
    /// Contract Ready To Review Email Service.
    /// </summary>
    public class ContractReadyToReviewEmailService : ContractsDataApiProvider, IContractReadyToReviewEmailService
    {
        private readonly string[] _requiredRoles = new[]
        {
            UserRole.ViewContractsAndAgreements.ToString(),
            UserRole.SignContractsAndAgreements.ToString()
        };

        private readonly INotificationEmailQueueService _notificationEmailQueueService;
        private readonly ILoggerAdapter<ContractReadyToReviewEmailService> _logger;
        private readonly IDfESignInPublicApi _dfESignInPublicApi;
        private readonly IAuditService _auditService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReadyToReviewEmailService"/> class.
        /// </summary>
        /// <param name="authenticationService">The authentication service.</param>
        /// <param name="configurationOptions">The configuration options.</param>
        /// <param name="notificationEmailQueueService">Notification email queue service.</param>
        /// <param name="logger">Logger.</param>
        /// <param name="dfESignInPublicApi">dfESignInPublicApi.</param>
        /// <param name="auditService">Audit Service.</param>
        /// <param name="httpClient">The HTTP client.</param>
        public ContractReadyToReviewEmailService(
            IAuthenticationService<ContractsDataApiConfiguration> authenticationService,
            IOptions<ContractsDataApiConfiguration> configurationOptions,
            INotificationEmailQueueService notificationEmailQueueService,
            ILoggerAdapter<ContractReadyToReviewEmailService> logger,
            IDfESignInPublicApi dfESignInPublicApi,
            IAuditService auditService,
            HttpClient httpClient)
            : base(authenticationService, httpClient, Options.Create(configurationOptions.Value))
        {
            _notificationEmailQueueService = notificationEmailQueueService;
            _dfESignInPublicApi = dfESignInPublicApi;
            _auditService = auditService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task Process(ContractReadyToReviewEmailMessage message)
        {
            _logger.LogInformation($"Processing {nameof(ContractReadyToReviewEmailMessage)} with Ukprn {message.Ukprn}, contract number {message.ContractNumber} and version number {message.VersionNumber}.");

            var contract = await Get<Contract>(ContractApiHelper.CreateContractQueryString(message.ContractNumber, message.VersionNumber, message.Ukprn));

            var audit = AuditHelper.ConstructAuditObject(
                    string.Format(Constants.AuditMessage, nameof(ContractReadyToReviewEmailMessage)),
                    contract.Ukprn,
                    ActionType.EmailMessagePushed,
                    Constants.ComponentName,
                    SeverityLevel.Information);

            if (contract.Status == ContractStatus.Approved && contract.AmendmentType == ContractAmendmentType.Notfication)
            {
                var userContactsResponse = await _dfESignInPublicApi.GetUserContactsForOrganisation(message.Ukprn, _requiredRoles);

                if (userContactsResponse.Users.IsNotNullOrEmpty())
                {
                    var toContractRecipients = userContactsResponse.Users.Select(user => user.Email);

                    var personalisation = new Dictionary<string, object>()
                    {
                         { "DocumentTitle",  contract.Title },
                         { "contract or agreement",  contract.DocumentType }
                    };

                    var notificationMessage = EmailNotificationHelper.ConstructNotification(toContractRecipients, Constants.RequestingService_Contracts, Constants.MessageType_ContractReadyToReviewEmail, personalisation);

                    await _notificationEmailQueueService.SendAsync(notificationMessage);

                    _logger.LogInformation(string.Format(Constants.LogMessage, contract.ContractDisplayText, audit.Message));
                }
                else
                {
                    var errorMessage = $"{nameof(ContractReadyToReviewEmailMessage)} processed and no users found with roles [{string.Join(", ", _requiredRoles)}] for organisation [{contract.Ukprn}]";
                    audit.UpdateErrorMessage(errorMessage);
                    _logger.LogError(string.Format(Constants.LogMessage, contract.ContractDisplayText, errorMessage));
                }

                await _auditService.AuditAsync(audit);
            }
            else
            {
                _logger.LogError($"Contract id {contract.ContractDisplayText} doesn't meet the the criteria - Funding type: [{contract.FundingType}] Status: [{contract.Status}] Amendmant type [{contract.AmendmentType}]. {nameof(ContractReadyToReviewEmailMessage)} was not processed.");
            }
        }
    }
}
