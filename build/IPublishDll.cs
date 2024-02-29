using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;
interface IPublishDll : IHazOutput, IHazPluginName {
    AbsolutePath PluginExtensionDirectory
        => NukeBuildExtensions.GetExtensionsPath(GetPluginExtensionDirectory(PublishDirectory));

    Target PushPluginDll => _ => _
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .Requires(() => PublishDirectory)
        .OnlyWhenDynamic(() => PluginExtensionDirectory.DirectoryExists(), $"{PluginExtensionDirectory} must exist")
        .Requires(() => PluginName)
        .Executes(() => {
            string branchName = $"nuke/{PluginName}";
            Log.Debug($"Execute git commands in directory: {PluginExtensionDirectory}");
            Git($"-C \"{PluginExtensionDirectory}\" config --local user.email \"nuke@gmail.com\""); //just for fun
            Git($"-C \"{PluginExtensionDirectory}\" config --local user.name \"nuke\"");
            Git($"-C \"{PluginExtensionDirectory}\" switch -c {branchName} -q");
            Git($"-C \"{PluginExtensionDirectory}\" status");
            Log.Debug("Commit updated *.dll");
            Git($"-C \"{PluginExtensionDirectory}\" add *.dll");
            Git($"-C \"{PluginExtensionDirectory}\" status");
            Git($"-C \"{PluginExtensionDirectory}\" commit -m \"Обновление библиотек плагина {PluginName}\"");
            Log.Debug("Push changes to origin");
            Git($"-C \"{PluginExtensionDirectory}\" push -q -u origin {branchName}");
        });

    private string GetPluginExtensionDirectory(string publishDirectory) {
        return publishDirectory.Split('\\').First();
    }
}
