using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <summary>
    /// Contract reminder service.
    /// </summary>
    public class ContractNotificationService : IContractNotificationService
    {
        /// <inheritdoc/>
        public async Task NotifyContractChanges(Contract contractChange)
        {
            await Task.CompletedTask;
        }

        /// <inheritdoc/>
        public async Task RemindContractsReadyForSigning()
        {
            await Task.CompletedTask;
        }
    }
}