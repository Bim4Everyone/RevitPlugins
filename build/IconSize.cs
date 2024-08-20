using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

using Nuke.Common.IO;
using Nuke.Common.Tooling;

[TypeConverter(typeof(TypeConverter<IconSize>))]
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
                Color.FromArgb(250, 82, 82),
                target.Parent / target.NameWithoutExtension + ".fail.png");

            CreateBitmap(
                image,
                Color.FromArgb(250, 176, 5),
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

    static async Task<byte[]> GetUrlContent(Uri uri) {
        using(var client = new HttpClient())
        using(var result = await client.GetAsync(uri))
            return result.IsSuccessStatusCode ? await result.Content.ReadAsByteArrayAsync() : null;
    }
}
