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
interface IPluginCreate : IHazSolution, IHazOutput, IHazPluginName {
    string TemplateName => "RevitPluginTemplate";
    AbsolutePath TemplateFile => RootDirectory / ".github" / "templates" / TemplateName;

    AbsolutePath PluginFile => RootDirectory / PluginName / $"{PluginName}.csproj";
    AbsolutePath PluginTemplateFile => RootDirectory / "RevitPlugins" / "RevitPlugins.csproj";

    Target CreatePlugin => _ => _
        .Triggers<ICreateScript>()
        .OnlyWhenDynamic(() => Solution.GetProject(PluginName) == null, $"Plugin \"{PluginName}\" does exists.")
        .Executes(() => {
            Log.Debug("TemplateName: {TemplateName}", TemplateName);
            Log.Debug("TemplateFile: {TemplateFile}", TemplateFile);

            Log.Debug("PluginFile: {PluginFile}", PluginFile);
            Log.Debug("PluginDirectory: {PluginDirectory}", PluginDirectory);
            Log.Debug("PluginTemplateFile: {PluginTemplateFile}", PluginTemplateFile);

            Log.Debug("HazProject: {HazProject}", Solution.GetProject(PluginName) != null);
            Log.Debug("HazDirectory: {HazDirectory}", PluginDirectory.Exists());

            CopyDirectory(TemplateFile, PluginDirectory);

            DotNet(arguments: $"sln add {PluginFile}");
            PluginFile.WriteAllText(PluginTemplateFile.ReadAllText());
        });

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    void CopyDirectory(AbsolutePath sourceDir, AbsolutePath targetDir, bool recursive = true) {
        // Check if the source directory exists
        if(!sourceDir.Exists())
            throw new DirectoryNotFoundException($"Source directory not found: {sourceDir}");

        // Cache directories before we start copying
        AbsolutePath[] children = sourceDir.GetDirectories().ToArray();

        // Create the destination directory
        targetDir = UpdateName(targetDir).CreateDirectory();

        // Get the files in the source directory and copy to the destination directory
        foreach(AbsolutePath file in sourceDir.GetFiles()) {
            AbsolutePath targetFilePath = UpdateName(targetDir / file.Name);

            string content = file.ReadAllText()
                .Replace(TemplateName, PluginName);

            targetFilePath.WriteAllText(content);
        }

        // If recursive and copying subdirectories, recursively call this method
        if(recursive) {
            foreach(AbsolutePath childDir in children) {
                CopyDirectory(childDir, targetDir / childDir.Name);
            }
        }
    }

    AbsolutePath UpdateName(AbsolutePath target) {
        string targetName = target.Name
            .Replace(TemplateName, PluginName);
        return target.Parent / targetName;
    }
}