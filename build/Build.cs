using System;
using System.Collections.Generic;
using System.IO;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

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
class Build : NukeBuild, IProjectBuild, ISolutionBuild, IProjectCreate {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => ((ISolutionBuild)x).FullCompile);
}