using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func
{
    /// <summary>
    /// Example ServiceBus queue triggered Azure Function.
    /// </summary>
    public class ExampleServiceBusFunction
    {
        private readonly IExampleService _exampleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleServiceBusFunction"/> class.
        /// </summary>
        /// <param name="exampleService">The example service.</param>
        public ExampleServiceBusFunction(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="myQueueItem">The queue item that triggered this function to run.</param>
        /// <param name="log">The logger.</param>
        /// <returns>Async Task.</returns>
        [FunctionName("ExampleServiceBusFunction")]
        public async Task Run(
            [ServiceBusTrigger("examplequeue", Connection = "ServiceBusConnection")] string myQueueItem,
            ILogger log)
        {
            var result = await _exampleService.Hello();

            log?.LogInformation($"Example C# ServiceBus queue trigger function processed message: {myQueueItem} with result {result}");
        }
    }
}