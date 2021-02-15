using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces;
using System.Text;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <inheritdoc/>
    public class ServiceBusMessagingService : IServiceBusMessagingService
    {
        private readonly IMessageSender _messageSender;

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBusMessagingService"/> class.
        /// </summary>
        /// <param name="messageSender">Message sender to use to send notifications.</param>
        public ServiceBusMessagingService(
            IMessageSender messageSender)
        {
            _messageSender = messageSender;
        }

        /// <inheritdoc/>
        public async Task SendMessageAsync<TObject>(TObject message)
        {
            string strMessage = JsonConvert.SerializeObject(message);
            byte[] byteMessage = Encoding.UTF8.GetBytes(strMessage);

            Message msg = new Message(byteMessage)
            {
                ContentType = "application/json"
            };

            await _messageSender.SendAsync(msg);
        }
    }
}
