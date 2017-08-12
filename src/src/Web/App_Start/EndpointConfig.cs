﻿using Autofac;
using Autofac.Integration.Mvc;
using Components;
using Components.GitHub;
using Messages.Commands;
using Messages.Events;
using NServiceBus;
using NServiceBus.Persistence.Sql;
using System.Data.SqlClient;
using System.Web.Mvc;

namespace Web
{
    public class EndpointConfig
    {
        public static void RegisterEndpoint(IEndpointInstance endpoint)
        {
            var configurationManager = new ConfigurationManager();
            var builder = new ContainerBuilder();

            // container
            builder.RegisterControllers(typeof(MvcApplication).Assembly);
            builder.Register(ctx => endpoint).SingleInstance();
            var container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));

            var endpointConfiguration = new EndpointConfiguration(configurationManager.NsbEndpointName);

            // container
            endpointConfiguration.UseContainer<AutofacBuilder>(
                customizations: customizations =>
                {
                    customizations.ExistingLifetimeScope(container);
                });
            RegisterComponents(endpointConfiguration, configurationManager);

            // error & audit
            endpointConfiguration.SendFailedMessagesTo(configurationManager.NsbErrorQueueName);
            endpointConfiguration.AuditProcessedMessagesTo(configurationManager.NsbAuditQueueName);

            // callbacks
            endpointConfiguration.EnableCallbacks();
            endpointConfiguration.MakeInstanceUniquelyAddressable(configurationManager.NsbEndpointInstanceId);

            // serialization
            endpointConfiguration.UseSerialization<JsonSerializer>();

            // convenstions
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(
                type =>
                {
                    return type.Namespace == "Messages.Commands";
                });
            conventions.DefiningEventsAs(
                type =>
                {
                    return type.Namespace == "Messages.Events";
                });

            // transport
            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(configurationManager.NsbTransportConnectionString);
            var routing = transport.Routing();
            routing.RouteToEndpoint(
                assembly: typeof(CreateBranch).Assembly,
                destination: configurationManager.NsbEndpointName);
            routing.RegisterPublisher(
                assembly: typeof(IBranchCreated).Assembly,
                publisherEndpoint: configurationManager.NsbEndpointName);

            // persistence
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlVariant(SqlVariant.MsSqlServer);
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(configurationManager.NsbTransportConnectionString);
                });

            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.DisableCache();

            // outbox
            endpointConfiguration.EnableOutbox();

            // recoverability
            if (configurationManager.DevMode == DevMode.Dev)
            {
                var recoverability = endpointConfiguration.Recoverability();
                recoverability.Immediate(
                    immediate =>
                    {
                        immediate.NumberOfRetries(0);
                    });
                recoverability.Delayed(
                    delayed =>
                    {
                        delayed.NumberOfRetries(0);
                    });
            }

            // installers
            endpointConfiguration.EnableInstallers();

            endpoint = Endpoint.Start(endpointConfiguration).GetAwaiter().GetResult();
        }

        public static void RegisterComponents(
            EndpointConfiguration endpointConfiguration,
            IConfigurationManager configurationManager)
        {

            endpointConfiguration.RegisterComponents(
                registration: configureComponents =>
                {
                    configureComponents.ConfigureComponent<ConfigurationManager>(DependencyLifecycle.InstancePerCall);
                });

            endpointConfiguration.RegisterComponents(
                registration: configureComponents =>
                {
                    configureComponents.ConfigureComponent<ComponentsConfigurationManager>(DependencyLifecycle.InstancePerCall);
                });

            if (configurationManager.NsbIsIntegrationTests)
            {
                endpointConfiguration.RegisterComponents(
                    registration: configureComponents =>
                    {
                        configureComponents.ConfigureComponent<GitHubApiForTests>(DependencyLifecycle.InstancePerCall);
                    });

                endpointConfiguration.RegisterComponents(
                    registration: configureComponents =>
                    {
                        configureComponents.ConfigureComponent<SendEmailForTests>(DependencyLifecycle.InstancePerCall);
                    });
            }
            else
            {
                ////TODO: use real api
            }
        }
    }
}