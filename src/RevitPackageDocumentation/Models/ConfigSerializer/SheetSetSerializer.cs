using System.Collections.Generic;

using pyRevitLabs.Json;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

public interface ISheetSetSerializer {
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
}

public class SheetSetSerializer : ISheetSetSerializer {
    private readonly JsonSerializerSettings _settings;

    public SheetSetSerializer(IEnumerable<JsonConverter> converters) {
        _settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented,
        };
        foreach(var converter in converters) {
            _settings.Converters.Add(converter);
        }
    }

    public string Serialize<T>(T obj) {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    public T Deserialize<T>(string json) {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }
}
