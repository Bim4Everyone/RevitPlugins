using System.Linq;

using Nuke.Common;

using static Nuke.Common.Tools.Git.GitTasks;

interface IHazGitUserName : INukeBuild {
    string UserName => Git("config --global user.name").First().Text;
}