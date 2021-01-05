using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Pds.Contracts.Notifications.Services.Interfaces;
using System.Threading.Tasks;

namespace Pds.Contracts.Notifications.Func
{
    /// <summary>
    /// Example HTTP triggered Azure Function.
    /// </summary>
    public class ExampleHttpFunction
    {
        private readonly IExampleService _exampleService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ExampleHttpFunction"/> class.
        /// </summary>
        /// <param name="exampleService">The example service.</param>
        public ExampleHttpFunction(IExampleService exampleService)
        {
            _exampleService = exampleService;
        }

        /// <summary>
        /// Entry point to the Azure Function.
        /// </summary>
        /// <param name="req">The HTTP request.</param>
        /// <param name="log">The logger.</param>
        /// <returns>The result of the action.</returns>
        [FunctionName("ExampleHttpFunction")]
        public async Task<IActionResult> Example(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "example")] HttpRequest req,
            ILogger log)
        {
            log?.LogInformation("Example C# HTTP trigger function processed a request.");

            var result = await _exampleService.Hello();

            return new OkObjectResult(result);
        }
    }
}