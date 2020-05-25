using System.Security;

namespace Bc.Contracts.Internals.Endpoint
{
    public interface IEndpointConfigurationProvider
    {
        bool IsUseFakes { get; }

        bool IsSendEmail { get; }
        
        string SmtpHost { get; }

        int SmtpPort { get; }

        string SmtpHostUserName { get; }

        SecureString SmtpHostPassword { get; }

        string SmtpFrom { get; }

        int CheckCommentAnswerTimeoutInSeconds { get; }
    }
}