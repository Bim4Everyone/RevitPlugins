using System.Linq;

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Components;

using static Nuke.Common.Tools.Git.GitTasks;

partial class Build {
    Target CreateBranch => _ => _
        .OnlyWhenDynamic(() => IsLocalBuild, "Need local build")
        .OnlyWhenDynamic(() => GitRepository.IsOnMainOrMasterBranch(), "Git branch should be main or master")
        .Executes(() => {
            string userName = Params.UserName;
            GitRepository.SetBranch($"{userName}/{PluginName}");
            Git($"checkout -b {userName}/{PluginName}");
            Git("add .");
            Git($"commit -m \"Создан новый плагин {PluginName}\"");
        });
}
