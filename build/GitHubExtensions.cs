using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Nuke.Common.CI.GitHubActions;

using Octokit;

using Serilog;

static class GitHubExtensions {
    public static GitHubClient CreateGitHubClient(string appToken) {
        return new GitHubClient(new ProductHeaderValue("app")) {Credentials = new Credentials(appToken)};
    }

    public static async Task<PullRequest> GetCurrentPullRequest(
        this GitHubClient client, Build.BuildParams buildParams) {
        // ReSharper disable once PossibleInvalidOperationException
        int pullRequestNumber = GitHubActions.Instance.PullRequestNumber.Value;
        return await client.PullRequest.Get(
            buildParams.OrganizationName, buildParams.CurrentRepoName, pullRequestNumber);
    }

    public static async Task<PullRequest> CreatePullRequest(
        this GitHubClient client,
        Build.BuildParams buildParams, PullRequest pullRequest) {

        Log.Debug("Repository: {@Organization}/{@RepoName}",
            buildParams.OrganizationName, buildParams.RepoName);

        Repository repository = await client.Repository.Get(
            buildParams.OrganizationName, buildParams.RepoName);

        Reference nukeBranch = await client.Git.Reference.Get(
            buildParams.OrganizationName, buildParams.RepoName, "heads/" + buildParams.NukeBranchName);

        Reference defaultBranch = await client.Git.Reference.Get(
            buildParams.OrganizationName, buildParams.RepoName, "heads/" + buildParams.MasterBranchName);

        Log.Debug("Pull Request: {@Title} (@{PRNumber}) {@DefaultRef} from {@NukeRef}",
            pullRequest.Title, pullRequest.Number, nukeBranch.Ref, defaultBranch.Ref);

        PullRequest createdPullRequest = await client.PullRequest.Create(
            repository.Id,
            new NewPullRequest(pullRequest.Title, nukeBranch.Ref, defaultBranch.Ref));

        Log.Debug("Update Pull Request");

        await client.PullRequest.Update(
            buildParams.OrganizationName,
            buildParams.RepoName,
            createdPullRequest.Number,
            new PullRequestUpdate() {
                Body = pullRequest.Body + $"{Environment.NewLine}------{Environment.NewLine}{pullRequest.HtmlUrl}"
            });

        Log.Debug("Copy assignee Pull Request");
        await client.CopyAssignee(createdPullRequest, buildParams);

        return createdPullRequest;
    }

    public static async Task ApprovePullRequest(
        this GitHubClient client,
        PullRequest pullRequest,
        Build.BuildParams buildParams) {

        Log.Debug("Review approve");

        await client.PullRequest.Review.Create(
            buildParams.OrganizationName,
            buildParams.RepoName, pullRequest.Number,
            new PullRequestReviewCreate() {Body = "Отлично, так держать!", Event = PullRequestReviewEvent.Approve});
    }

    public static async Task MergePullRequest(
        this GitHubClient client,
        PullRequest pullRequest,
        Build.BuildParams buildParams) {
        Reference nukeBranch = await client.Git.Reference.Get(
            buildParams.OrganizationName, buildParams.RepoName, "heads/" + buildParams.NukeBranchName);

        Log.Debug("Merge Pull Request");

        await client.PullRequest.Merge(
            buildParams.OrganizationName,
            buildParams.RepoName,
            pullRequest.Number,
            new MergePullRequest() {
                MergeMethod = PullRequestMergeMethod.Squash, CommitTitle = pullRequest.Title + $" (#{pullRequest.Number})"
            });

        Log.Debug("Delete branch {@NukeRef}", nukeBranch.Ref);
        await client.Git.Reference.Delete(buildParams.OrganizationName, buildParams.RepoName, nukeBranch.Ref);
    }

    public static async Task CopyAssignee(this GitHubClient client,
        PullRequest pullRequest,
        Build.BuildParams buildParams) {
        await client.Issue.Assignee.AddAssignees(buildParams.OrganizationName,
            buildParams.RepoName,
            pullRequest.Number,
            new AssigneesUpdate(pullRequest.Assignees.Select(item => item.Login).ToList()));
    }
}
