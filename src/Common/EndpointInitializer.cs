﻿namespace Common
{
    using System;
    using System.Data.SqlClient;
    using System.IO;
    using System.Net;
    using System.Net.Mail;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Mailer;
    using NServiceBus.Persistence.Sql;

    public class EndpointInitializer : IEndpointInitializer
    {
        private readonly IConfigurationManager configurationManager;

        public EndpointInitializer(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        public void Initialize(EndpointConfiguration endpointConfiguration)
        {
            // endpoint configuration
            endpointConfiguration.SendFailedMessagesTo(this.configurationManager.NsbErrorQueueName);
            endpointConfiguration.AuditProcessedMessagesTo(this.configurationManager.NsbAuditQueueName);
            endpointConfiguration.UseSerialization<JsonSerializer>();

            // conventions
            var conventions = endpointConfiguration.Conventions();
            conventions.DefiningCommandsAs(
                type =>
                {
                    return type.Namespace == typeof(CreateBranch).Namespace;
                });
            conventions.DefiningEventsAs(
                type =>
                {
                    return type.Namespace == typeof(IBranchCreated).Namespace;
                });

            // transport
            var transport = endpointConfiguration.UseTransport<SqlServerTransport>();
            transport.ConnectionString(this.configurationManager.NsbTransportConnectionString);
            var routing = transport.Routing();
            routing.RouteToEndpoint(
                assembly: typeof(CreateBranch).Assembly,
                destination: this.configurationManager.NsbEndpointName);
            routing.RegisterPublisher(
                assembly: typeof(IBranchCreated).Assembly,
                publisherEndpoint: this.configurationManager.NsbEndpointName);

            // persistence
            var persistence = endpointConfiguration.UsePersistence<SqlPersistence>();
            persistence.SqlVariant(SqlVariant.MsSqlServer);
            persistence.ConnectionBuilder(
                connectionBuilder: () =>
                {
                    return new SqlConnection(this.configurationManager.NsbTransportConnectionString);
                });

            var subscriptions = persistence.SubscriptionSettings();
            subscriptions.DisableCache();

            // outbox
            endpointConfiguration.EnableOutbox();

            // recoverability
            if (this.configurationManager.DevMode == DevMode.Dev)
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

            // mailer
            var mailSettings = endpointConfiguration.EnableMailer();
            mailSettings.UseSmtpBuilder(buildSmtpClient: () =>
            {
                var smtpClient = new SmtpClient();

                if (this.configurationManager.DevMode != DevMode.Production)
                {
                    var directoryLocation = Path.Combine(Environment.CurrentDirectory, "Emails");
                    Directory.CreateDirectory(directoryLocation);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = directoryLocation;
                }
                else
                {
                    smtpClient.Host = this.configurationManager.SmtpHost;
                    smtpClient.Port = this.configurationManager.SmtpPort;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(
                        this.configurationManager.SmtpHostUserName,
                        this.configurationManager.SmtpHostPassword);
                }

                return smtpClient;
            });

            // installers
            endpointConfiguration.EnableInstallers();
        }
    }
}
