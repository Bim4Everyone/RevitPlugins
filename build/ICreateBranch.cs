using System.Linq;

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Components;

using static Nuke.Common.Tools.Git.GitTasks;

interface ICreateBranch : IHazGitRepository, IHazPluginName {
    Target CreateBranch => _ => _
        .OnlyWhenDynamic(() => IsLocalBuild, "Need local build")
        .OnlyWhenDynamic(() => GitRepository.IsOnDevelopBranch(), "Git branch should be develop")
        .Executes(() => {
            string userName = Git("config --global user.name").First().Text;
            GitRepository.SetBranch($"{userName}/{PluginName}");
            Git($"checkout -b {userName}/{PluginName}");
            Git("add .");
            Git($"commit -m \"Создан новый плагин {PluginName}\"");
        });
}