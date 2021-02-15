namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Service bus configuration properties.
    /// </summary>
    public class ServiceBusConfiguration
    {
        /// <summary>
        /// Gets or sets the connection string.
        /// </summary>
        public string ConnectionString { get; set; }

        /// <summary>
        /// Gets or sets the queue name.
        /// </summary>
        public string QueueName { get; set; }
    }
}
