using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;
internal class LinkTypeElement {
    public LinkTypeElement(RevitLinkType revitLinkType) {
        RevitLink = revitLinkType;
        Id = RevitLink.Id;
        Name = RevitLink.Name;
        FullName = GetFullName();
    }

    public RevitLinkType RevitLink { get; }
    public ElementId Id { get; }
    public string Name { get; }
    public string FullName { get; }

    public void Reload() {
        RevitLink.Reload();
    }

    private string GetFullName() {
        var dictionary = RevitLink
            .GetExternalResourceReferences();
        return dictionary
            .Values
            .Select(refer => refer.GetReferenceInformation())
            .Select(dic => dic["Path"])
            .First();
    }
}
