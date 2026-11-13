using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal interface IMarkSerializerService {
    void ExportMarkData(string filePath, MarkData markData);
    MarkData ImportMarkData(string filePath);
}
