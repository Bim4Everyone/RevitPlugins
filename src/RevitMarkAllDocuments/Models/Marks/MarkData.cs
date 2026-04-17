using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Services;

namespace RevitMarkAllDocuments.Models;

internal class MarkData {
    public RevitParam RevitParam { get; set; }
    public List<MarkDataByDocument> MarkDataByDocument { get; set; } = [];

    public MarkDataByDocument GetDataByDocument(string documentName) {
        return MarkDataByDocument.FirstOrDefault(x => x.DocumentName == documentName);
    }

    public bool HasLinksForExport(string documentName) {
        int documentsForExport = MarkDataByDocument.Count;

        if(documentsForExport == 0) {
            return false;
        } else if(MarkDataByDocument.Count > 1) {
            return true;
        } else if(GetDataByDocument(documentName) == null) {
            return true;
        }

        return false;
    }

    public void CreateMarkValues(IList<RevitParam> sortParams, MarkStartValue startValue) {        
        var sortService = new SortElementService();
        var sortedMarkedElements = sortService.SortElements(GetAllElements(), sortParams);
        int startNumber = int.Parse(startValue.StartValue);

        foreach(var element in sortedMarkedElements) {
            element.MarkValue = $"{startValue.Prefix}{startNumber}{startValue.Suffix}";
            startNumber++;
        }
    }

    public IList<MarkedElement> GetAllElements() {
        return MarkDataByDocument.SelectMany(x => x.Elements).ToList();
    }
}
