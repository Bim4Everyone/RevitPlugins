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

[ParameterPrefix(nameof(IPluginCreate))]
interface IPluginCreate : ICreateScript, ICreateProfile, ICreateBranch {
    AbsolutePath PluginFile => RootDirectory / PluginName / $"{PluginName}.csproj";
    AbsolutePath PluginTemplateFile => RootDirectory / "RevitPlugins" / "RevitPlugins.csproj";

    Target CreatePlugin => _ => _
        .Triggers(CreateBranch)
        .DependsOn(CreateBundle, CreateScript, CreateProfile)
        .OnlyWhenDynamic(() => Solution.GetProject(PluginName) == null, $"Plugin \"{PluginName}\" does exists.")
        .OnlyWhenDynamic(() => !PluginDirectory.DirectoryExists(), $"Plugin directory \"{PluginName}\" does exists.")
        .Executes(() => {
            Log.Debug("TemplateName: {TemplateName}", TemplateName);
            Log.Debug("TemplateDirectory: {TemplateDirectory}", TemplateDirectory);

            Log.Debug("PluginFile: {PluginFile}", PluginFile);
            Log.Debug("PluginDirectory: {PluginDirectory}", PluginDirectory);
            Log.Debug("PluginTemplateFile: {PluginTemplateFile}", PluginTemplateFile);

            Log.Debug("HazProject: {HazProject}", Solution.GetProject(PluginName) != null);
            Log.Debug("HazDirectory: {HazDirectory}", PluginDirectory.Exists());
            
            CopyDirectory(TemplateDirectory, PluginDirectory);

            DotNet(arguments: $"sln add {PluginFile}");
            PluginFile.WriteAllText(PluginTemplateFile.ReadAllText());
        });
}