using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Contracts.Notifications.Services.Models;
using Pds.Core.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Services.Implementations
{
    /// <inheritdoc/>
    public class ContractReminderProcessingService : IContractReminderProcessingService
    {
        private readonly IContractNotificationService _contractNotificationService;
        private readonly ILoggerAdapter<ContractReminderProcessingService> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContractReminderProcessingService"/> class.
        /// </summary>
        /// <param name="contractNotificationService">Contract notification service to use to access contracts data.</param>
        /// <param name="logger">ILogger reference to log output.</param>
        public ContractReminderProcessingService(
            IContractNotificationService contractNotificationService,
            ILoggerAdapter<ContractReminderProcessingService> logger)
        {
            _contractNotificationService = contractNotificationService;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task IssueContractReminders()
        {
            ContractReminders reminders = null;
            int remindersSent = 0;
            do
            {
                reminders = await _contractNotificationService.GetOverdueContracts();
                if (reminders?.Contracts?.Count > 0)
                {
                    _logger.LogInformation($"Processing a list of contracts with {reminders.Contracts.Count} records.");
                    IList<Task> tasks = new List<Task>();
                    foreach (var contract in reminders.Contracts)
                    {
                        var reminderProcess = Task.Run(async () =>
                        {
                            try
                            {
                                _logger.LogInformation($"Starting processing contract with id {contract.Id}.");

                                await _contractNotificationService.QueueContractEmailReminderMessage(contract);

                                await _contractNotificationService.NotifyContractReminderSent(contract);

                                _logger.LogInformation($"Completed processing for contract with id {contract.Id}.");
                            }
                            catch (Exception e)
                            {
                                _logger.LogError(e, $"Contract with id [{contract.Id}] failed to process successfully.");
                                throw;
                            }
                        });

                        tasks.Add(reminderProcess);
                    }

                    try
                    {
                        await Task.WhenAll(tasks);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Error processign one or more records.");
                    }

                    if (tasks.All(p => !p.IsCompletedSuccessfully))
                    {
                        _logger.LogCritical("All processes failed to complete successfully.  Aborting...");
                        _logger.LogInformation($"Contract reminders processes aborted.  Sent {remindersSent} reminders prior to abort.");
                        throw new ContractProcessingException("All processes failed to complete successfully.", tasks.First(p => !p.IsCompletedSuccessfully).Exception);
                    }
                    else
                    {
                        remindersSent += tasks.Count(p => p.IsCompletedSuccessfully);
                    }
                }
            }
            while (reminders?.Contracts?.Count > 0);

            _logger.LogInformation($"Contract reminders process completed.  Sent {remindersSent} reminders.");
        }
    }
}