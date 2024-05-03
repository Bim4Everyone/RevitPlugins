using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class FittingMinSizeChecker : ClashChecker {
        private readonly MepCategory[] _mepCategories;

        public FittingMinSizeChecker(RevitRepository revitRepository, IClashChecker clashChecker, params MepCategory[] mepCategories) : base(revitRepository, clashChecker) {
            _mepCategories = mepCategories;
        }
        public override bool CheckModel(ClashModel clashModel) {
            var fitting = (FamilyInstance) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            var diameter = fitting.GetMaxDiameter();
            if(diameter > 0) {
                var minDiameter = GetSize(Parameters.Diameter);
                if(diameter >= minDiameter?.GetConvertedValue()) {
                    return true;
                }
            }
            var height = fitting.GetMaxHeight();
            var width = fitting.GetMaxWidth();
            var minHeight = GetSize(Parameters.Height);
            var minWidth = GetSize(Parameters.Width);
            return height >= minHeight?.GetConvertedValue() && width >= minWidth?.GetConvertedValue();
        }

        private Size GetSize(Parameters parameter) {
            return _mepCategories
                .Select(item => item.MinSizes[parameter])
                .FirstOrDefault(item => item != null);
        }

        //public override string GetMessage() => nameof(FittingMinSizeChecker);
        public override string GetMessage() => RevitRepository.SystemCheck;
    }
}
