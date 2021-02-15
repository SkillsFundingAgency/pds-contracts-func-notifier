using Pds.Core.ApiClient;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Configuration setting for the Contracts Data API.
    /// </summary>
    public class ContractsDataApiConfiguration : BaseApiClientConfiguration
    {
        /// <summary>
        /// Gets or sets the contact reminder endpoint configuration settings.
        /// </summary>
        public QuerystringEndpointConfiguration ContractReminderEndpoint { get; set; }

        /// <summary>
        /// Gets or sets the contract reminder patch endpoint settings.
        /// </summary>
        public EndpointConfiguration ContractReminderPatchEndpoint { get; set; }
    }
}
