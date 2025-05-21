using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models {
    internal class LinkTypeElement {

        private readonly RevitLinkType _revitLinkType;
        private readonly ElementId _id;
        private readonly string _name;
        private readonly string _fullName;

        public LinkTypeElement(RevitLinkType revitLinkType) {
            _revitLinkType = revitLinkType;
            _id = _revitLinkType.Id;
            _name = _revitLinkType.Name;
            _fullName = GetFullName();
        }

        public RevitLinkType RevitLink => _revitLinkType;
        public ElementId Id => _id;
        public string Name => _name;
        public string FullName => _fullName;

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
