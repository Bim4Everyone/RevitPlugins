using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

partial class Build {
    Target CreateBundle => _ => _
        .Requires(() => PluginName)
        .Requires(() => IconUrl)
        .Requires(() => BundleName)
        .Requires(() => BundleType)
        .Requires(() => BundleOutput)
        .OnlyWhenDynamic(() =>
            !Params.BundleDirectory.DirectoryExists(), "Bundle directory is exists")
        .OnlyWhenDynamic(() =>
            !Params.IconUrl.AbsolutePath.StartsWith(@"https://icons8.com/icon/"), "Bundle icon must be from icons8 site")
        .Executes(async () => {
            Log.Debug("PluginName: {PluginName}", Params.PluginName);
            Log.Debug("TemplateName: {TemplateName}", Params.TemplateName);
            Log.Debug("TemplateBundleDirectory: {TemplateBundleDirectory}", Params.TemplateBundle);

            Log.Debug("IconUrl: {IconUrl}", Params.IconUrl);
            Log.Debug("BundleName: {BundleName}", Params.BundleName);
            Log.Debug("BundleType: {BundleType}", Params.BundleType);
            Log.Debug("BundleDirectory: {BundleDirectory}", Params.BundleDirectory);

            Params.BundleDirectory.CreateDirectory();
            CopyDirectory(Params.TemplateBundle, Params.BundleDirectory,
                new Dictionary<string, string>() {
                    {"${{ gen.bundle_name }}", Params.BundleName},
                    {"${{ gen.plugin_name }}", Params.PluginName},
                    {"${{ gen.plugin_command }}", Params.PluginName + "Command"},
                    {"${{ gen.author }}", Params.UserName},
                    {"${{ gen.min_revit_version }}", Params.BuildRevitVersions.MinBy(item => (int) item)},
                    {"${{ gen.max_revit_version }}", Params.BuildRevitVersions.MaxBy(item => (int) item)}
                });

            await IconSize.CreateImages(GetImageUri(), Params.BundleDirectory / "icon.png");
        });

    Uri GetImageUri() {
        return new Uri(string.Format(Params.BundleUriIconFormat, Params.BundleIconSize.Size, IconUrl.AbsolutePath.Split('/')[^2]));
    }
}
