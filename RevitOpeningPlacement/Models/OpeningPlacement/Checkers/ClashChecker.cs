using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal class ClashChecker {
        public bool Result { get; set; } = true;

        public ClashChecker CheckMainElementIsMepCurve(RevitRepository revitRepository, ClashModel clashModel) {
            if(Result) {
                Result = clashModel.MainElement.GetElement(revitRepository.DocInfos) is MEPCurve;
            }
            return this;
        }

        public ClashChecker CheckOtherElementIsWall(RevitRepository revitRepository, ClashModel clashModel) {
            if(Result) {
                Result = clashModel.OtherElement.GetElement(revitRepository.DocInfos) is Wall;
            }
            return this;
        }

        public ClashChecker CheckMainElementIsNotVertical(RevitRepository revitRepository, ClashModel clashModel) {
            if(Result) {
                Result = !((MEPCurve) clashModel.MainElement.GetElement(revitRepository.DocInfos)).IsVertical();
            }
            return this;
        }

        public ClashChecker CheckElementsIsNotParallel(RevitRepository revitRepository, ClashModel clashModel) {
            if(Result) {
                var curve = (MEPCurve) clashModel.MainElement.GetElement(revitRepository.DocInfos);
                var wall = (Wall) clashModel.OtherElement.GetElement(revitRepository.DocInfos);
                Result = !curve.IsParallel(wall)
                          && !curve.RunAlongWall(wall);
            }
            return this;
        }

        public ClashChecker CheckWallIsNotCurtain(RevitRepository revitRepository, ClashModel clashModel) {
            if(Result) {
                Result = ((Wall) clashModel.OtherElement.GetElement(revitRepository.DocInfos)).WallType.Kind != WallKind.Curtain;
            }
            return this;
        }

        public static bool CheckWallClash(RevitRepository revitRepository, ClashModel clashModel) {
            return new ClashChecker().CheckMainElementIsMepCurve(revitRepository, clashModel)
                                     .CheckOtherElementIsWall(revitRepository, clashModel)
                                     .CheckMainElementIsNotVertical(revitRepository, clashModel)
                                     .CheckElementsIsNotParallel(revitRepository, clashModel)
                                     .CheckWallIsNotCurtain(revitRepository, clashModel)
                                     .Result;
        }
    }
}
