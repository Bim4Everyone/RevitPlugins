using System.Collections.Generic;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build {
    Target CreatePlugin => _ => _
        .Triggers(CreateBranch)
        .DependsOn(CreateBundle, CreateWorkflow, CreateProfile)
        .OnlyWhenDynamic(() => !ProjectExistInSolution(PluginName), $"Plugin \"{PluginName}\" does exists.")
        .OnlyWhenDynamic(() => !Params.PluginDirectory.DirectoryExists(), $"Plugin directory \"{PluginName}\" does exists.")
        .Executes(() => {
            Log.Debug("PluginType: {PluginType}", Params.PluginType);
            Log.Debug("TemplateName: {TemplateName}", Params.TemplateName);
            Log.Debug("TemplateDirectory: {TemplateDirectory}", Params.TemplateDirectory);
            Log.Debug("TemplateProjectDirectory: {TemplateProjectDirectory}", Params.TemplateProjectDirectory);

            Log.Debug("PluginFile: {PluginFile}", Params.PluginFile);
            Log.Debug("PluginDirectory: {PluginDirectory}", Params.PluginDirectory);

            Log.Debug("HazProject: {HazProject}", ProjectExistInSolution(PluginName));
            Log.Debug("HazDirectory: {HazDirectory}", Params.PluginDirectory.Exists());

            CopyDirectory(Params.TemplateProjectDirectory, Params.PluginDirectory,
                new Dictionary<string, string>() { { "${{ gen.bundle_name }}", BundleName ?? "Название плагина" } });
            AbsolutePath templatePluginType = Params.TemplateDirectory / Params.PluginType;
            CopyDirectory(templatePluginType, Params.PluginDirectory, new Dictionary<string, string>());

            DotNet(arguments: $"sln add {Params.PluginFile} --in-root");
        });

    private bool ProjectExistInSolution(string projectName) {
        return DotNet(arguments: "sln list", logOutput: false)
            .Any(line => line.Text.EndsWith($"{projectName}.csproj"));
    }
}
