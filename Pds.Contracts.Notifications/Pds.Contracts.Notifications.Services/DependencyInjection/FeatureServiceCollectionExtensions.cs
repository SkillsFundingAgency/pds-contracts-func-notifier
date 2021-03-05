using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pds.Audit.Api.Client.Registrations;
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

            // Configure Polly Policies for IContractsApproverService HttpClient
            services
                .AddPolicies<IContractNotificationService>(config, policyRegistry)
                .AddHttpClient<IContractNotificationService, ContractNotificationService, ContractsDataApiConfiguration>(config, policies);

            services.AddAzureServiceBusSender(config);

            // Configure service for audit
            services.AddAuditApiClient(config, policyRegistry);

            // Need to allow resue of Azure AAD auth tokens
            services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
            services.AddTransient(typeof(IAuthenticationService<>), typeof(AuthenticationService<>));

            services.AddScoped(typeof(IContractReminderProcessingService), typeof(ContractReminderProcessingService));
            services.AddScoped(typeof(IServiceBusMessagingService), typeof(ServiceBusMessagingService));

            services.AddScoped<IContractStatusChangePublisher, ContractStatusChangePublisher>();

            services.AddAutoMapper(typeof(FeatureServiceCollectionExtensions).Assembly);

            return services;
        }

        private static IServiceCollection AddAzureServiceBusSender(this IServiceCollection services, IConfiguration config)
        {
            var sbSettings = new MonolithServiceBusConfiguration();
            config.GetSection(nameof(MonolithServiceBusConfiguration)).Bind(sbSettings);

            services.AddSingleton<IMessageSender, MessageSender>(
                provider => ActivatorUtilities.CreateInstance<MessageSender>(
                provider,
                sbSettings.ConnectionString,
                sbSettings.QueueName));

            return services;
        }
    }
}