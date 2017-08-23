﻿namespace Components
{
    using System;
    using System.Net;
    using System.Net.Mail;
    using System.Text;
    using Messages;

    public class EmailSender : IEmailSender
    {
        private readonly IComponentsConfigurationManager componentsConfigurationManager;

        public EmailSender(IComponentsConfigurationManager componentsConfigurationManager)
        {
            this.componentsConfigurationManager = componentsConfigurationManager;
        }

        public void Send(string userName, string userEmail, CommentResponseStatus status)
        {
            var client = new SmtpClient()
            {
                Host = this.componentsConfigurationManager.SmtpHost,
                Port = this.componentsConfigurationManager.SmtpPort,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(
                    this.componentsConfigurationManager.SmtpHostUserName,
                    this.componentsConfigurationManager.SmtpHostPassword)
            };

            ////TODO: to implement
            MailMessage mm = new MailMessage(
                this.componentsConfigurationManager.SmtpFrom,
                userEmail,
                "[testSubject]",
                this.GetBody(userName, status))
            {
                BodyEncoding = Encoding.UTF8
            };

            client.Send(mm);
        }

        public string GetBody(string userName, CommentResponseStatus status)
        {
            ////TOTO: to implement
            return "test mail body: " + userName + " " + status;
        }
    }
}
