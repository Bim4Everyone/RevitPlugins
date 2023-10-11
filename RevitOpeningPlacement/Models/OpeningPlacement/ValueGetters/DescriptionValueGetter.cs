using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DescriptionValueGetter : IValueGetter<StringParamValue> {
        private readonly Element _element1;
        private readonly Element _element2;
        private readonly OpeningsGroup _openingsGroup;
        private readonly bool _createdByOpeningGroup = false;

        public DescriptionValueGetter(OpeningsGroup openingsGroup) {
            _openingsGroup = openingsGroup ?? throw new ArgumentNullException(nameof(openingsGroup));
            _createdByOpeningGroup = true;
        }

        public DescriptionValueGetter(Element element1, Element element2) {
            _element1 = element1 ?? throw new ArgumentNullException(nameof(element1));
            _element2 = element2 ?? throw new ArgumentNullException(nameof(element2));
            _createdByOpeningGroup = false;
        }


        public StringParamValue GetValue() {
            if(_createdByOpeningGroup) {
                return new StringParamValue(_openingsGroup.GetDescription());
            } else {
                var firstElementName = _element1.Name;
                var firstElementId = _element1.Id.GetIdValue();
                var secondElementName = _element2.Name;
                var secondElementId = _element2.Id.GetIdValue();

                var description = firstElementName + ": " + firstElementId + "; " + secondElementName + ": " + secondElementId;
                return new StringParamValue(description);
            }
        }
    }
}
