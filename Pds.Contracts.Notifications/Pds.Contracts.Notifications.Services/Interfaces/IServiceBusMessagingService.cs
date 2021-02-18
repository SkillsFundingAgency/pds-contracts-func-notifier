using System.Collections.Generic;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces
{
    /// <summary>
    /// Service to manage connectivity to Azure Service Bus.
    /// </summary>
    public interface IServiceBusMessagingService
    {
        /// <summary>
        /// Sends a message with the contents of <see cref="TObject"/> to the Azure Service Bus.
        /// The message is serialised using DataContracts.
        /// </summary>
        /// <typeparam name="TObject">The object type to send.</typeparam>
        /// <param name="message">The message contents.</param>
        /// <param name="properties">Any custom properties to add to the message.</param>
        /// <returns>Async Task.</returns>
        Task SendAsBinaryXmlMessageAsync<TObject>(TObject message, IDictionary<string, string> properties);
    }
}
