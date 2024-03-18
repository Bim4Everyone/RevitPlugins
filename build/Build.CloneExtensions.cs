using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.PowerShell.PowerShellTasks;

partial class Build {
    Target CloneRepos => _ => _
        .After(Clean)
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .Executes(() => {
            Log.Debug("ExtensionName: {@ExtensionName}", Params.ExtensionName);
            Log.Debug("ExtensionsJsonUrl: {@ExtensionsJsonUrl}", Params.ExtensionsJsonUrl);
            Log.Debug("ExtensionsJsonPath: {@ExtensionsJsonUrl}", Params.ExtensionsJsonPath.ToString());

            Log.Debug("Download extensions.json");
            PowerShell(
                $"curl.exe -L \"{Params.ExtensionsJsonUrl}\" -o \"{Params.ExtensionsJsonPath}\" --create-dirs -s");

            Log.Debug("Clone repositories");
            foreach(JToken token in Params.GetExtensions()) {
                Log.Debug("Clone repository: {@RepoName}", token.GetExtensionName());

                string repoUrl = token.GetExtensionUrl();
                AbsolutePath dirPath = NukeBuildExtensions.GetExtensionsPath(token.GetExtensionDirName());

                if(dirPath.DirectoryExists()) {
                    Log.Debug("Skipped clone: {@RepoUrl}", repoUrl);
                    continue;
                }

                Log.Debug("RepoUrl: {@RepoUrl}", repoUrl);
                Log.Debug("DirPath: {@DirPath}", dirPath.ToString());

                if(!string.IsNullOrEmpty(Params.RevitPluginsAppToken)) {
                    // https://token@github.com/Bim4Everyone/Bim4Everyone
                    repoUrl = new Uri(new UriBuilder(repoUrl) {UserName = Params.RevitPluginsAppToken}.ToString()).ToString();
                }

                ProcessTasks.StartProcess(GitPath, $"clone \"{repoUrl}\" \"{dirPath}\" -q",
                    outputFilter: m => m.Replace(Params.RevitPluginsAppToken, "****")).WaitForExit();
            }
        });
}
