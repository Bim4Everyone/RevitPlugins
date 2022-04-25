using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitWindowGapPlacement.Model {
    internal class GapWindow : BaseElement {
        private readonly FamilyInstance _gapElement;

        public GapWindow(FamilyInstance element, RevitRepository revitRepository)
            : base(element, revitRepository) {
            _gapElement = element;
        }

        protected override XYZ GetPlaceLocation() {
            throw new System.NotImplementedException();
        }

        protected override Element GetHostObject() {
            throw new System.NotImplementedException();
        }

        protected override HostObject GetNextHostObject() {
            throw new System.NotImplementedException();
        }
    }
}