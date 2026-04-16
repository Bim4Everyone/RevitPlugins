using System.Security.Cryptography;

using Autodesk.Revit.DB;

using pyRevitLabs.Json;

namespace RevitMarkAllDocuments.Models;

internal class MarkedElement {
    public MarkedElement() {
    }

    public MarkedElement(Element element) {
        RevitElement = element;
#if REVIT_2023_OR_LESS
        Id = element.Id.IntegerValue;
#else
        Id = element.Id.Value;
#endif
    }

    [JsonIgnore]
    public Element RevitElement { get; set; }

#if REVIT_2023_OR_LESS
    public int Id { get; set; }
#else
    public long Id { get; set; }
#endif

    public string MarkValue { get; set; }
}
