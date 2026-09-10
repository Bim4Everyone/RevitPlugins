using System.Collections.Generic;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

namespace RevitMechanicalSpecification.Entities {
    internal class DuctReplacement {
        public DuctReplacement(
            ElementId oldDuctId,
            Duct newDuct,
            IReadOnlyCollection<ElementReplacement> insulations) {
            OldDuctId = oldDuctId;
            NewDuct = newDuct;
            Insulations = insulations;
        }

        public ElementId OldDuctId { get; }
        public Duct NewDuct { get; }
        public IReadOnlyCollection<ElementReplacement> Insulations { get; }
    }
}
