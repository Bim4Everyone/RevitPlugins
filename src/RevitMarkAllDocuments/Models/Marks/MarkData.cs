using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
