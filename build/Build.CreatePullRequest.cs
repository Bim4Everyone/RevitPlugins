using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;

using Octokit;

using Serilog;

partial class Build {
    Target CreatePullRequest => _ => _
        .Requires(() => ExtensionsAppToken)
        .OnlyWhenStatic(() => IsServerBuild, $"Target should be run only on server.")
        .OnlyWhenDynamic(() => Params.PullRequestMerged, "Target works when pull request merged.")
        .OnlyWhenDynamic(() => GitHubActions.Instance.IsPullRequest, $"Target should be run only on pull request.")
        .Executes(async () => {
            var extensionsClient = GitHubExtensions.CreateGitHubClient(Params.ExtensionsAppToken);
            var revitPluginsClient = GitHubExtensions.CreateGitHubClient(Params.RevitPluginsAppToken);

            PullRequest pullRequest = await revitPluginsClient.GetCurrentPullRequest(Params);
            PullRequest createdPullRequest = await revitPluginsClient.CreatePullRequest(Params, pullRequest);

            await extensionsClient.ApprovePullRequest(createdPullRequest, Params);
            await revitPluginsClient.MergePullRequest(createdPullRequest, Params);
        });
}
