using System.Collections.Generic;

namespace RevitMarkAllDocuments.Models;

internal class MarkDataByDocument {
    public string DocumentName { get; set; }
    public IList<MarkedElement> Elements { get; set; }
}

