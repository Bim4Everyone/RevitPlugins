using System;
using System.Linq;

using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Tools.GitVersion;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface ICompile : IClean, IHazSolution, IHazGitVersion, IHazGitRepository, IHazConfigurations {
    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(Output), Output)
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile(PluginName)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(Versioning, config)));
        });

    Target Publish => _ => _
        .DependsOn(Clean)
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .Executes(() => {
            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
                .AddPairWhenValueNotNull(nameof(PublishDirectory), PublishDirectory)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile(PluginName)
                .SetOutputDirectory(PublishDirectory)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(Versioning, config)));
        });

    Target FullCompile => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile(Solution)
                .CombineWith(DebugConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(Versioning, config)));
        });
    
    Target FullPublish => _ => _
        .DependsOn(FullClean)
        .Requires(() => Output)
        .Executes(() => {
            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile(Solution)
                .CombineWith(ReleaseConfigurations, (settings, config) => settings
                    .SetConfiguration(config)
                    .SetSimpleVersion(Versioning, config)));
        });

    sealed Configure<DotNetBuildSettings> CompileSettingsBase => _ => _
        .DisableNoRestore()
        .SetOutputDirectory(Output)
        .SetCopyright($"Copyright © {DateTime.Now.Year}")
        .When(IsServerBuild, _ => _
            .EnableContinuousIntegrationBuild())
        .WhenNotNull(this as IHazGitRepository, (_, o) => _
            .SetRepositoryUrl(o.GitRepository.HttpsUrl));
}