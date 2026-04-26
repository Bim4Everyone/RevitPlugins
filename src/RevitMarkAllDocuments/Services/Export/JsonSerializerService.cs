using System.IO;

using dosymep.Bim4Everyone.SimpleServices;
using dosymep.SimpleServices;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services.Export;

internal class JsonSerializerService {
    private readonly ISerializationService _serializationService = ServicesProvider
        .GetPlatformService<ISerializationService>();

    public void ExportMarkData(string filePath, MarkData markData) {
        string serializedData = _serializationService.Serialize(markData);
        filePath = Path.ChangeExtension(filePath, "json");

        using var file = File.CreateText(filePath);
        file.Write(serializedData);
    }

    public MarkData ImportMarkData(string filePath) {
        string json = File.ReadAllText(filePath);
        return _serializationService.Deserialize<MarkData>(json);
    }
}
