﻿namespace Components
{
    using System;
    using System.Threading.Tasks;
    using Common;
    using Messages;
    using Messages.Commands;
    using Messages.Events;
    using NServiceBus;
    using NServiceBus.Logging;

    public class HandlerCommentSaga :
       Saga<CommentSagaData>,
        IAmStartedByMessages<StartAddingComment>,
        IHandleMessages<IBranchCreated>,
        IHandleMessages<ICommentAdded>,
        IHandleMessages<IPullRequestCreated>,
        IHandleTimeouts<CheckCommentResponseTimeout>,
        IHandleMessages<ICommentResponseAdded>
    {
        private readonly IConfigurationManager configurationManager;
        private readonly ILog log = LogManager.GetLogger<HandlerCommentSaga>();

        public HandlerCommentSaga()
        {
            ////for unit tests only
        }

        public HandlerCommentSaga(IConfigurationManager configurationManager)
        {
            this.configurationManager = configurationManager;
        }

        public Task Handle(StartAddingComment message, IMessageHandlerContext context)
        {
            this.Data.CommentId = message.CommentId;
            this.Data.UserName = message.UserName;
            this.Data.UserEmail = message.UserEmail;
            this.Data.UserWebsite = message.UserWebsite;
            this.Data.FileName = message.FileName;
            this.Data.Content = message.Content;

            return context.Send<CreateBranch>(command => command.CommentId = this.Data.CommentId);
        }

        public Task Handle(IBranchCreated message, IMessageHandlerContext context)
        {
            this.Data.BranchName = message.CreatedBranchName;

            return context.Send<AddComment>(command =>
             {
                 command.CommentId = this.Data.CommentId;
                 command.UserName = this.Data.UserName;
                 command.BranchName = this.Data.BranchName;
                 command.FileName = this.Data.FileName;
                 command.Content = this.Data.Content;
             });
        }

        public Task Handle(ICommentAdded message, IMessageHandlerContext context)
        {
            return context.Send<CreatePullRequest>(command =>
             {
                 command.CommentId = this.Data.CommentId;
                 command.CommentBranchName = this.Data.BranchName;
                 command.BaseBranchName = this.configurationManager.MasterBranchName;
             });
        }

        public Task Handle(IPullRequestCreated message, IMessageHandlerContext context)
        {
            this.Data.PullRequestLocation = message.PullRequestLocation;

            return this.SendTimeout(
                context,
                TimeSpan.FromSeconds(this.configurationManager.CommentResponseAddedSagaTimeoutInSeconds),
                this.Data.CommentId);
        }

        public Task Timeout(CheckCommentResponseTimeout state, IMessageHandlerContext context)
        {
            return context.Send<CheckCommentResponse>(command =>
            {
                command.CommentId = this.Data.CommentId;
                command.PullRequestUri = this.Data.PullRequestLocation;
                command.Etag = this.Data.ETag;
            });
        }

        public async Task Handle(ICommentResponseAdded message, IMessageHandlerContext context)
        {
            if (message.CommentResponse.ResponseStatus == CommentResponseStatus.Approved ||
                message.CommentResponse.ResponseStatus == CommentResponseStatus.Rejected)
            {
                await context.Send<SendEmail>(command =>
                {
                    command.UserName = this.Data.UserName;
                    command.UserEmail = this.Data.UserEmail;
                    command.FileName = this.Data.FileName;
                    command.CommentResponseStatus = message.CommentResponse.ResponseStatus;
                }).ConfigureAwait(false);

                this.MarkAsComplete();
            }
            else
            {
                this.Data.ETag = message.CommentResponse.ETag;

                await this.SendTimeout(
                    context,
                    TimeSpan.FromSeconds(this.configurationManager.CommentResponseAddedSagaTimeoutInSeconds),
                    this.Data.CommentId).ConfigureAwait(false);
            }
        }

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<CommentSagaData> mapper)
        {
            mapper.ConfigureMapping<StartAddingComment>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
            mapper.ConfigureMapping<IBranchCreated>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
            mapper.ConfigureMapping<ICommentAdded>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
            mapper.ConfigureMapping<IPullRequestCreated>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
            mapper.ConfigureMapping<CheckCommentResponseTimeout>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
            mapper.ConfigureMapping<ICommentResponseAdded>(message => message.CommentId).ToSaga(sagaData => sagaData.CommentId);
        }

        private Task SendTimeout(IMessageHandlerContext context, TimeSpan timeoutInterval, Guid commentId)
        {
            return this.RequestTimeout(
                context,
                timeoutInterval,
                new CheckCommentResponseTimeout { CommentId = commentId });
        }
    }
}
