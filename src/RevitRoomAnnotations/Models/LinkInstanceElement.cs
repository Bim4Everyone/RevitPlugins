using Autodesk.Revit.DB;


namespace RevitRoomAnnotations.Models;
public class LinkInstanceElement {
    private readonly RevitLinkType _revitLinkType;
    private readonly RevitLinkInstance _revitLinkInstance;

    public LinkInstanceElement(RevitLinkInstance revitLinkInstance, Document document) {
        _revitLinkInstance = revitLinkInstance;
        _revitLinkType = document.GetElement(_revitLinkInstance.GetTypeId()) as RevitLinkType;
    }

    public string Name => _revitLinkInstance.Name;
    public ElementId Id => _revitLinkInstance.Id;
    public bool IsNestedLink => _revitLinkType.IsNestedLink;
    public LinkedFileStatus LinkedFileStatus => _revitLinkType.GetLinkedFileStatus();

    public Document GetLinkDocument() {
        return _revitLinkInstance.GetLinkDocument();
    }
}
