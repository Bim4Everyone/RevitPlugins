using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

using Nuke.Common;
using Nuke.Common.IO;

using Serilog;

interface ICreateBundle : IHazOutput, IHazTemplate {
    [Parameter] Uri IconUrl => TryGetValue(() => IconUrl);
    [Parameter] string BundleName => TryGetValue(() => BundleName);
    [Parameter] BundleType BundleType => TryGetValue(() => BundleType);

    AbsolutePath TemplateBundleDirectory => TemplateDirectory + BundleType.ExtensionWithDot;
    AbsolutePath BundleDirectory => Output / BundleName + BundleType.ExtensionWithDot;
    
    IconSize IconSize => IconSize.Size96;
    string UriIconFormat => "https://img.icons8.com/?size={0}&id={1}&format=png";


    Target CreateBundle => _ => _
        .Requires(() => BundleName)
        .Requires(() => BundleType)
        .Executes(async () => {
            Log.Debug("TemplateName: {TemplateName}", TemplateName);
            Log.Debug("TemplateBundleDirectory: {TemplateBundleDirectory}", TemplateBundleDirectory);

            Log.Debug("IconUrl: {IconUrl}", IconUrl);
            Log.Debug("BundleName: {BundleName}", BundleName);
            Log.Debug("BundleType: {BundleType}", BundleType);
            Log.Debug("BundleDirectory: {BundleDirectory}", BundleDirectory);

            CopyDirectory(TemplateBundleDirectory, BundleDirectory,
                new Dictionary<string, string>() {
                    {"${{ gen.bundle_name }}", BundleName},
                    {"${{ gen.plugin_name }}", this.From<IHazPluginName>().PluginName},
                    {"${{ gen.plugin_command }}", this.From<IHazPluginName>().PluginName + "Command"},
                });

            BundleDirectory.CreateDirectory();
            await IconSize.CreateImages(GetImageUri(), BundleDirectory / "image.png");
        });

    Uri GetImageUri() {
        return new Uri(string.Format(UriIconFormat, IconSize.Size, IconUrl.AbsolutePath.Split('/')[^2]));
    }
}