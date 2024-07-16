using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitDeclarations.Models {
    internal class JsonExporter<T> {
        public void Export(string path, IEnumerable<T> elements) {
            ISerializationService service = ServicesProvider.GetPlatformService<ISerializationService>();
            string text = service.Serialize(elements);
            path += ".json";

            using(StreamWriter file = File.CreateText(path)) {
                file.Write(text);
            }
        }
    }
}
