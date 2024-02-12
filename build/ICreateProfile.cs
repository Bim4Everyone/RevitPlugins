using System.Collections.Generic;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

interface ICreateProfile : ICreateBundle {
    AbsolutePath ProfileFile => RootDirectory / ".nuke" / $"parameters.{PluginName}.json";

    Target CreateProfile => _ => _
        .OnlyWhenDynamic(() => !ProfileFile.FileExists(), $"Profile file does exists.")
        .Executes(() => {
            Dictionary<string, object> result = new();
            result.Add("$schema", "./build.schema.json");
            result.Add("Solution", "RevitPlugins.sln");

            // Build
            result.Add(nameof(PluginName), PluginName);
            result.Add(nameof(PublishDirectory), PublishDirectory);
            result.Add(nameof(RevitVersions), BuildRevitVersions.Select(item => $"Rv{item}"));
            
            // CreateBundle
            result.Add(nameof(IconUrl), IconUrl.ToString());
            result.Add(nameof(BundleName), BundleName);
            result.Add(nameof(BundleType), BundleType);
            result.Add(nameof(BundleOutput), BundleOutput);

            ProfileFile.WriteJson(result);
        });
}
