using Pds.Core.ApiClient;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Configuration setting for the audit API.
    /// </summary>
    public class AuditApiConfiguration : BaseApiClientConfiguration
    {
        /// <summary>
        /// Gets or sets the endpoint configuration for creating a new audit entry.
        /// </summary>
        public EndpointConfiguration CreateAuditEntryEndpoint { get; set; }
    }
}
