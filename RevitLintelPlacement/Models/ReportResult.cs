using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.Revit;

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
        private readonly double _rightOffset;
        private readonly double _leftOffset;

        public WrongLintelParameters(RevitRepository revitRepository, 
            IEnumerable<ParameterCheckResult> parameterResults, 
            ConcreteRuleViewModel rule, 
            FamilyInstance lintel, 
            FamilyInstance elementInWall,
            double rightOffset, double leftOffset) {
            this._revitRepository = revitRepository;
            this._parameterResults = parameterResults;
            this._rule = rule;
            this._lintel = lintel;
            this._elementInWall = elementInWall;
            this._rightOffset = rightOffset;
            this._leftOffset = leftOffset;
        }

        public ResultCode Code { get; set; } = ResultCode.WorngLintelParameters;

        public void Handle() {
            var offset = _rule.LintelRightOffsetParameter.RightOffsetInternal;
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
                    case ParameterCheckResult.WrongLintelWidth: {
                        _rule.OpeningWidthParameter.SetTo(_lintel, _elementInWall);
                        break;
                    }
                    case ParameterCheckResult.HasNotLintelRightCorner: {
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, _rightOffset > 0 ? _rightOffset : 0);
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightCorner, 1);
                        break;
                    }
                    case ParameterCheckResult.HasLintelRightCorner: {
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightOffset, _rightOffset > 0 && _rightOffset <= offset ? _rightOffset : offset);
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelRightCorner, 0);
                        break;
                    }
                    case ParameterCheckResult.WrongLintelRightOffset: {
                        _rule.LintelRightOffsetParameter.RightOffset = _rule.LintelLeftOffsetParameter.LeftOffset;
                        _rule.LintelRightOffsetParameter.SetTo(_lintel, _elementInWall);
                        break;
                    }
                    case ParameterCheckResult.HasNotLintelLeftCorner: {
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftOffset, _leftOffset > 0 ? _leftOffset : 0);
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftCorner, 1);
                        break;
                    }
                    case ParameterCheckResult.HasLintelLeftCorner: {
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftOffset, _leftOffset > 0 && _leftOffset <= offset ? _leftOffset : offset);
                        _lintel.SetParamValue(_revitRepository.LintelsCommonConfig.LintelLeftCorner, 0);
                        break;
                    }
                    case ParameterCheckResult.WrongLintelLeftOffset: {
                        _rule.LintelLeftOffsetParameter.SetTo(_lintel, _elementInWall);
                        break;
                    }
                }
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
        WrongLintelType,
        WrongLintelWidth,
        HasNotLintelRightCorner,
        WrongLintelRightOffset,
        HasNotLintelLeftCorner,
        WrongLintelLeftOffset,
        HasLintelRightCorner,
        HasLintelLeftCorner,
    }
}
