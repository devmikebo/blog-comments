﻿namespace Components
{
    using System;
    using System.Text;
    using System.Threading.Tasks;
    using Components.GitHub;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;

    public class HandlerCreateBranch : IHandleMessages<CreateBranch>
    {
        private readonly IComponentsConfigurationManager componentsConfigurationManager;
        private readonly IGitHubApi gitHubApi;

        public HandlerCreateBranch(IComponentsConfigurationManager componentsConfigurationManager, IGitHubApi gitHubApi)
        {
            this.componentsConfigurationManager = componentsConfigurationManager;
            this.gitHubApi = gitHubApi;
        }

        public async Task Handle(CreateBranch message, IMessageHandlerContext context)
        {
            var sb = new StringBuilder();
            sb.Append(DateTime.UtcNow).Append(Guid.NewGuid());
            string branchName = sb.ToString();

            ////TODO: Is this idempotent ?
            ////TODO: Is this can be awaitable ?
            this.gitHubApi.CreateRepositoryBranch(
                this.componentsConfigurationManager.UserAgent,
                this.componentsConfigurationManager.AuthorizationToken,
                this.componentsConfigurationManager.RepositoryName,
                this.componentsConfigurationManager.MasterBranchName,
                branchName);

            await context.Publish<IBranchCreated>(evt => evt.CommentId = message.CommentId)
                .ConfigureAwait(false);
        }
    }
}
