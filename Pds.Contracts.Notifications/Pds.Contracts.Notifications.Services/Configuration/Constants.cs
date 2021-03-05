using Pds.Contracts.Notifications.Services.Models;

namespace Pds.Contracts.Notifications.Services.Configuration
{
    /// <summary>
    /// All constant literals are kept in this class.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Component that is sending the notification, equivalent to message handler type.
        /// </summary>
        public const string Component = "Component";

        /// <summary>
        /// Audit user.
        /// </summary>
        public const string Audit_User_System = "System_NotificationFunction";

        /// <summary>
        /// Message that should be used for auditing contract status change notification when forwarded to legacy service bus queue.
        /// </summary>
        public static readonly string ContractNotificationForwardedMessage = $"A contract notification has been forwarded to the [MessageProcessor] by [{{{Component}}}] for contract [{{{nameof(Contract.ContractNumber)}}}] Version [{{{nameof(Contract.ContractVersion)}}}] with the status of [{{{nameof(Contract.Status)}}}]";
    }
}