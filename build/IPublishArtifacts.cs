using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;

interface IPublishArtifacts : IHazOutput, IHazPluginName {
    string ExtensionName => PublishDirectory.Split('\\').First();

    AbsolutePath ExtensionDirectory => NukeBuildExtensions.GetExtensionsPath(ExtensionName);

    Target PushPluginDll => _ => _
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .OnlyWhenDynamic(() => ExtensionDirectory.DirectoryExists(), $"{ExtensionDirectory} must exist")
        .Executes(() => {
            string branchName = $"nuke/{PluginName}";
           
            Log.Debug("Execute git commands in directory: {@ExtensionDirectory}", ExtensionDirectory);
            Git($"-C \"{ExtensionDirectory}\" switch -c {branchName} -q");
            Git($"-C \"{ExtensionDirectory}\" status");
            
            Log.Debug("Commit updated *.dll");
            Git($"-C \"{ExtensionDirectory}\" add *.dll");
            Git($"-C \"{ExtensionDirectory}\" status");
            Git($"-C \"{ExtensionDirectory}\" commit -m \"Обновление библиотек плагина {PluginName}\"");
           
            Log.Debug("Push changes to origin");
            Git($"-C \"{ExtensionDirectory}\" push -q -u origin {branchName}");
        });
}
