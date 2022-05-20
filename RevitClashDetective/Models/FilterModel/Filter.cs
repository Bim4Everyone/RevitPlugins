using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.FilterGenerators;
using pyRevitLabs.Json;
using System.IO;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone;

namespace RevitClashDetective.Models.FilterModel {

    internal class FiltersConfig : ProjectConfig {
        public List<Filter> Filters { get; set; }
        public override string ProjectConfigPath { get; set; }
        public override IConfigSerializer Serializer { get; set; }
        public static FiltersConfig GetFiltersConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new RevitClashConfigSerializer())
                .SetPluginName(nameof(RevitClashDetective))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(FiltersConfig) + ".json")
                .Build<FiltersConfig>();
        }
    }
    internal class Filter {
        public Filter(RevitRepository revitRepository) {
            RevitRepository = revitRepository;
        }

        public string Name { get; set; }
        public Set Set { get; set; }

        [JsonIgnore]
        public RevitRepository RevitRepository { get; set; }
        public List<int> CategoryIds { get; set; }

        public void CreateRevitFilter() {
            var generator = new RevitFilterGenerator();
            Set.FilterGenerator = generator;
            Set.Generate();
            var elementFilter = generator.Generate();
            var ids = CategoryIds.Select(item => new ElementId(item));
            RevitRepository.CreateFilter(ids, elementFilter, Name);
        }

        //public void Save() {
        //    var jsonString = JsonConvert.SerializeObject(this, Formatting.Indented, new JsonSerializerSettings { 
        //        TypeNameHandling = TypeNameHandling.Objects,
        //        SerializationBinder = new RevitClashesSerializationBinder()
        //    });

        //    File.WriteAllText(@"C:\WorkLera\test.json", jsonString);
        //}

        //public Filter Read() {
        //    var jsonString = File.ReadAllText(@"C:\WorkLera\test.json");
        //    return JsonConvert.DeserializeObject<Filter>(jsonString, new JsonSerializerSettings {
        //        TypeNameHandling = TypeNameHandling.Objects,
        //        SerializationBinder = new RevitClashesSerializationBinder()
        //    });
        //}
    }

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
