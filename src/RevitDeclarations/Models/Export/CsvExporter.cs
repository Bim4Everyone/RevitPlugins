using System.Collections.Generic;
using System.IO;

namespace RevitDeclarations.Models {
    internal class CsvExporter {
        public void Export(string path, string data) {
            path = Path.ChangeExtension(path, "scv");

            using(StreamWriter file = File.CreateText(path)) {
                file.Write(data);
            }
        }
    }
}
