namespace Bc.Logic.Endpoint.GitHubPullRequestVerification

open Bc.Contracts.Internals.Endpoint.GitHubPullRequestVerification
open Bc.Contracts.Internals.Endpoint.GitHubPullRequestVerification.Messages

type PolicyLogic() =
    interface IPolicyLogic with
        member this.CheckPullRequestStatus pullRequestUri etag =
            async {
                let! isOpenResult = GitHubApi.IsPullRequestOpen.execute
                                        GitHubConfigurationProvider.userAgent
                                        GitHubConfigurationProvider.authorizationToken
                                        pullRequestUri
                                        (Some etag)

                if isOpenResult.isOpen then
                    return ResponseCheckPullRequestStatus(PullRequestStatus.Open, isOpenResult.etag)
                else
                    let! isMerged = GitHubApi.IsPullRequestMerged.execute
                                            GitHubConfigurationProvider.userAgent
                                            GitHubConfigurationProvider.authorizationToken
                                            pullRequestUri
                    if isMerged then
                        return ResponseCheckPullRequestStatus(PullRequestStatus.Merged, isOpenResult.etag)
                    else
                        return ResponseCheckPullRequestStatus(PullRequestStatus.Closed, isOpenResult.etag)
            } |> Async.StartAsTask
            
type PolicyLogicFake() =
    interface IPolicyLogic with
        member this.CheckPullRequestStatus _ _ =
            async {
                return ResponseCheckPullRequestStatus(PullRequestStatus.Merged, "etag_123")
            } |> Async.StartAsTask
            