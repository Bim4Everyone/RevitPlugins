using System.Collections.Generic;
using System.IO;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class JsonExporter<T> {
        public static void Export(string path, IEnumerable<T> elements) {
            path += ".json";
            using(StreamWriter file = File.CreateText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, elements);
            }
        }
    }
}
