using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Components;

using Serilog;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

partial class Build {
    Target CreatePlugin => _ => _
        .Triggers(CreateBranch)
        .DependsOn(CreateBundle, CreateWorkflow, CreateProfile)
        .OnlyWhenDynamic(() => Solution.GetProject(PluginName) == null, $"Plugin \"{PluginName}\" does exists.")
        .OnlyWhenDynamic(() => !Params.PluginDirectory.DirectoryExists(), $"Plugin directory \"{PluginName}\" does exists.")
        .Executes(() => {
            Log.Debug("TemplateName: {TemplateName}", Params.TemplateName);
            Log.Debug("TemplateDirectory: {TemplateDirectory}", Params.TemplateDirectory);

            Log.Debug("PluginFile: {PluginFile}", Params.PluginFile);
            Log.Debug("PluginDirectory: {PluginDirectory}", Params.PluginDirectory);
            Log.Debug("PluginTemplateFile: {PluginTemplateFile}", Params.PluginTemplateFile);

            Log.Debug("HazProject: {HazProject}", Solution.GetProject(PluginName) != null);
            Log.Debug("HazDirectory: {HazDirectory}", Params.PluginDirectory.Exists());

            CopyDirectory(Params.TemplateDirectory, Params.PluginDirectory,
                new Dictionary<string, string>() {{"${{ gen.bundle_name }}", BundleName ?? "Название плагина"}});

            DotNet(arguments: $"sln add {Params.PluginFile} --in-root");
            Params.PluginFile.WriteAllText(Params.PluginTemplateFile.ReadAllText());
        });
}
