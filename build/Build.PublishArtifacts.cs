using System.Linq;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;

partial class Build {
    Target PublishArtifacts => _ => _
        .Triggers(CreatePullRequest)
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .OnlyWhenDynamic(() => Params.PullRequestMerged, "Target works when pull request merged.")
        .OnlyWhenDynamic(() => GitHubActions.Instance.IsPullRequest, $"Target should be run only on pull request.")
        .OnlyWhenDynamic(() => Params.ExtensionDirectory.DirectoryExists(), "ExtensionDirectory does not exists")
        .Executes(() => {
            Log.Debug("Set git user");
            Git($"-C \"{Params.ExtensionDirectory}\" config --global user.name \"dosymep\"");
            Git($"-C \"{Params.ExtensionDirectory}\" config --global user.email \"dosymep@gmail.com\"");

            Log.Debug("Execute git commands in directory: {@ExtensionDirectory}", Params.ExtensionDirectory.ToString());
            Git($"-C \"{Params.ExtensionDirectory}\" switch -c {Params.NukeBranchName} -q");
            Git($"-C \"{Params.ExtensionDirectory}\" status");

            Log.Debug("Commit updated *.dll");
            Git($"-C \"{Params.ExtensionDirectory}\" add *.dll");
            Git($"-C \"{Params.ExtensionDirectory}\" status");
            Git($"-C \"{Params.ExtensionDirectory}\" commit -m " +
                $"\"Обновление библиотек плагина \"\"\"{Params.PluginName}\"\"\"\"");

            Log.Debug("Push changes to origin");
            Git($"-C \"{Params.ExtensionDirectory}\" push -q -u origin {Params.NukeBranchName}");
        });
}
