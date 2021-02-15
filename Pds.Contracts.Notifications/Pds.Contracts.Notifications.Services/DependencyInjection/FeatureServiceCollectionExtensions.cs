using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Contracts.Notifications.Services.Configuration;
using Pds.Contracts.Notifications.Services.HttpPolicyConfiguration;
using Pds.Contracts.Notifications.Services.Implementations;
using Pds.Contracts.Notifications.Services.Interfaces;
using Pds.Core.ApiClient.Interfaces;
using Pds.Core.ApiClient.Services;
using Pds.Core.Utils.Implementations;
using Pds.Core.Utils.Interfaces;

namespace Pds.Contracts.Notifications.Services.DependencyInjection
{
    /// <summary>
    /// Extensions class for <see cref="IServiceCollection"/> for registering the feature's services.
    /// </summary>
    public static class FeatureServiceCollectionExtensions
    {
        /// <summary>
        /// Adds services for the current feature to the specified <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/> to add the feature's services to.</param>
        /// <param name="config">The <see cref="IConfiguration"/> elements for the current service.</param>
        /// <returns>A reference to this instance after the operation has completed.</returns>
        public static IServiceCollection AddFeatureServices(this IServiceCollection services, IConfiguration config)
        {
            var policyRegistry = services.AddPolicyRegistry();
            var policies = new PolicyType[] { PolicyType.Retry, PolicyType.CircuitBreaker };

            // Configure Polly Policies for IAuditService HttpClient
            services
                .AddPolicies<IAuditService>(config, policyRegistry)
                .AddHttpClient<IAuditService, AuditService, AuditApiConfiguration>(config, policies);

            // Configure Polly Policies for IContractsApproverService HttpClient
            services
                .AddPolicies<IContractNotificationService>(config, policyRegistry)
                .AddHttpClient<IContractNotificationService, ContractNotificationService, ContractsDataApiConfiguration>(config, policies);

            services.AddAzureServiceBusSender(config);

            // Need to allow resue of Azure AAD auth tokens
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

            services.AddScoped(typeof(IContractReminderProcessingService), typeof(ContractReminderProcessingService));

            services.AddScoped(typeof(IServiceBusMessagingService), typeof(ServiceBusMessagingService));

            services.AddTransient(typeof(IAuthenticationService<>), typeof(AuthenticationService<>));

            return services;
        }

        private static IServiceCollection AddAzureServiceBusSender(this IServiceCollection services, IConfiguration config)
        {
            var sbSettings = new ServiceBusConfiguration();
            config.GetSection(nameof(ServiceBusConfiguration)).Bind(sbSettings);

            services.AddSingleton<IMessageSender, MessageSender>(
                provider => ActivatorUtilities.CreateInstance<MessageSender>(
                provider,
                sbSettings.ConnectionString,
                sbSettings.QueueName));

            return services;
        }
    }
}