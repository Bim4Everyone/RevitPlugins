using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;

partial class Build {
    Target PushPluginDll => _ => _
        .Requires(() => PluginName)
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .OnlyWhenDynamic(() => Params.ExtensionDirectory.DirectoryExists(), "ExtensionDirectory must exists")
        .Executes(() => {
            string branchName = $"nuke/{Params.PluginName}";
           
            Log.Debug("Execute git commands in directory: {@ExtensionDirectory}", Params.ExtensionDirectory);
            Git($"-C \"{Params.ExtensionDirectory}\" switch -c {branchName} -q");
            Git($"-C \"{Params.ExtensionDirectory}\" status");
            
            Log.Debug("Commit updated *.dll");
            Git($"-C \"{Params.ExtensionDirectory}\" add *.dll");
            Git($"-C \"{Params.ExtensionDirectory}\" status");
            Git($"-C \"{Params.ExtensionDirectory}\" commit -m \"Обновление библиотек плагина {Params.PluginName}\"");
           
            Log.Debug("Push changes to origin");
            Git($"-C \"{Params.ExtensionDirectory}\" push -q -u origin {branchName}");
        });
}
