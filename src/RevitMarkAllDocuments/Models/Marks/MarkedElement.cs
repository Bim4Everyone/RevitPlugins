using Autodesk.Revit.DB;

using dosymep.Revit;

using pyRevitLabs.Json;

namespace RevitMarkAllDocuments.Models;

internal class MarkedElement {
    public MarkedElement() {
    }

    public MarkedElement(Element element) {
        RevitElement = element;
        Id = element.Id;
    }

    [JsonIgnore]
    public Element RevitElement { get; set; }
    public ElementId Id { get; set; }
    public string MarkValue { get; set; }

    public Element GetElementWithParam(bool isForType, FilterableParam param) {
        return param.IsTypeParam && !isForType ? RevitElement.GetElementType() : RevitElement;
    }
}
