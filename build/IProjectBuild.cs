using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface IProjectBuild : ICommonParams, IPluginParams {
    Target Clean => _ => _
        .Requires(() => PluginName)
        .Executes(() => {
            SourceDirectory.GlobDirectories("**/bin", "**/obj").ForEach(item => item.DeleteDirectory());
            Output.CreateOrCleanDirectory();
        });

    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetProjectFile(PluginName)
                .SetOutputDirectory(Output)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });

    Target Publish => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetProjectFile(PluginName)
                .SetOutputDirectory(Output)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });
}