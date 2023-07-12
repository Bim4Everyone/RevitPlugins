using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters {
    internal class DescriptionValueGetter : IValueGetter<StringParamValue> {
        private readonly Element _element1;
        private readonly Element _element2;

        public DescriptionValueGetter(Element element1, Element element2) {
            if(element1 is null) {
                throw new ArgumentNullException(nameof(element1));
            }
            if(element2 is null) {
                throw new ArgumentNullException(nameof(element2));
            }
            _element1 = element1;
            _element2 = element2;
        }


        public StringParamValue GetValue() {
            var firstElementName = _element1.Name;
            var firstElementId = _element1.Id.IntegerValue;
            var secondElementName = _element2.Name;
            var secondElementId = _element2.Id.IntegerValue;

            var description = firstElementName + ": " + firstElementId + "; " + secondElementName + ": " + secondElementId;
            return new StringParamValue(description);
        }
    }
}
