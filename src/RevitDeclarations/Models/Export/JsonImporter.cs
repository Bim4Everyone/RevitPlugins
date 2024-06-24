using System.Collections.Generic;
using System.IO;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class JsonImporter<T> {
        public static List<T> Import(string path) {
            List<T> elements = new List<T>();

            using(StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                elements = (List<T>)serializer.Deserialize(file, typeof(List<T>));
            }

            return elements;
        }
    }
}
