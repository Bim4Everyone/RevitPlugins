using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.Bim4Everyone;

using RevitMarkAllDocuments.Services;

namespace RevitMarkAllDocuments.Models;

internal class MarkData {
    public MarkData() {
        MarkDataByDocument = new List<MarkDataByDocument>();
    }

    public string ParamName { get; set; }
    public List<MarkDataByDocument> MarkDataByDocument { get; set; }

    public IList<MarkedElement> GetAllElements() {
        return MarkDataByDocument.SelectMany(x => x.Elements).ToList();
    }

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

    public void SerMarkValues(IList<RevitParam> sortParams, MarkStartValue startValue) {
        var sortService = new SortElementService();
        var sortedMarkedElements = sortService.SortElements(GetAllElements(), sortParams);
        int startNumber = int.Parse(startValue.StartValue);

        foreach(var element in sortedMarkedElements) {
            element.MarkValue = $"{startValue.Prefix}{startNumber}{startValue.Suffix}";
            startNumber++;
        }
    }
}
