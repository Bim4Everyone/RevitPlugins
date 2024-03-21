using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using dosymep.Nuke.RevitVersions;

using Newtonsoft.Json.Linq;

using Nuke.Common;
using Nuke.Common.Git;
using Nuke.Common.IO;

using Octokit;

using Serilog;

using static Nuke.Common.Tools.Git.GitTasks;

partial class Build {
    /// <summary>
    /// Min Revit version.
    /// </summary>
    [Parameter("Min Revit version. Default value is \"Rv2022\".")]
    public RevitVersion MinVersion { get; set; } = RevitVersion.Rv2022;

    /// <summary>
    /// Max Revit version.
    /// </summary>
    [Parameter("Max Revit version. Default value is \"Rv2024\".")]
    public RevitVersion MaxVersion { get; set; } = RevitVersion.Rv2024;

    /// <summary>
    /// Build Revit versions.
    /// </summary>
    [Parameter("Build Revit versions. Default value is Empty.")]
    public RevitVersion[] RevitVersions { get; set; } = new RevitVersion[0];

    /// <summary>
    /// Project (plugin) name in solution.
    /// </summary>
    [Parameter("Project (plugin) name in solution.")]
    public string PluginName { get; set; }

    /// <summary>
    /// Output directory.
    /// </summary>
    [Parameter("Output directory.")]
    public AbsolutePath Output { get; set; } = RootDirectory / "bin";

    /// <summary>
    /// Publish directory.
    /// </summary>
    [Parameter("Publish directory.")]
    public string PublishDirectory { get; set; } = RootDirectory / "bin";

    /// <summary>
    /// Configuration to build. Default is 'Debug' (local) or 'Release' (server).
    /// </summary>
    [Parameter("Configuration to build. Default is 'Debug' (local) or 'Release' (server)")]
    public Configuration Configuration { get; set; } = IsLocalBuild ? Configuration.Debug : Configuration.Release;

    /// <summary>
    /// Extensions GitHub token value.
    /// </summary>
    [Secret]
    [Parameter("Extensions token value.")]
    public string ExtensionsAppToken { get; set; }
    
    /// <summary>
    /// RevitPlugins GitHub token value.
    /// </summary>
    [Secret]
    [Parameter("RevitPlugins token value.")]
    public string RevitPluginsAppToken { get; set; }

    /// <summary>
    /// Bundle icon url.
    /// </summary>
    [Parameter("Bundle icon url.")]
    public Uri IconUrl { get; set; }

    /// <summary>
    /// Bundle name.
    /// </summary>
    [Parameter("Bundle name.")]
    public string BundleName { get; set; }

    /// <summary>
    /// Bundle type.
    /// </summary>
    [Parameter("Bundle type.")]
    public BundleType BundleType { get; set; }

    /// <summary>
    /// Bundle output.
    /// </summary>
    [Parameter("Bundle output.")]
    public string BundleOutput { get; set; }
    
    /// <summary>
    /// When PullRequest has merged equals true.
    /// </summary>
    [Parameter("When PullRequest has merged equals true.")]
    public bool PullRequestMerged { get; set; }
    
    public class BuildParams {
        public BuildParams(Build build) {
            PluginName = build.PluginName;
            Output = build.Output ?? DefaultOutput;
            PublishDirectory = build.PublishDirectory ?? Output;
            Configuration = build.Configuration;

            ExtensionsAppToken = build.ExtensionsAppToken;
            RevitPluginsAppToken = build.RevitPluginsAppToken;

            IconUrl = build.IconUrl;
            BundleName = build.BundleName;
            BundleType = build.BundleType ?? BundleType.InvokeButton;
            BundleOutput = build.BundleOutput ?? Output;

            PullRequestMerged = build.PullRequestMerged;

            BuildRevitVersions = build.RevitVersions.Length > 0
                ? build.RevitVersions
                : RevitVersion.GetRevitVersions(build.MinVersion, build.MaxVersion);

            BranchName = build.GitRepository.Branch;
            BranchCommitSha = build.GitRepository.Commit;
            BranchCommitCount = Git($"rev-list --count \"{BranchCommitSha}\"").First().Text;

            if(build.GitRepository.IsOnMainOrMasterBranch()) {
                BranchTag = "";
            } else if(build.GitRepository.IsOnDevelopBranch()) {
                BranchTag = "-beta";
            } else if(build.GitRepository.IsOnHotfixBranch()) {
                BranchTag = "-fix";
            } else if(build.GitRepository.IsOnReleaseBranch()) {
                BranchTag = "-release";
            } else {
                BranchTag = "-alpha";
            }
        }


        /// <summary>
        /// Project (plugin) name in solution.
        /// </summary>
        public string PluginName { get; }

        /// <summary>
        /// Output directory. Default value is <see cref="DefaultOutput"/>.
        /// </summary>
        public AbsolutePath Output { get; }

        /// <summary>
        /// Publish directory. Default value is <see cref="Output"/>.
        /// </summary>
        public string PublishDirectory { get; }

        /// <summary>
        /// Configuration to build. Default value is 'Debug' (local) or 'Release' (server).
        /// </summary>
        public Configuration Configuration { get; }

        /// <summary>
        /// Extensions GitHub token value.
        /// </summary>
        public string ExtensionsAppToken { get; }
        
        /// <summary>
        /// RevitPlugins GitHub token value.
        /// </summary>
        public string RevitPluginsAppToken { get; }

        /// <summary>
        /// Bundle icon url.
        /// </summary>
        public Uri IconUrl { get; }

        /// <summary>
        /// Bundle name.
        /// </summary>
        public string BundleName { get; }

        /// <summary>
        /// Bundle type. Default value is<see cref="BundleType.InvokeButton"/>.
        /// </summary>
        public BundleType BundleType { get; }

        /// <summary>
        /// Bundle output. Default value is <see cref="Output"/>.
        /// </summary>
        public string BundleOutput { get; }

        
        /// <summary>
        /// When PullRequest has merged equals true.
        /// </summary>
        public bool PullRequestMerged { get; set; }
        
        /// <summary>
        /// Build Revit versions. Default is Rv2022-Rv2024.
        /// </summary>
        public IEnumerable<RevitVersion> BuildRevitVersions { get; }

        /// <summary>
        /// Default output value. Current value is "RootDirectory/bin".
        /// </summary>
        public AbsolutePath DefaultOutput => RootDirectory / "bin";

        /// <summary>
        /// Git username.
        /// </summary>
        public string UserName => Git("config --global user.name").First().Text;

        /// <summary>
        /// Plugin directory.
        /// </summary>
        public AbsolutePath PluginDirectory => RootDirectory / "src" / PluginName;

        /// <summary>
        /// Nuke profile file path.
        /// </summary>
        public AbsolutePath ProfileFile => RootDirectory / ".nuke" / $"parameters.{PluginName}.json";

        /// <summary>
        /// C# project template name.
        /// </summary>
        public string TemplateName => "RevitPluginTemplate";

        /// <summary>
        /// C# project template path.
        /// </summary>
        public AbsolutePath TemplateDirectory => RootDirectory / ".github" / "templates" / TemplateName;

        /// <summary>
        /// Workflow template file path.
        /// </summary>
        public AbsolutePath TemplateWorkflowFile => RootDirectory / ".github" / "templates" / "default.yml";

        /// <summary>
        /// Plugin workflow file path.
        /// </summary>
        public AbsolutePath PluginWorkflowFile => RootDirectory / ".github" / "workflows" / $"publish.{PluginName}.yml";

        /// <summary>
        /// Plugin project file.
        /// </summary>
        public AbsolutePath PluginFile => PluginDirectory / $"{PluginName}.csproj";

        /// <summary>
        /// Template plugin project file.
        /// </summary>
        public AbsolutePath PluginTemplateFile => RootDirectory / "src" / "RevitPlugins" / "RevitPlugins.csproj";

        /// <summary>
        /// Template bundle.
        /// </summary>
        public AbsolutePath TemplateBundle => TemplateDirectory + BundleType.ExtensionWithDot;

        /// <summary>
        /// Bundle directory.
        /// </summary>
        public AbsolutePath BundleDirectory => NukeBuildExtensions.GetExtensionsPath(
            Path.Combine(BundleOutput, BundleName + BundleType.ExtensionWithDot));

        /// <summary>
        /// Bundle icon size. Default value is Size96.
        /// </summary>
        public IconSize BundleIconSize => IconSize.Size96;

        /// <summary>
        /// Bundle url icon format.
        /// </summary>
        public string BundleUriIconFormat => "https://img.icons8.com/?size={0}&id={1}&format=png";

        /// <summary>
        /// Extensions.json url.
        /// </summary>
        public Uri ExtensionsJsonUrl
            => new("https://raw.githubusercontent.com/Bim4Everyone/BIMExtensions/master/extensions.json");

        /// <summary>
        /// Extension name.
        /// </summary>
        public string ExtensionName => PublishDirectory.Split('\\').First();

        /// <summary>
        /// Extension directory.
        /// </summary>
        public AbsolutePath ExtensionDirectory => NukeBuildExtensions.GetExtensionsPath(ExtensionName);

        /// <summary>
        /// extensions.json path.
        /// </summary>
        public AbsolutePath ExtensionsJsonPath => Path.Combine(DefaultOutput, "extensions.json");

        public string NukeBranchName => $"nuke/{PluginName}";
        public string MasterBranchName => "master";
        public string OrganizationName => "Bim4Everyone";
        public string CurrentRepoName => "RevitPlugins";

        public string RepoName => GetCurrentExtensionUrl().Split('/').LastOrDefault();
        
        public string BranchTag { get; }
        public string BranchName { get; }
        public string BranchCommitSha { get; }
        public string BranchCommitCount { get; }

        public IEnumerable<JToken> GetExtensions() {
            string extensionsJsonContent = File.ReadAllText(ExtensionsJsonPath);
            return JObject.Parse(extensionsJsonContent)
                ?.GetValue("extensions")
                ?.ToObject<JToken[]>()
                ?.Where(item => item.IsLib()
                                || ExtensionName.Equals(item.GetExtensionDirName(), StringComparison.OrdinalIgnoreCase));
        }

        public string GetCurrentExtensionUrl() {
            return GetExtensions()
                .FirstOrDefault(item =>
                    ExtensionName.Equals(item.GetExtensionDirName(), StringComparison.OrdinalIgnoreCase))
                .GetExtensionUrl();
        }
    }
}
