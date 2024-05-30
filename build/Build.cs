using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using dosymep.Nuke.RevitVersions;

using Nuke.Common;
using Nuke.Common.CI.GitHubActions;
using Nuke.Common.Git;
using Nuke.Common.IO;
using Nuke.Common.ProjectModel;
using Nuke.Common.Tools.GitVersion;
using Nuke.Components;

using Serilog;

partial class Build : NukeBuild {
    /// Support plugins are available for:
    ///   - JetBrains ReSharper        https://nuke.build/resharper
    ///   - JetBrains Rider            https://nuke.build/rider
    ///   - Microsoft VisualStudio     https://nuke.build/visualstudio
    ///   - Microsoft VSCode           https://nuke.build/vscode
    public static int Main() => Execute<Build>(b => b.Compile);

    public BuildParams Params { get; set; }

    [Solution] public readonly Solution Solution;
    [GitRepository] public readonly GitRepository GitRepository;

    protected override void OnBuildInitialized() {
        Params = new BuildParams(this);
        base.OnBuildInitialized();

        Log.Information("Build plugin: {PluginName}", Params.PluginName);
        Log.Information("Plugin directory: {PluginDirectory}", Params.PluginDirectory);
        Log.Information("Build revit versions: {BuildRevitVersions}", Params.BuildRevitVersions);

        Log.Information("Output: {Output}", Params.Output);

        Log.Information("IsLocalBuild: {IsLocalBuild}", IsLocalBuild);
        Log.Information("IsServerBuild: {IsServerBuild}", IsServerBuild);

        Log.Information("Repository Url: {RepoUrl}", GitRepository.HttpsUrl);
        Log.Information("Repository Branch: {RepoBranch}", GitRepository.Branch);
    }

    // https://learn.microsoft.com/en-us/dotnet/standard/io/how-to-copy-directories
    void CopyDirectory(AbsolutePath sourceDir,
        AbsolutePath targetDir,
        Dictionary<string, string> replaceMap = default,
        bool recursive = true) {
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

            string content = file.ReadAllText();
            if(!file.HasExtension(".png")) {
                content = content.Replace(Params.TemplateName, Params.PluginName);
                if(replaceMap != null) {
                    foreach((string key, string value) in replaceMap) {
                        content = content.Replace(key, value);
                    }
                }
            }

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
            .Replace(Params.TemplateName, Params.PluginName);
        return target.Parent / targetName;
    }
}
