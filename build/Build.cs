using System;
using System.Collections.Generic;
using System.IO;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;

using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

[GitHubActions(
    "RevitPlugins",
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = false,
    PublishArtifacts = false,
    EnableGitHubToken = true,
    OnPushIncludePaths = new[] {"RevitPlugins/**", "**.cs"}
)]
class Build : NukeBuild, ICompile, ICreateScript, IPluginCreate, ICreateBundle, ICreateProfile {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>();

    protected override void OnBuildInitialized() {
        Log.Information("Build plugin: {PluginName}", this.From<IHazPluginName>().PluginName);
        Log.Information("Plugin directory: {PluginDirectory}", this.From<IHazPluginName>().PluginDirectory);

        Log.Information("Output: {Output}", this.From<IHazOutput>().Output);

        Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
        Log.Information("IsServerBuild: {IsServerBuild}", IsServerBuild);

        Log.Information("Repository Url: {RepoUrl}", this.From<IHazGitRepository>().GitRepository.HttpsUrl);
        Log.Information("Repository Branch: {RepoBranch}", this.From<IHazGitRepository>().GitRepository.Branch);
    }
}