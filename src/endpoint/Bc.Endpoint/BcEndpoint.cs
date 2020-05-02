using System.IO;
using System.Net;
using System.Net.Mail;
using System.Reflection;
using Bc.Common.Endpoint;
using Bc.Contracts.Internals.Endpoint;
using Bc.Contracts.Internals.Endpoint.Operations;
using Bc.Logic.Endpoint;
using NServiceBus;
using NServiceBus.Mailer;

namespace Bc.Endpoint
{
    public static class BcEndpoint
    {
        public static EndpointConfiguration GetEndpoint(IEndpointConfigurationProvider configurationProvider)
        {
            const string endpointName = "Bc.Endpoint";
            
            var endpoint = EndpointCommon.GetEndpoint(
                endpointName,
                false,
                new EndpointCommonConfigurationProvider());

            // routing
            var transport = endpoint.UseTransport<SqlServerTransport>();
            var routing = transport.Routing();
            routing.RouteToEndpoint(
                assembly: typeof(TakeCommentCmd).Assembly,
                destination: endpointName);
            
            // dependency injection
            endpoint.RegisterComponents(reg =>
            {
                reg.ConfigureComponent<EndpointConfigurationProvider>(DependencyLifecycle.InstancePerCall);
                reg.ConfigureComponent<RegisterCommentPolicyLogic>(DependencyLifecycle.InstancePerCall);
            });

            // mailer
            var mailSettings = endpoint.EnableMailer();
            mailSettings.UseSmtpBuilder(buildSmtpClient: () =>
            {
                var smtpClient = new SmtpClient();
            
                if (!configurationProvider.IsSendEmail)
                {
                    var directoryLocation = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "Emails");
                    Directory.CreateDirectory(directoryLocation);
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = directoryLocation;
                }
                else
                {
                    smtpClient.Host = configurationProvider.SmtpHost;
                    smtpClient.Port = configurationProvider.SmtpPort;
                    smtpClient.EnableSsl = true;
                    smtpClient.DeliveryMethod = SmtpDeliveryMethod.Network;
                    smtpClient.UseDefaultCredentials = false;
                    smtpClient.Credentials = new NetworkCredential(
                        configurationProvider.SmtpHostUserName,
                        configurationProvider.SmtpHostPassword);
                }
            
                return smtpClient;
            });            

            return endpoint;
        }
    }
}