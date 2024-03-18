using System.Threading.Tasks;

using Nuke.Common.CI.GitHubActions;

using Octokit;

using Serilog;

static class GitHubExtensions {
    public static async Task<PullRequest> GetCurrentPullRequest(
        this GitHubClient client, Build.BuildParams buildParams) {
        // ReSharper disable once PossibleInvalidOperationException
        int pullRequestNumber = GitHubActions.Instance.PullRequestNumber.Value;
        return await client.PullRequest.Get(
            buildParams.OrganizationName, buildParams.CurrentRepoName, pullRequestNumber);
    }

    public static async Task CreatePullRequest(
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
            new PullRequestUpdate() {Body = pullRequest.Body});

        Log.Debug("Review approve");
        
        await client.PullRequest.Review.Create(
            buildParams.OrganizationName,
            buildParams.RepoName, createdPullRequest.Number,
            new PullRequestReviewCreate() {Body = "Отлично, так держать!", Event = PullRequestReviewEvent.Approve});

        Log.Debug("Merge Pull Request");

        await client.PullRequest.Merge(
            buildParams.OrganizationName,
            buildParams.RepoName,
            createdPullRequest.Number,
            new MergePullRequest() {MergeMethod = PullRequestMergeMethod.Squash, CommitTitle = pullRequest.Title});

        Log.Debug("Delete branch {@NukeRef}", nukeBranch.Ref);
        await client.Git.Reference.Delete(buildParams.OrganizationName, buildParams.RepoName, nukeBranch.Ref);
    }
}
