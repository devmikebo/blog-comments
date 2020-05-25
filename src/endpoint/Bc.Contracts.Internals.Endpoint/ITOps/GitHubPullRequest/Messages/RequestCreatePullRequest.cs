namespace Bc.Contracts.Internals.Endpoint.ITOps.GitHubPullRequest.Messages
{
    public class RequestCreatePullRequest
    {
        public RequestCreatePullRequest(string branchName)
        {
            this.BranchName = branchName;
        }
    
        public string BranchName { get; }
    }
}