using Microsoft.Extensions.Logging;
using Pds.Audit.Api.Client.Interfaces;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Contracts.Notifications.Services.Utilities;
using Sfa.Sfs.Contracts.Messaging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Contract status change publisher.
    /// </summary>
    public class ContractStatusChangePublisher : IContractStatusChangePublisher
    {
        private readonly IServiceBusMessagingService _serviceBusMessagingService;
        private readonly IAuditService _auditService;
        private readonly ILogger<IContractStatusChangePublisher> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractStatusChangePublisher"/> class.
        /// </summary>
        /// <param name="serviceBusMessagingService">The messaging service to use for queuing status change messages.</param>
        /// <param name="auditService">The shared audit service.</param>
        /// <param name="logger">The logger.</param>
        public ContractStatusChangePublisher(IServiceBusMessagingService serviceBusMessagingService, IAuditService auditService, ILogger<IContractStatusChangePublisher> logger)
        {
            _serviceBusMessagingService = serviceBusMessagingService;
            _auditService = auditService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task NotifyContractApprovedAsync(Contract contract)
        {
            _logger.LogInformation($"Queuing contract approval email for contract [{contract.ContractNumber}].");
            var message = new ContractApprovedMessage { ContractId = contract.Id };
            await NotifyAsync(message, contract, nameof(NotifyContractApprovedAsync));
        }

        /// <inheritdoc/>
        public async Task NotifyContractChangesAreReadyForReviewAsync(Contract contract)
        {
            _logger.LogInformation($"Queuing contract ready for review email for contract [{contract.ContractNumber}].");
            var message = new ContractReadyToReviewEmailMessage
            {
                ContractNumber = contract.ContractNumber,
                Ukprn = contract.Ukprn,
                VersionNumber = contract.ContractVersion
            };
            await NotifyAsync(message, contract, nameof(NotifyContractChangesAreReadyForReviewAsync));
        }

        /// <inheritdoc/>
        public async Task NotifyContractIsReadyToSignAsync(Contract contract)
        {
            _logger.LogInformation($"Queuing contract ready to sign email for contract [{contract.ContractNumber}].");
            var message = new ContractReadyToSignEmailMessage
            {
                ContractNumber = contract.ContractNumber,
                Ukprn = contract.Ukprn,
                VersionNumber = contract.ContractVersion
            };
            await NotifyAsync(message, contract, nameof(NotifyContractIsReadyToSignAsync));
        }

        /// <inheritdoc/>
        public async Task NotifyContractWithdrawnAsync(Contract contract)
        {
            _logger.LogInformation($"Queuing contract withdrawn email for contract [{contract.ContractNumber}].");
            var message = new ContractWithdrawnEmailMessage
            {
                ContractNumber = contract.ContractNumber,
                Ukprn = contract.Ukprn,
                VersionNumber = contract.ContractVersion
            };
            await NotifyAsync(message, contract, nameof(NotifyContractWithdrawnAsync));
        }

        private async Task NotifyAsync<T>(T message, Contract contract, string component)
        {
            IDictionary<string, string> properties = new Dictionary<string, string>()
            {
                { "messageType", typeof(T).FullName }
            };

            await _serviceBusMessagingService.SendAsBinaryXmlMessageAsync(message, properties);
            await _auditService.AuditAsync(
                new Pds.Audit.Api.Client.Models.Audit
                {
                    Severity = 0,
                    Action = Audit.Api.Client.Enumerations.ActionType.ContractNotificationForwarded,
                    Ukprn = contract.Ukprn,
                    User = Constants.Audit_User_System,
                    Message = Constants.ContractNotificationForwardedMessage
                        .ReplaceTokens(contract)
                        .ReplaceTokens(new Dictionary<string, string> { { Constants.Component, component } })
                });
        }
    }
}