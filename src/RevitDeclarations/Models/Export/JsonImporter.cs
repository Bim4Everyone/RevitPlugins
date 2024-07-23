using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitDeclarations.Models {
    internal class JsonImporter<T> {
        private readonly ISerializationService _serializationService =
            ServicesProvider.GetPlatformService<ISerializationService>();
        public string ErrorInfo { get; set; }

        public List<T> Import(string path) {
            using(StreamReader file = File.OpenText(path)) {
                string allText = file.ReadToEnd();
                return _serializationService.Deserialize<List<T>>(allText);
            }
        }
    }
}
