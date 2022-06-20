using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitWindowGapPlacement.Model {
    internal class GapElementWindow : BaseElement {
        private readonly FamilyInstance _gapElement;

        public GapElementWindow(FamilyInstance element, RevitRepository revitRepository)
            : base(element, revitRepository) {
            _gapElement = element;
        }
        
        public override double Width 
            => _gapElement.GetParamValue<double>(BuiltInParameter.WINDOW_WIDTH);
        
        public override double Height 
            => _gapElement.GetParamValue<double>(BuiltInParameter.WINDOW_HEIGHT);

        protected override Element GetHostObject() {
            throw new System.NotImplementedException();
        }

        protected override HostObject GetNextHostObject() {
            throw new System.NotImplementedException();
        }
    }
}