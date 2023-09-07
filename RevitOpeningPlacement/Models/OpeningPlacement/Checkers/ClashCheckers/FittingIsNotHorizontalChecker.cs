
using System;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class FittingIsNotHorizontalChecker : ClashChecker {
        public FittingIsNotHorizontalChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }

        public override bool CheckModel(ClashModel clashModel) {
            var fitting = (FamilyInstance) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            try {
                return !fitting.IsHorizontal();
            } catch {
                throw new ArgumentException($"ID-{clashModel.MainElement.Id}");
            }

        }
        //public override string GetMessage() => "Задание на отверстие в перекрытии: Соединительная арматура расположена горизонтально.";
        public override string GetMessage() => RevitRepository.SystemCheck;
    }
}
