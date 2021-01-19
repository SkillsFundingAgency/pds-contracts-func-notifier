using Pds.Contracts.Notifications.Services.Models;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Contract reminder service.
    /// </summary>
    public interface IContractNotificationService
    {
        /// <summary>
        /// Reminds contracts that are ready for signing.
        /// </summary>
        /// <returns>Async task.</returns>
        Task RemindContractsReadyForSigning();

        /// <summary>
        /// Notifies about changes to the contract.
        /// </summary>
        /// <param name="contractChange">Changed contract.</param>
        /// <returns>Async task.</returns>
        Task NotifyContractChanges(Contract contractChange);
    }
}