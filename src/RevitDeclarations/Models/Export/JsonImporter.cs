using System;
using System.Collections.Generic;
using System.IO;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class JsonImporter<T> {
        public string ErrorInfo { get; set; }

        public List<T> Import(string path) {
            List<T> elements = new List<T>();

            using(StreamReader file = File.OpenText(path)) {
                try {                  
                    ISerializationService service = ServicesProvider.GetPlatformService<ISerializationService>();
                    string allText = file.ReadToEnd();
                    return service.Deserialize<List<T>>(allText);
                }
                catch(JsonSerializationException e) {
                    ErrorInfo = $"Ошибка сериализации json файла: {e.Message}";
                }
                catch(JsonReaderException e) {
                    ErrorInfo = $"Ошибка в синтаксисе json файла: {e.Message}";
                }
                catch(Exception e) {
                    ErrorInfo = e.Message;
                }
            }

            return elements;
        }
    }
}
