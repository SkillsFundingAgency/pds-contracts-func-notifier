using System.Collections.Generic;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// Endpoint configuration with querystring data.
    /// </summary>
    public class QuerystringEndpointConfiguration : EndpointConfiguration
    {
        /// <summary>
        /// Gets or sets a collection of query parameters.
        /// </summary>
        public IDictionary<string, string> QueryParameters { get; set; }
    }
}
