using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

namespace RevitMarkAllDocuments.Models.Export;

internal class JsonExporter {
    private readonly ISerializationService _serializationService =
    ServicesProvider.GetPlatformService<ISerializationService>();

    public void Export(string path, MarkData elements) {
        string text = _serializationService.Serialize(elements);
        path = Path.ChangeExtension(path, "json");

        using var file = File.CreateText(path);
        file.Write(text);
    }
}
