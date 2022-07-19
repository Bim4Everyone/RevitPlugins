using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;

using pyRevitLabs.Json;

namespace RevitOpeningPlacement.Models.Configs {
    internal class OpeningConfig : ProjectConfig {
        public string RevitVersion { get; set; }
        [JsonIgnore]
        public override string ProjectConfigPath { get; set; }
        [JsonIgnore]
        public override IConfigSerializer Serializer { get; set; }
        public MepCategoryCollection Categories { get; set; }

        public static OpeningConfig GetOpeningConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new OpeningSerializer())
                .SetPluginName(nameof(RevitOpeningPlacement))
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(nameof(OpeningConfig) + ".json")
                .Build<OpeningConfig>();
        }
    }

    internal class MepCategoryCollection : IEnumerable<MepCategory> {
        public MepCategoryCollection(IEnumerable<MepCategory> categories) {
            Categories = new List<MepCategory>(categories);
        }
        public List<MepCategory> Categories { get; set; } = new List<MepCategory>();
        public int Count => Categories.Count;
        public MepCategory this[BuiltInCategory category] => Categories.FirstOrDefault(item => item.Name.Equals(RevitRepository.CategoryNames[category]));
        public IEnumerator<MepCategory> GetEnumerator() => Categories.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal class MepCategory {
        public string Name { get; set; }
        public string ImageSource { get; set; }
        public SizeCollection MinSizes { get; set; }
        public List<Offset> Offsets { get; set; }
    }

    internal class SizeCollection : IEnumerable<Size> {
        public SizeCollection(IEnumerable<Size> sizes) {
            Sizes = new List<Size>(sizes);
        }

        List<Size> Sizes { get; set; }
        public Size this[Parameters index] => Sizes.FirstOrDefault(item => item.Name.Equals(RevitRepository.ParameterNames[index]));

        public IEnumerator<Size> GetEnumerator() => Sizes.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();
    }

    internal class Size {
        public string Name { get; set; }
        public double Value { get; set; }
    }

    internal class Offset {
        public double From { get; set; }
        public double To { get; set; }
        public double OffsetValue { get; set; }
    }
}
