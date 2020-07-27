module Bc.Endpoint.Tests.CommentRegistrationPolicy

open System
open Bc.Contracts.Externals.Endpoint.CommentRegistration.Events
open Bc.Contracts.Internals.Endpoint.CommentRegistration.Commands
open Bc.Contracts.Internals.Endpoint.CommentRegistration.Logic
open Bc.Contracts.Internals.Endpoint.GitHubPullRequestCreation.Messages
open Bc.Endpoint
open NServiceBus.Testing
open NSubstitute
open NUnit.Framework

let getContext() =
    TestableMessageHandlerContext()

module CommentRegistrationPolicyTests =

    let logic = Substitute.For<ICommentRegistrationPolicyLogic>()

    let getPolicy data =
        CommentRegistrationPolicy(logic, Data = data)

    [<Test>]
    let Handle_RegisterComment_ProperResult () =

        // Arrange
        let commentId = Guid.NewGuid()
        let userName = "sample_user_name"
        let userWebsite = "sample_user_website"
        let userComment = "sample_user_comment"
        let articleFileName = "article_file_name"
        let commentAddedDate = DateTime(2020, 7, 25)
        let message = RegisterComment(commentId, userName, userWebsite, userComment, articleFileName, commentAddedDate)

        let policyData = CommentRegistrationPolicy.PolicyData()
        let policy = getPolicy policyData
        let context = getContext ()

        // Act
        policy.Handle(message, context) |> ignore

        // Assert
        let sentNumberOfMessages = context.SentMessages.Length
        let sentMessage = context.SentMessages.[0].Message :?> RequestCreateGitHubPullRequest

        Assert.That(sentNumberOfMessages, Is.EqualTo(1))
        Assert.That(sentMessage.CommentId, Is.EqualTo(commentId))
        Assert.That(sentMessage.FileName, Is.EqualTo(articleFileName))
        Assert.That(sentMessage.Content, Is.EqualTo(userComment))
        Assert.That(sentMessage.AddedDate, Is.EqualTo(commentAddedDate))

    [<Test>]
    let Handle_ResponseCreateGitHubPullRequest_ProperResult () =

        // Arrange
        let commentId = Guid.NewGuid()
        let pullRequestUri = "uri_123"
        let message = ResponseCreateGitHubPullRequest(commentId, pullRequestUri)

        let policyData = CommentRegistrationPolicy.PolicyData(CommentId = commentId)
        let policy = getPolicy policyData
        let context = getContext ()

        // Act
        policy.Handle(message, context) |> ignore

        // Assert
        let publishedNumberOfMessages = context.PublishedMessages.Length
        let publishedMessage = context.PublishedMessages.[0].Message :?> CommentRegistered

        Assert.That(publishedNumberOfMessages, Is.EqualTo(1))
        Assert.That(publishedMessage.CommentId, Is.EqualTo(commentId))
        Assert.That(publishedMessage.CommentUri, Is.EqualTo(pullRequestUri))
        Assert.That(policy.Completed, Is.True)