using Autodesk.Revit.DB;


namespace RevitRoomAnnotations.Models;
public class LinkInstanceElement {
    private readonly Document _document;
    private readonly RevitLinkInstance _revitLinkInstance;
    private readonly RevitLinkType _revitLinkType;

    public LinkInstanceElement(Document document, RevitLinkInstance revitLinkInstance) {
        _document = document;
        _revitLinkInstance = revitLinkInstance;
        _revitLinkType = GetRevitLinkType(_revitLinkInstance.GetTypeId());
    }

    public string Name => GetLinkName();
    public LinkedFileStatus LinkedFileStatus => _revitLinkType.GetLinkedFileStatus();

    public Document GetLinkDocument() {
        return _revitLinkInstance.GetLinkDocument();
    }

    private string GetLinkName() {
        return _revitLinkType.IsNestedLink
            ? $"{GetRevitLinkType(_revitLinkType.GetParentId()).Name}: {_revitLinkType.Name}"
            : _revitLinkType.Name;
    }

    private RevitLinkType GetRevitLinkType(ElementId elementId) {
        return _document.GetElement(elementId) as RevitLinkType;
    }
}
