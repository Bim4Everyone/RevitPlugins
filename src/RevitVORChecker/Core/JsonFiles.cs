using System.IO;
using System.Text;

using pyRevitLabs.Json;

namespace RevitVORChecker.Core;

/// <summary>
/// Чтение и запись файлов JSON рабочей папки проверки (UTF-8 без BOM).
/// </summary>
internal static class JsonFiles {
    private static readonly JsonSerializerSettings _settings = new() {
        Formatting = Formatting.None,
        FloatParseHandling = FloatParseHandling.Double
    };

    public static T Load<T>(string path) {
        return JsonConvert.DeserializeObject<T>(File.ReadAllText(path, Encoding.UTF8), _settings);
    }

    public static void Save(string path, object value, bool indented = false) {
        string directory = Path.GetDirectoryName(path);
        if(!string.IsNullOrEmpty(directory)) {
            Directory.CreateDirectory(directory);
        }

        var settings = new JsonSerializerSettings {
            Formatting = indented ? Formatting.Indented : Formatting.None
        };
        File.WriteAllText(path, JsonConvert.SerializeObject(value, settings), new UTF8Encoding(false));
    }
}
