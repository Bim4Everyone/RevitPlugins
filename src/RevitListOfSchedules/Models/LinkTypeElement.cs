using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;
internal class LinkTypeElement {
    public LinkTypeElement(RevitLinkType revitLinkType) {
        RevitLink = revitLinkType;
        Id = RevitLink.Id;
        Name = RevitLink.Name;
    }

    public RevitLinkType RevitLink { get; }
    public ElementId Id { get; }
    public string Name { get; }

    public void Reload() {
        RevitLink.Reload();
    }
}
