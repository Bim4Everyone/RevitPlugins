using System;
using System.Collections.Generic;
using System.Linq;

using Nuke.Common;
using Nuke.Common.CI;
using Nuke.Common.Execution;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.EnvironmentInfo;
using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.IO.PathConstruction;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

class Build : NukeBuild {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(x => x.Compile);

    [Parameter("Output")] readonly string Output = RootDirectory / "bin";

    [Parameter("PluginName")] readonly string PluginName;

    [GitVersion] readonly GitVersion GitVersion;

    readonly IEnumerable<RevitConfiguration> Configurations = IsLocalBuild
        ? RevitConfiguration.GetDebugConfiguration()
        : RevitConfiguration.GetReleaseConfiguration();

    AbsolutePath SourceDirectory => RootDirectory / PluginName;

    Target Clean => _ => _
        .Requires(() => PluginName)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(DeleteDirectory);
            EnsureCleanDirectory(Output);
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .SetProjectFile(PluginName)
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(Configurations, (settings, config) => settings
                    .SetConfiguration(config)));
        });

    Target CompileVersion => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .SetProjectFile(PluginName)
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(Configurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetAssemblyVersion(UpdateMajorVersion(config, GitVersion.AssemblySemVer))
                    .SetFileVersion(UpdateMajorVersion(config, GitVersion.AssemblySemFileVer))
                    .SetInformationalVersion(UpdateMajorVersion(config, GitVersion.InformationalVersion))));
        });

    string UpdateMajorVersion(RevitConfiguration configuration, string versionString) {
        var index = versionString.IndexOf('.');
        return configuration.Version + versionString.Substring(index);
    }
}