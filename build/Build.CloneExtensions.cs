using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.PowerShell.PowerShellTasks;

partial class Build {
    Target CloneRepos => _ => _
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => IsServerBuild, "Target should be run only on server")
        .Executes(() => {
            Log.Debug("ExtensionName: {@ExtensionName}", Params.ExtensionName);
            Log.Debug("ExtensionsJsonUrl: {@ExtensionsJsonUrl}", Params.ExtensionsJsonUrl);
            Log.Debug("ExtensionsJsonPath: {@ExtensionsJsonUrl}", Params.ExtensionsJsonPath);

            Log.Debug("Download extensions.json");
            PowerShell($"curl.exe -L \"{Params.ExtensionsJsonUrl}\" -o \"{Params.ExtensionsJsonPath}\" --create-dirs -s");

            Log.Debug("Clone repositories");
            foreach(JToken token in GetExtensions()) {
                Log.Debug("Clone repository: {@RepoName}", token.GetExtensionName());

                string repoUrl = token.GetExtensionUrl();
                AbsolutePath dirPath = NukeBuildExtensions.GetExtensionsPath(token.GetExtensionDirName());
                
                if(dirPath.Existing("*file") is not null) {
                    Log.Debug("Skipped clone: {@RepoUrl}", repoUrl);
                    continue;
                }

                Log.Debug("RepoUrl: {@RepoUrl}", repoUrl);
                Log.Debug("DirPath: {@DirPath}", dirPath);

                if(!string.IsNullOrEmpty(GitHubAppToken)) {
                    // https://token@github.com/Bim4Everyone/Bim4Everyone
                    repoUrl = new Uri(new UriBuilder(repoUrl) {UserName = GitHubAppToken}.ToString()).ToString();
                }

                Git($"clone \"{repoUrl}\" \"{dirPath}\" -q");
            }
        });

    private IEnumerable<JToken> GetExtensions() {
        string extensionsJsonContent = File.ReadAllText(Params.ExtensionsJsonPath);
        return JObject.Parse(extensionsJsonContent)
            ?.GetValue("extensions")
            ?.ToObject<JToken[]>()
            ?.Where(item => item.IsLib()
                            || Params.ExtensionName.Equals(item.GetExtensionName(), StringComparison.OrdinalIgnoreCase));
    }
}
