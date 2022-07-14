using pyRevitLabs.Json;
using dosymep.Bim4Everyone.ProjectConfigs;

namespace RevitClashDetective.Models.FilterModel {
    internal class RevitClashConfigSerializer : IConfigSerializer {
        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new RevitClashesSerializationBinder()
            });
        }

        public string Serialize<T>(T @object) {
            return JsonConvert.SerializeObject(@object, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new RevitClashesSerializationBinder()
            });
        }
    }
}
