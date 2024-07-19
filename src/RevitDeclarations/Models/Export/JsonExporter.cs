using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitDeclarations.Models {
    internal class JsonExporter<T> {
        private readonly ISerializationService _serializationService = 
            ServicesProvider.GetPlatformService<ISerializationService>();

        public void Export(string path, IEnumerable<T> elements) {
            string text = _serializationService.Serialize(elements);
            path = Path.ChangeExtension(path, "json");

            using(StreamWriter file = File.CreateText(path)) {
                file.Write(text);
            }
        }
    }
}
