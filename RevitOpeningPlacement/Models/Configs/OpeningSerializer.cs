using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs {
    internal class OpeningSerializer : IConfigSerializer {
        public T Deserialize<T>(string text) {
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new OpeningSerializationBinder()
            });
        }

        public string Serialize<T>(T @object) {
            return JsonConvert.SerializeObject(@object, Formatting.Indented, new JsonSerializerSettings {
                TypeNameHandling = TypeNameHandling.Objects,
                SerializationBinder = new OpeningSerializationBinder()
            });
        }
    }
}
