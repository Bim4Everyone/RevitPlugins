using System.Collections.Generic;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Utilities;

partial class Build {
    Target CreateProfile => _ => _
        .OnlyWhenDynamic(() => !Params.ProfileFile.FileExists(), $"Profile file does exists.")
        .Executes(() => {
            Dictionary<string, object> result = new();
            result.Add("$schema", "./build.schema.json");
            result.Add("Solution", "RevitPlugins.sln");

            // Build
            result.Add(nameof(PluginName), PluginName);
            result.Add(nameof(PublishDirectory), PublishDirectory);
            result.Add(nameof(RevitVersions), Params.BuildRevitVersions.Select(item => $"Rv{item}"));
            
            // CreateBundle
            result.Add(nameof(IconUrl), IconUrl.ToString());
            result.Add(nameof(BundleName), BundleName);
            result.Add(nameof(BundleType), BundleType);
            result.Add(nameof(BundleOutput), BundleOutput);

            Params.ProfileFile.WriteJson(result);
        });
}
