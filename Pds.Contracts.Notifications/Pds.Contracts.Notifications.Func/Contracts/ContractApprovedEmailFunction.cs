using Microsoft.Azure.WebJobs;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.Interfaces.Contracts;
using Pds.Contracts.Notifications.Services.Models.ServiceBusMessages;
using Pds.Core.Utils.Helpers;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func.Contracts
{
    /// <summary>
    /// Contract Approved Email Function which listens and process the contractapprovedemail queue messages.
    /// </summary>
    public class ContractApprovedEmailFunction
    {
        private readonly IContractApprovedEmailService _contractApprovedEmailService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractApprovedEmailFunction"/> class.
        /// </summary>
        /// <param name="contractApprovedEmailService">Contract approved email Service.</param>
        public ContractApprovedEmailFunction(IContractApprovedEmailService contractApprovedEmailService)
        {
            _contractApprovedEmailService = contractApprovedEmailService;
        }

        /// <summary>
        /// Listens to contractapprovedemail queue and send the email.
        /// </summary>
        /// <param name="contractApprovedEmailMessage">Contract approved email message from queue.</param>
        /// <returns><see cref="Task"/> representing the asynchronous operation.</returns>
        [FunctionName(nameof(ContractApprovedEmailFunction))]
        public async Task Run([ServiceBusTrigger(Constants.ContractApprovedEmailQueue, Connection = "NotifierServiceBusConnectionString")]ContractApprovedEmailMessage contractApprovedEmailMessage)
        {
            try
            {
                It.IsNullOrDefault(contractApprovedEmailMessage.ContractId)
                    .AsGuard<ArgumentException>();

                await _contractApprovedEmailService.Process(contractApprovedEmailMessage);
            }
            catch
            {
                throw;
            }
        }
    }
}
