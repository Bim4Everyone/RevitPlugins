using System;

using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface ICompile : IClean,
    IHazSolution,
    IHazGitVersion,
    IHazGitRepository,
    IHazRevitVersion,
    IHazConfigurations,
    IPyRevitInstall,
    ICloneRepos,
    IPublishDll {
    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .Executes(() => {
            string publishDirectory = NukeBuildExtensions.GetExtensionsPath(PublishDirectory);
            Log.Debug("publishDirectory: {PublishDirectory}", publishDirectory);

            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
                .AddPairWhenValueNotNull(nameof(PublishDirectory), PublishDirectory)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile("src/" + PluginName)
                .SetConfiguration(Configuration.Debug)
                .SetOutputDirectory(publishDirectory)
                .CombineWith(BuildRevitVersions, (settings, revitVersion) => settings
                    .SetSimpleVersion(Versioning, revitVersion)
                    .SetProperty("RevitVersion", (int) revitVersion)
                    .SetProperty("AssemblyName", $"{PluginName}_{revitVersion}")));
        });

    Target Publish => _ => _
        .DependsOn(Clean, InstallPyRevit, CloneRepos)
        .Triggers(PushPluginDll)
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .Executes(() => {
            string publishDirectory = NukeBuildExtensions.GetExtensionsPath(PublishDirectory);
            Log.Debug("publishDirectory: {PublishDirectory}", publishDirectory);

            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
                .AddPairWhenValueNotNull(nameof(PublishDirectory), PublishDirectory)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile("src/" + PluginName)
                .SetConfiguration(Configuration.Release)
                .SetOutputDirectory(publishDirectory)
                .CombineWith(BuildRevitVersions, (settings, revitVersion) => settings
                    .SetSimpleVersion(Versioning, revitVersion)
                    .SetProperty("RevitVersion", (int) revitVersion)
                    .SetProperty("AssemblyName", $"{PluginName}_{revitVersion}")));
        })
    ;

    sealed Configure<DotNetBuildSettings> CompileSettingsBase => _ => _
        .DisableNoRestore()
        .SetOutputDirectory(Output)
        .SetCopyright($"Copyright © {DateTime.Now.Year}")
        .When(IsServerBuild, _ => _
            .EnableContinuousIntegrationBuild())
        .WhenNotNull(this as IHazGitRepository, (_, o) => _
            .SetRepositoryUrl(o.GitRepository.HttpsUrl));
}
