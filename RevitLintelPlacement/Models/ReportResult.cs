using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitLintelPlacement.Models.Interfaces;
using RevitLintelPlacement.ViewModels;

namespace RevitLintelPlacement.Models {
    internal class ReportResult : IResultHandler {

        public ReportResult(ElementId id) {
            LintelId = id;
        }

        public ResultCode Code { get; set; }
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

        public ResultCode Code { get; set; }

        public void Handle() {
            _revitRepository.DeleteLintel(_lintel);
        }
    }

    internal class EmptyResult : IResultHandler {
        public ResultCode Code { get; set; }

        public void Handle() { }
    }

    internal class WrongLintelParameters : IResultHandler {
        private readonly RevitRepository _revitRepository;
        private readonly IEnumerable<ParameterCheckResult> _parameterResults;
        private readonly ConcreteRuleViewModel _rule;
        private readonly FamilyInstance _lintel;
        private readonly FamilyInstance _elementInWall;

        public WrongLintelParameters(RevitRepository revitRepository, IEnumerable<ParameterCheckResult> parameterResults, ConcreteRuleViewModel rule, FamilyInstance lintel, FamilyInstance elementInWall) {
            this._revitRepository = revitRepository;
            this._parameterResults = parameterResults;
            this._rule = rule;
            this._lintel = lintel;
            this._elementInWall = elementInWall;
        }

        public ResultCode Code { get; set; } = ResultCode.WorngLintelParameters;

        public void Handle() {
            using(Transaction t  = _revitRepository.StartTransaction("Изменение параметров перемычки")) {
                foreach(var result in _parameterResults) {
                    switch(result) {
                        case ParameterCheckResult.WrongLintelThickness: {
                            _rule.WallHalfThicknessParameter.SetTo(_lintel, _elementInWall);
                            break;
                        }
                        case ParameterCheckResult.WrongLintelType: {
                            _lintel.ChangeTypeId(_revitRepository.GetLintelType(_rule.SelectedLintelType).Id);
                            break;
                        }
                    }
                }
                t.Commit();
            }
        }
    }



    internal enum ResultCode {
        Correct,
        LintelInGroup,
        LintelIsFixedWithoutElement,
        LintelWithoutElement,
        ElementInWallWithoutRule,
        LintelGeometricalDisplaced,
        LintelWithWrongWallAbove,
        WorngLintelParameters
    }

    internal enum ParameterCheckResult {
        Correct,
        WrongLintelThickness,
        WrongLintelType
    }
}
