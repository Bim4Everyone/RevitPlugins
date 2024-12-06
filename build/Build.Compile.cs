using System;

using Nuke.Common;
using Nuke.Common.Tooling;
using Nuke.Common.Tools.DotNet;
using Nuke.Common.Utilities;
using Nuke.Common.Utilities.Collections;
using Nuke.Components;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build {
    Target Compile => _ => _
        .DependsOn(Clean)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            Log.Debug("Params.Output: {Output}", Params.Output);

            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
                .AddPairWhenValueNotNull(nameof(Params.Configuration), Params.Configuration)
                .AddPairWhenValueNotNull(nameof(Params.Output), Params.Output)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile("src/" + PluginName)
                .SetConfiguration(Params.Configuration)
                .CombineWith(Params.BuildRevitVersions, (settings, revitVersion) => settings
                    .SetSimpleVersion(Params, revitVersion)
                    .SetProperty("RevitVersion", (int) revitVersion)
                    .SetProperty("OutputPath", Params.Output)
                    .SetProperty("AssemblyName", $"{PluginName}_{revitVersion}")));
        });

    Target Publish => _ => _
        .DependsOn(Clean, CloneRepos)
        .Triggers(PublishArtifacts)
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .Executes(() => {
            string publishDirectory = NukeBuildExtensions.GetExtensionsPath(PublishDirectory);
            Log.Debug("publishDirectory: {PublishDirectory}", publishDirectory);

            ReportSummary(_ => _
                .AddPairWhenValueNotNull(nameof(PluginName), PluginName)
                .AddPairWhenValueNotNull(nameof(Params.Configuration), Params.Configuration)
                .AddPairWhenValueNotNull(nameof(PublishDirectory), PublishDirectory)
            );

            DotNetBuild(s => s
                .Apply(CompileSettingsBase)
                .SetProjectFile("src/" + PluginName)
                .SetConfiguration(Params.Configuration)
                .CombineWith(Params.BuildRevitVersions, (settings, revitVersion) => settings
                    .SetSimpleVersion(Params, revitVersion)
                    .SetProperty("RevitVersion", (int) revitVersion)
                    .SetProperty("OutputPath", publishDirectory)
                    .SetProperty("AssemblyName", $"{PluginName}_{revitVersion}")));
        });

    Configure<DotNetBuildSettings> CompileSettingsBase => _ => _
        .DisableNoRestore()
        .SetOutputDirectory(Output)
        .SetCopyright($"Copyright © {DateTime.Now.Year}")
        .When(settings => IsServerBuild,
            _ => _
                .EnableContinuousIntegrationBuild())
        .WhenNotNull(this, (_, o) => _
            .SetRepositoryUrl(o.GitRepository.HttpsUrl));
}
