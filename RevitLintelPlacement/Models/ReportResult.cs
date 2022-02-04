using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;

namespace RevitLintelPlacement.Models {
    internal class ReportResult : IResultHandler {
        public ReportResult(ElementId id) {
            LintelId = id;
        }
        public ElementId LintelId { get; }

        public void Handle() { }
    }

    internal class LintelForDeletionResult : IResultHandler {
        private readonly RevitRepository _revitRepository;
        private readonly FamilyInstance _lintel;

        public LintelForDeletionResult(RevitRepository revitRepository, FamilyInstance lintel) {
            this._revitRepository = revitRepository;
            this._lintel = lintel;
        }

        public void Handle() {
            _revitRepository.DeleteLintel(_lintel);
        }
    }


    //internal class LintelInGroup : IResultHandler {
    //    public void Handle() {}
    //}

    //internal class CorrectLintel : IResultHandler {
    //    public void Handle() {}
    //}

    internal class LintelIsFixedWithoutElement : ReportResult {
        public LintelIsFixedWithoutElement(ElementId id) :base(id) {}
    }

    internal class LintelWithoutElement : LintelForDeletionResult {
        public LintelWithoutElement(RevitRepository revitRepository, FamilyInstance lintel) : base(revitRepository, lintel) { }
    }

    internal class ElementInWallWithoutRule : LintelForDeletionResult {
        public ElementInWallWithoutRule(RevitRepository revitRepository, FamilyInstance lintel) : base(revitRepository, lintel) {}
    }

    internal class LintelGeometricalDisplaced : ReportResult {
        public LintelGeometricalDisplaced(ElementId id) :base(id) {}
    }
}
