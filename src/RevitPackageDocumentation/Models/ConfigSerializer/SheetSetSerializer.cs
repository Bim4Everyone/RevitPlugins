using pyRevitLabs.Json;

namespace RevitPackageDocumentation.Models.ConfigSerializer;

public interface ISheetSetSerializer {
    string Serialize<T>(T obj);
    T Deserialize<T>(string json);
}

public class SheetSetSerializer : ISheetSetSerializer {
    private readonly JsonSerializerSettings _settings;

    public SheetSetSerializer() {
        _settings = new JsonSerializerSettings {
            Formatting = Formatting.Indented,
            Converters = { new SheetComponentConverter() }
        };
    }

    public string Serialize<T>(T obj) {
        return JsonConvert.SerializeObject(obj, _settings);
    }

    public T Deserialize<T>(string json) {
        return JsonConvert.DeserializeObject<T>(json, _settings);
    }
}
