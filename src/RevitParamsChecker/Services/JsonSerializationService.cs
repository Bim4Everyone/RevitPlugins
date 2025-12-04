using System;

using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitParamsChecker.Services;

internal class JsonSerializationService : IConfigSerializer {
    private readonly JsonSerializerSettings _settings;

    public JsonSerializationService() {
        _settings = new JsonSerializerSettings() { TypeNameHandling = TypeNameHandling.Objects };
    }

    public string Serialize<T>(T @object) {
        if(@object == null) {
            throw new ArgumentNullException(nameof(@object));
        }

        return JsonConvert.SerializeObject(@object, _settings);
    }

    public T Deserialize<T>(string text) {
        if(string.IsNullOrEmpty(text)) {
            throw new ArgumentException("Value cannot be null or empty.", nameof(text));
        }

        return JsonConvert.DeserializeObject<T>(text, _settings);
    }
}
