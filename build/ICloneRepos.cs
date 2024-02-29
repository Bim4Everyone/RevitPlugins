using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;

interface ICloneRepos : IHazOutput {
    Uri BimExtensionsRepoUrl => new Uri("https://github.com/Bim4Everyone/BIMExtensions.git");

    AbsolutePath BimExtensionsDirectory => NukeBuildExtensions.GetExtensionsPath("01.BIM.extension");

    AbsolutePath ExtensionsJsonPath => Path.Combine(BimExtensionsDirectory, "extensions.json");


    Target CloneRepos => _ => _
        .OnlyWhenStatic(() => !IsServerBuild, "TODO after testing make only on ServerBuild")
        .Requires(() => PublishDirectory)
        .OnlyWhenStatic(() => !BimExtensionsDirectory.DirectoryExists(), "01.BIM.extension directory must not exist")
        .Executes(() => {
            Log.Debug($"Clone BIMExtensions repo: {BimExtensionsRepoUrl} to directory: {BimExtensionsDirectory}");
            Git($"clone {BimExtensionsRepoUrl} {BimExtensionsDirectory} -q");
            Assert.FileExists(ExtensionsJsonPath, $"extensions.json must exist in {BimExtensionsDirectory}");

            string extensionsJson = File.ReadAllText(ExtensionsJsonPath);
            Log.Debug($"extensions.json content: {extensionsJson}");

            string libRepoUrl = GetLibRepoUrl(extensionsJson);
            string libDirPath = NukeBuildExtensions.GetExtensionsPath(GetLibRepoDirectory(extensionsJson));
            Assert.False(Directory.Exists(libDirPath), "BIM4Everyone.lib directory must not exist");
            Log.Debug($"Clone BIM4Everyone repo: {libRepoUrl} to directory: {libDirPath}");
            Git($"clone {libRepoUrl} {libDirPath} -q");

            string pluginRepoUrl = GetPluginExtensionRepoUrl(extensionsJson);
            string pluginDirPath = NukeBuildExtensions.GetExtensionsPath(GetPluginExtensionDirectory(PublishDirectory));
            //we've already cloned BIMExtension
            if(!Directory.Exists(pluginDirPath)) {
                Log.Debug($"Clone plugin extension repo: {pluginRepoUrl} to directory: {pluginDirPath}");
                Git($"clone {pluginRepoUrl} {pluginDirPath} -q");
            }
        });


    /// <summary>
    /// Returns specified plugin's extension repo url. For example url of BIMExtensions repo
    /// </summary>
    /// <param name="extensionsFileContent">extensions.json from root of BIMExtensions repo</param>
    /// <returns></returns>
    private string GetPluginExtensionRepoUrl(string extensionsFileContent) {
        return GetPluginExtension(extensionsFileContent)
            .Value<string>("url");
    }

    private string GetPluginExtensionDirectory(string publishDirectory) {
        return publishDirectory.Split('\\').First();
    }

    private JToken GetPluginExtension(string extensionsFileContent) {
        string pluginExtensionDir = GetPluginExtensionDirectory(PublishDirectory);
        return GetExtension(extensionsFileContent,
            ext => (ext.Value<string>("name") + '.' + ext.Value<string>("type")) == pluginExtensionDir);
    }

    /// <summary>
    /// Returns Bim4Everyone.lib repo url
    /// </summary>
    /// <param name="extensionsFileContent">extensions.json from root of BIMExtensions repo</param>
    /// <returns></returns>
    private string GetLibRepoUrl(string extensionsFileContent) {
        return GetLibExtension(extensionsFileContent)
            .Value<string>("url");
    }

    private string GetLibRepoDirectory(string extensionsFileContent) {
        JToken libJson = GetLibExtension(extensionsFileContent);
        return libJson.Value<string>("name") + '.' + libJson.Value<string>("type");
    }

    private JToken GetLibExtension(string extensionsFileContent) {
        return GetExtension(extensionsFileContent, extension => extension.Value<string>("type") == "lib");
    }

    private JToken GetExtension(string extensionsFileContent, Predicate<JToken> filter) {
        return JObject.Parse(extensionsFileContent)
            .GetValue("extensions")
            .ToObject<JToken[]>()
            .First(filter.Invoke);
    }
}
