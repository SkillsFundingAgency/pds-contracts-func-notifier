using System;
using System.Collections.Generic;
using System.Text;

namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// A class to represent an audit entry for the Audit API.
    /// </summary>
    public struct Audit
    {
        /// <summary>
        /// Gets or sets the severity.
        /// </summary>
        public int Severity { get; set; }

        /// <summary>
        /// Gets or sets the user.
        /// </summary>
        public string User { get; set; }

        /// <summary>
        /// Gets or sets the ukprn.
        /// </summary>
        public int? Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets the action.
        /// </summary>
        public int Action { get; set; }
    }
}
