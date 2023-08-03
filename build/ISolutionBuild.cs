using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;

using Serilog;

using static Nuke.Common.IO.FileSystemTasks;
using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface ISolutionBuild : ICommonParams {
    Target FullClean => _ => _
        .Executes(() => {
            RootDirectory.GlobDirectories("**/bin", "**/obj")
                .Where(item => item != (RootDirectory / "build" / "bin"))
                .Where(item => item != (RootDirectory / "build" / "obj"))
                .ForEach(item => item.DeleteDirectory());
            Output.CreateOrCleanDirectory();
        });

    Target FullCompile => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });

    Target FullPublish => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .DisableNoRestore()
                .SetOutputDirectory(Output)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(GitVersion, config)));
        });
}