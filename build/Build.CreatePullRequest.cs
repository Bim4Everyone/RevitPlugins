using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;

using Octokit;

using Serilog;

partial class Build {
    Target CreatePullRequest => _ => _
        .OnlyWhenStatic(() => IsServerBuild, $"Target should be run only on server.")
        .OnlyWhenDynamic(() => Params.PullRequestMerged, "Target works when pull request merged.")
        .OnlyWhenDynamic(() => GitHubActions.Instance.IsPullRequest, $"Target should be run only on pull request.")
        .Executes(async () => {
            var client = new GitHubClient(new ProductHeaderValue(Params.CurrentRepoName));
            client.Credentials = new Credentials(Params.ExtensionsAppToken);

            Log.Debug("Create pull request");
            await client.CreatePullRequest(Params, await client.GetCurrentPullRequest(Params));
        });
}
