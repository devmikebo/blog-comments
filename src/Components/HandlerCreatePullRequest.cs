﻿namespace Components
{
    using System.Threading.Tasks;
    using Components.GitHub;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;

    public class HandlerCreatePullRequest : IHandleMessages<CreatePullRequest>
    {
        private readonly IComponentsConfigurationManager componentsConfigurationManager;
        private readonly IGitHubApi gitHubApi;

        public HandlerCreatePullRequest(IComponentsConfigurationManager componentsConfigurationManager, IGitHubApi gitHubApi)
        {
            this.componentsConfigurationManager = componentsConfigurationManager;
            this.gitHubApi = gitHubApi;
        }

        public async Task Handle(CreatePullRequest message, IMessageHandlerContext context)
        {
            var result = await this.gitHubApi.CreatePullRequest(
                this.componentsConfigurationManager.UserAgent,
                this.componentsConfigurationManager.AuthorizationToken,
                this.componentsConfigurationManager.RepositoryName,
                message.CommentBranchName,
                message.BaseBranchName).ConfigureAwait(false);

            await context.Publish<IPullRequestCreated>(evt =>
            {
                evt.CommentId = message.CommentId;
                evt.PullRequestLocation = result;
            })
            .ConfigureAwait(false);
        }
    }
}