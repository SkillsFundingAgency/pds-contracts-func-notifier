namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Defines a message for a reminder contract.
    /// </summary>
    public struct ContractReminderMessage
    {
        /// <summary>
        /// Gets or sets the id of the contract that still needs to be signed (email reminder).
        /// </summary>
        public int ContractId { get; set; }
    }
}
