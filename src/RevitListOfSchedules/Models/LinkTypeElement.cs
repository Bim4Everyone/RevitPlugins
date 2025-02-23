using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models {
    internal class LinkTypeElement {

        public LinkTypeElement(RevitLinkType revitLinkType) {

            RevitLink = revitLinkType;
            Name = revitLinkType.Name;
            FullName = GetFullName();
            Id = revitLinkType.Id;
        }

        public RevitLinkType RevitLink { get; }
        public string Name { get; }
        public string FullName { get; }
        public ElementId Id { get; }


        public void Reload() {
            RevitLink.Reload();
        }

        private string GetFullName() {
            IDictionary<ExternalResourceType, ExternalResourceReference> dictionary = RevitLink
                .GetExternalResourceReferences();

            return dictionary
                .Values
                .Select(refer => refer.GetReferenceInformation())
                .Select(dic => dic["Path"])
                .First();
        }
    }
}
