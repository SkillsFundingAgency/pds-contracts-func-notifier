using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func
{
    /// <summary>
    /// Example ServiceBus queue triggered Azure Function.
    /// </summary>
    public class ContractChangeNotificationsServiceBusFunction
    {
        private readonly IContractNotificationService _notificationService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractChangeNotificationsServiceBusFunction"/> class.
        /// </summary>
        /// <param name="notificationService">The example service.</param>
        public ContractChangeNotificationsServiceBusFunction(IContractNotificationService notificationService)
        {
            _notificationService = notificationService;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="contract">The queue item that triggered this function to run.</param>
        /// <param name="log">The logger.</param>
        /// <returns>Async Task.</returns>
        [FunctionName("ContractChangeNotificationsServiceBusFunction")]
        public async Task Run(
            [ServiceBusTrigger("%Pds.Contracts.Notifications.Topic%", "%Pds.Contracts.Notifier.Subscription%", Connection = "%sb-connection-string%")] Contract contract,
            ILogger log)
        {
            log?.LogInformation($"[Start] Contract change notification function processing message with contract number: {contract.ContractNumber}, version: {contract.ContractVersion}.");

            await _notificationService.NotifyContractChanges(contract);

            log?.LogInformation($"[End] Contract change notification function processed message with contract number: {contract.ContractNumber}, version: {contract.ContractVersion}.");
        }
    }
}