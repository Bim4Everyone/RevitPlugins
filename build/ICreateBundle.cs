using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Nuke.Common;
using Nuke.Common.IO;
using Nuke.Common.Tooling;

using Serilog;

interface ICreateBundle : IClean, IHazTemplate {
    [Parameter]
    Uri IconUrl => TryGetValue(() => IconUrl)
                   ?? new Uri("https://icons8.com/icon/UgAl9mP8tniQ/example");

    [Parameter] string BundleName => TryGetValue(() => BundleName) ?? "BundleName";
    [Parameter] BundleType BundleType => TryGetValue(() => BundleType) ?? BundleType.InvokeButton;

    AbsolutePath TemplateBundleDirectory => TemplateDirectory + BundleType.ExtensionWithDot;
    AbsolutePath BundleDirectory => Output / BundleName + BundleType.ExtensionWithDot;


    IconSize IconSize => IconSize.Size96;
    string UriIconFormat => "https://img.icons8.com/?size={0}&id={1}&format=png";


    Target CreateBundle => _ => _
        //.Triggers(Clean)
        .Requires(() => BundleName)
        .Requires(() => BundleType)
        .Executes(async () => {
            Log.Debug("TemplateName: {TemplateName}", TemplateName);
            Log.Debug("TemplateDirectory: {TemplateDirectory}", TemplateDirectory);

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

class BundleType : Enumeration {
    public static readonly BundleType PushButton = new() {Value = nameof(PushButton)};
    public static readonly BundleType InvokeButton = new() {Value = nameof(InvokeButton)};

    public string ExtensionWithDot => "." + Value.ToLower();
}

class IconSize : Enumeration {
    public static readonly IconSize Size32 = new() {Value = nameof(Size32), Size = 32};
    public static readonly IconSize Size64 = new() {Value = nameof(Size64), Size = 64};
    public static readonly IconSize Size96 = new() {Value = nameof(Size96), Size = 96};

    public int Size { get; protected set; }

    public static async Task CreateImages(Uri uri, AbsolutePath target) {
        using(MemoryStream stream = new(await GetUrlContent(uri))) {
            Bitmap image = (Bitmap) Image.FromStream(stream);
            image.Save(target);

            CreateBitmap(
                image,
                Color.FromArgb(235, 235, 235),
                target.Parent / target.NameWithoutExtension + ".dark.png");

            CreateBitmap(
                image,
                Color.FromArgb(250, 176, 5),
                target.Parent / target.NameWithoutExtension + ".fail.png");

            CreateBitmap(
                image,
                Color.FromArgb(250, 82, 82),
                target.Parent / target.NameWithoutExtension + ".warning.png");
        }
    }

    static void CreateBitmap(Bitmap source, Color color, AbsolutePath target) {
        Bitmap output = new(source);
        for(int x = 0; x < source.Width; x++) {
            for(int y = 0; y < source.Height; y++) {
                Color pixel = source.GetPixel(x, y);
                output.SetPixel(x, y,
                    pixel.A == 0
                        ? pixel
                        : Color.FromArgb(pixel.A, color.R, color.G, color.B));
            }
        }

        output.Save(target);
    }

    public static async Task<byte[]> GetUrlContent(Uri uri) {
        using(var client = new HttpClient())
        using(var result = await client.GetAsync(uri))
            return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
    }
}