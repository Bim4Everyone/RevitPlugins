using System.Collections.Generic;

using dosymep.Nuke.RevitVersions;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Components;

using Serilog;

[GitHubActions(
    "RevitPlugins",
    GitHubActionsImage.WindowsLatest,
    AutoGenerate = false,
    PublishArtifacts = false,
    EnableGitHubToken = true,
    OnPushIncludePaths = new[] { "RevitPlugins/**", "**.cs" }
)]
class Build : NukeBuild, ICompile, ICreateScript, IPluginCreate, ICreateBundle, ICreateProfile {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>();

    public IEnumerable<RevitVersion> BuildRevitVersions { get; set; }

    protected override void OnBuildInitialized() {
        base.OnBuildInitialized();
        var hazRevitVersion = this.From<IHazRevitVersion>();
        BuildRevitVersions = hazRevitVersion.RevitVersions.Length > 0
            ? hazRevitVersion.RevitVersions
            : RevitVersion.GetRevitVersions(hazRevitVersion.MinVersion, hazRevitVersion.MaxVersion);

        Log.Information("Build revit versions: {BuildRevitVersions}", BuildRevitVersions);
        Log.Information("Build plugin: {PluginName}", this.From<IHazPluginName>().PluginName);
        Log.Information("Plugin directory: {PluginDirectory}", this.From<IHazPluginName>().PluginDirectory);

        Log.Information("Output: {Output}", this.From<IHazOutput>().Output);

        Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
        Log.Information("IsServerBuild: {IsServerBuild}", IsServerBuild);

        Log.Information("Repository Url: {RepoUrl}", this.From<IHazGitRepository>().GitRepository.HttpsUrl);
        Log.Information("Repository Branch: {RepoBranch}", this.From<IHazGitRepository>().GitRepository.Branch);
    }
}
