using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;

using static Nuke.Common.Tools.DotNet.DotNetTasks;

interface IProjectCreate : ICommonParams, IPluginParams {
    string PluginTemplateName => "RevitPluginTemplate";
    AbsolutePath PluginTemplatePath => RootDirectory / ".github" / "templates" / PluginTemplateName;

    AbsolutePath ScriptTemplatePath => RootDirectory / ".github" / "templates" / "default.yml";
    AbsolutePath PluginScriptPath => RootDirectory / ".github" / "workflows" / $"{PluginName}.yml";

    AbsolutePath ProjectPath => RootDirectory / PluginName / $"{PluginName}.csproj";
    AbsolutePath TemplatePath => RootDirectory / "RevitPlugins" / "RevitPlugins.csproj";

    Target CreateScript => _ => _
        .Requires(() => Output)
        .Requires(() => PluginName)
        .Executes(() => {
            string content = ScriptTemplatePath.ReadAllText()
                .Replace("${{ gen.output }}", GetOutput(Output))
                .Replace("${{ gen.plugin_name }}", PluginName);
            PluginScriptPath.WriteAllText(content);
        });

    Target CreatePlugin => _ => _
        .DependsOn(CreateScript)
        .Requires(() => Output)
        .Requires(() => PluginName)
        .OnlyWhenDynamic(() => Solution.GetProject(PluginName) == null, $"Плагин \"{PluginName}\" уже существует.")
        .Executes(() => {
            CopyDirectory(PluginTemplatePath, RootDirectory / PluginName);
            
            DotNet(arguments: $"sln add {ProjectPath}");
            ProjectPath.WriteAllText(TemplatePath.ReadAllText());
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
                .Replace(PluginTemplateName, PluginName);

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
            .Replace(PluginTemplateName, PluginName);
        return target.Parent / targetName;
    }
}