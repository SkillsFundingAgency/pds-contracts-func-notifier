﻿using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Interfaces.FundingClaims
{
    /// <summary>
    /// Funding Claim Withdrawn Email Service.
    /// </summary>
    public interface IFundingClaimWithdrawnEmailService
    {
        /// <summary>
        /// Process and consume the funding claim withdrawn email message.
        /// </summary>
        /// <param name="message">Funding claim withdrawn email message.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        public Task Process(FundingClaimWithdrawnEmailMessage message);
    }
}
