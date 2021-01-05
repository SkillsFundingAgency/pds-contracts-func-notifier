using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using System;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func
{
    /// <summary>
    /// Example timer triggered Azure Function.
    /// </summary>
    public class ExampleTimerFunction
    {
        private readonly IExampleService _exampleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleTimerFunction"/> class.
        /// </summary>
        /// <param name="exampleService">The example service.</param>
        public ExampleTimerFunction(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="myTimer">The timer info.</param>
        /// <param name="log">The logger.</param>
        /// <returns>Async task.</returns>
        [FunctionName("ExampleTimerFunction")]
        public async Task Run(
            [TimerTrigger("*/10 * * * * *")] TimerInfo myTimer,
            ILogger log)
        {
            var result = await _exampleService.Hello();

            log?.LogInformation($"Example C# Timer trigger function executed at: {DateTime.Now} and the result was {result}");
        }
    }
}