using System;
using System.Collections.Generic;
using System.Text;
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
        /// </summary>
        /// <typeparam name="TObject">The object type to send.</typeparam>
        /// <param name="message">The message contents.</param>
        /// <returns>Async Task.</returns>
        Task SendMessageAsync<TObject>(TObject message);
    }
}
