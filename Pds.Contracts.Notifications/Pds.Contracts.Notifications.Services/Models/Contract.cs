namespace Pds.Contracts.Notifications.Services.Models
{
    /// <summary>
    /// Contract object.
    /// </summary>
    public class Contract
    {
        /// <summary>
        /// Gets or sets contract Id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets contract Ukprn.
        /// </summary>
        public int Ukprn { get; set; }

        /// <summary>
        /// Gets or sets the contract number.
        /// </summary>
        public string ContractNumber { get; set; }

        /// <summary>
        /// Gets or sets the contract version.
        /// </summary>
        public int ContractVersion { get; set; }

        /// <summary>
        /// Gets or sets status.
        /// </summary>
        public ContractStatus Status { get; set; }
    }
}