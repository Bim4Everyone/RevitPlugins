using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Models.RevitClashReport {
    internal class PluginClashesLoader : IClashesLoader {
        public PluginClashesLoader(string path) {
            FilePath = path;
        }

        public string FilePath { get; }

        public IEnumerable<ClashModel> GetClashes() {
            try {
                var configLoader = new ConfigLoader();
                return configLoader.Load<ClashesConfig>(FilePath).Clashes;
            } catch(pyRevitLabs.Json.JsonSerializationException) {
                throw new ArgumentException("Неверный файл конфигурации.");
            }

        }

        public bool IsValid() {
            return FilePath.EndsWith(".json");
        }
    }
}