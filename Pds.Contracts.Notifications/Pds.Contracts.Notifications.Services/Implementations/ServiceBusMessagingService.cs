using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Pds.Contracts.Notifications.Services.Interfaces;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using System.Xml;

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
        public async Task SendAsBinaryXmlMessageAsync<TObject>(TObject message, IDictionary<string, string> properties)
        {
            var data = Serialise(message);
            var msg = new Message(data)
            {
                ContentType = "application/xml"
            };

            if (properties?.Count > 0)
            {
                foreach (var item in properties.Keys)
                {
                    msg.UserProperties.Add(item, properties[item]);
                }
            }

            await _messageSender.SendAsync(msg);
        }

        private byte[] Serialise<TObject>(TObject obj)
        {
            var serializer = new DataContractSerializer(typeof(TObject));
            var stream = new MemoryStream();
            using (var writer = XmlDictionaryWriter.CreateBinaryWriter(stream))
            {
                serializer.WriteObject(writer, obj);
            }

            return stream.ToArray();
        }
    }
}
