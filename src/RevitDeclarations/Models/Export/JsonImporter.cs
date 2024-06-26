using System;
using System.Collections.Generic;
using System.IO;

using pyRevitLabs.Json;

namespace RevitDeclarations.Models {
    internal class JsonImporter<T> {
        public string ErrorInfo { get; set; }

        public List<T> Import(string path) {
            List<T> elements = new List<T>();

            using(StreamReader file = File.OpenText(path)) {
                JsonSerializer serializer = new JsonSerializer();
                try {
                    elements = (List<T>)serializer.Deserialize(file, typeof(List<T>));
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
