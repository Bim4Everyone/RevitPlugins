using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitMarkAllDocuments.Models;

namespace RevitMarkAllDocuments.Services;

internal interface IFiltrationService {
    MarkData FilterElements(MarkData markData, IReadOnlyList<Document> documents);
}
