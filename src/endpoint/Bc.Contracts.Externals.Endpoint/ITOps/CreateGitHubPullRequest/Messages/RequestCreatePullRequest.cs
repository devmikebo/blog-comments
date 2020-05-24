using System;

namespace Bc.Contracts.Externals.Endpoint.ITOps.CreateGitHubPullRequest.Messages
{
    public class RequestCreatePullRequest
    {
        public RequestCreatePullRequest(
            Guid commentId,
            string userName,
            string userWebSite,
            string fileName,
            string content,
            DateTime addedDate)
        {
            this.CommentId = commentId;
            this.UserName = userName;
            this.UserWebSite = userWebSite;
            this.FileName = fileName;
            this.Content = content;
            this.AddedDate = addedDate;
        }

        public Guid CommentId { get; set; }
        public string UserName { get; }

        public string UserWebSite { get; }

        public string FileName { get; }

        public string Content { get; }

        public DateTime AddedDate { get; }        
    }
}