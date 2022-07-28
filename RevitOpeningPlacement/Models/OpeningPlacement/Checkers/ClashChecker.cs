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

        public ClashChecker CheckMainElementIsMepCurve(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            if(Result) {
                Result = clashModel.MainElement.GetElement(docInfos) is MEPCurve;
            }
            return this;
        }

        public ClashChecker CheckOtherElementIsWall(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            if(Result) {
                Result = clashModel.OtherElement.GetElement(docInfos) is Wall;
            }
            return this;
        }

        public ClashChecker CheckMainElementIsNotVertical(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            if(Result) {
                Result = !((MEPCurve) clashModel.MainElement.GetElement(docInfos)).IsVertical();
            }
            return this;
        }

        public ClashChecker CheckElementsIsNotParallel(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            if(Result) {
                Result = !((MEPCurve) clashModel.MainElement.GetElement(docInfos)).IsParallel((Wall) clashModel.OtherElement.GetElement(docInfos));
            }
            return this;
        }

        public ClashChecker CheckWallIsNotCurtain(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            if(Result) {
                Result = ((Wall) clashModel.OtherElement.GetElement(docInfos)).WallType.Kind != WallKind.Curtain;
            }
            return this;
        }

        public static bool CheckWallClash(IEnumerable<DocInfo> docInfos, ClashModel clashModel) {
            return new ClashChecker().CheckMainElementIsMepCurve(docInfos, clashModel)
                                     .CheckOtherElementIsWall(docInfos, clashModel)
                                     .CheckMainElementIsNotVertical(docInfos, clashModel)
                                     .CheckElementsIsNotParallel(docInfos, clashModel)
                                     .CheckWallIsNotCurtain(docInfos, clashModel)
                                     .Result;
        }
    }
}
