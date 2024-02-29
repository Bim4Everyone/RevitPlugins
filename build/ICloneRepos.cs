using System;
using System.IO;
using System.Linq;

using Newtonsoft.Json.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;
using static Nuke.Common.Tools.PowerShell.PowerShellTasks;

interface ICloneRepos : IHazOutput {
    Uri ExtensionsJsonUrl
        => new Uri("https://raw.githubusercontent.com/Bim4Everyone/BIMExtensions/master/extensions.json");

    AbsolutePath ExtensionsJsonPath => Path.Combine(RootDirectory, "bin", "extensions.json");

    AbsolutePath ExtensionsDirectory
        => Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "pyRevit");

    Target CloneRepos => _ => _
        .Requires(() => PublishDirectory)
        .OnlyWhenDynamic(() => !ExtensionsDirectory.DirectoryExists(), $"{ExtensionsDirectory} must not exist")
        .Executes(() => {
            Log.Debug($"Download extensions.json from: {ExtensionsJsonUrl} to {ExtensionsJsonPath}");
            PowerShell($"curl.exe -L \"{ExtensionsJsonUrl}\" -o \"{ExtensionsJsonPath}\" --create-dirs -s");
            Assert.FileExists(ExtensionsJsonPath, $"{ExtensionsJsonPath} must exist");
            string extensionsJson = File.ReadAllText(ExtensionsJsonPath);

            string libRepoUrl = GetLibRepoUrl(extensionsJson);
            string libDirPath = NukeBuildExtensions.GetExtensionsPath(GetLibRepoDirectory(extensionsJson));
            Assert.False(Directory.Exists(libDirPath), $"{libDirPath} must not exist");
            Log.Debug($"Clone BIM4Everyone repo: {libRepoUrl} to directory: {libDirPath}");
            Git($"clone \"{libRepoUrl}\" \"{libDirPath}\" -q");

            string pluginRepoUrl = GetPluginExtensionRepoUrl(extensionsJson);
            string pluginDirPath = NukeBuildExtensions.GetExtensionsPath(GetPluginExtensionDirectory(PublishDirectory));
            Assert.False(Directory.Exists(pluginDirPath), $"{pluginDirPath} must not exist");
            Log.Debug($"Clone plugin extension repo: {pluginRepoUrl} to directory: {pluginDirPath}");
            Git($"clone \"{pluginRepoUrl}\" \"{pluginDirPath}\" -q");
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
