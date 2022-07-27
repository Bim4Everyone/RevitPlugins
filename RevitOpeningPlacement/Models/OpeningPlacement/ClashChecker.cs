using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;

namespace RevitOpeningPlacement.Models.OpeningPlacement {
    internal class ClashChecker {
        public bool Result { get; set; } = true;

        public ClashChecker CheckMainElementIsMepCurve(ClashModel clashModel) {
            if(Result) {
                Result = clashModel.MainElement.GetElement() is MEPCurve;
            }
            return this;
        }

        public ClashChecker CheckOtherElementIsWall(ClashModel clashModel) {
            if(Result) {
                Result = clashModel.OtherElement.GetElement() is Wall;
            }
            return this;
        }

        public ClashChecker CheckMainElementIsNotVertical(ClashModel clashModel) {
            if(Result) {
                Result = !((MEPCurve) clashModel.MainElement.GetElement()).IsVertical();
            }
            return this;
        }

        public ClashChecker CheckElementsIsNotParallel(ClashModel clashModel) {
            if(Result) {
                Result = !((MEPCurve) clashModel.MainElement.GetElement()).IsParallel((Wall) clashModel.OtherElement.GetElement());
            }
            return this;
        }

        public ClashChecker CheckWallIsNotCurtain(ClashModel clashModel) {
            if(Result) {
                Result = ((Wall) clashModel.OtherElement.GetElement()).WallType.Kind != WallKind.Curtain;
            }
            return this;
        }

        public static bool CheckPipeWallClash(ClashModel clashModel) {
            return new ClashChecker().CheckMainElementIsMepCurve(clashModel)
                                     .CheckOtherElementIsWall(clashModel)
                                     .CheckMainElementIsNotVertical(clashModel)
                                     .CheckElementsIsNotParallel(clashModel)
                                     .CheckWallIsNotCurtain(clashModel)
                                     .Result;
        }
    }
}
