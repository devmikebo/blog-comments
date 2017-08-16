﻿namespace Components.Integration.Tests
{
    using System;
    using System.Threading.Tasks;
    using Components.GitHub;
    using NUnit.Framework;

    [TestFixture]
    [Ignore("only for manual tests")]
    public class GitHubApiTests
    {
        private readonly IComponentsConfigurationManager configurationComponentsManager =
            new ComponentsConfigurationManager();

        [Test]
        public async Task GetRepository_Execute_ProperResult()
        {
            // Arrange
            var api = this.GetGitHubApi();

            // Act
            var result = await api.GetRepository(
                this.configurationComponentsManager.UserAgent,
                this.configurationComponentsManager.AuthorizationToken,
                this.configurationComponentsManager.RepositoryName,
                this.configurationComponentsManager.MasterBranchName).ConfigureAwait(false);

            // Assert
            Assert.NotNull(result.Ref);
            Assert.NotNull(result.Url);
            Assert.NotNull(result.Object.Sha);

            Console.WriteLine(string.Format("{0} {1} {2}", result.Ref, result.Url, result.Object.Sha));
        }

        [Test]
        public async Task CreateRepositoryBranch_Execute_ProperResult()
        {
            // Arrange
            const string newBranchName = "c-15";
            var api = this.GetGitHubApi();

            // Act
            await api.CreateRepositoryBranch(
                this.configurationComponentsManager.UserAgent,
                this.configurationComponentsManager.AuthorizationToken,
                this.configurationComponentsManager.RepositoryName,
                this.configurationComponentsManager.MasterBranchName,
                newBranchName).ConfigureAwait(false);

            // Assert
            var result = await api.GetRepository(
                this.configurationComponentsManager.UserAgent,
                this.configurationComponentsManager.AuthorizationToken,
                this.configurationComponentsManager.RepositoryName,
                newBranchName).ConfigureAwait(false);

            Assert.NotNull(result.Ref);
            Assert.NotNull(result.Url);
            Assert.NotNull(result.Object.Sha);

            Console.WriteLine(string.Format("{0} {1} {2}", result.Ref, result.Url, result.Object.Sha));
        }

        [Test]
        public Task UpdateFile_Execute_ProperResult()
        {
            // Arrange
            const string branchName = "c-14";
            var api = this.GetGitHubApi();

            // Act
            return api.UpdateFile(
                this.configurationComponentsManager.UserAgent,
                this.configurationComponentsManager.AuthorizationToken,
                this.configurationComponentsManager.RepositoryName,
                branchName,
                "test.txt",
                "\nnew comment - " + DateTime.Now);

            // Assert
            ////TODO: to implement
        }

        [Test]
        public Task CreatePullRequest_Execute_ProperResult()
        {
            // Arrange
            const string branchName = "c-14";
            var api = this.GetGitHubApi();

            // Act
            return api.CreatePullRequest(
                this.configurationComponentsManager.UserAgent,
                this.configurationComponentsManager.AuthorizationToken,
                this.configurationComponentsManager.RepositoryName,
                branchName,
                "master");

            // Assert
            ////TODO: to implement
        }

        private GitHubApi GetGitHubApi()
        {
            return new GitHubApi();
        }
    }
}
