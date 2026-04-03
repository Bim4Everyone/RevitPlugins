using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitMarkAllDocuments.Models;

internal class MarkDataByDocument {
    public string DocumentName { get; set; }
    public List<MarkedElement> Elements { get; set; }
}

