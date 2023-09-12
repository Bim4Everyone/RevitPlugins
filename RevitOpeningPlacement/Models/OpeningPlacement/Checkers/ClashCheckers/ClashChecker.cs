using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models.OpeningPlacement.Checkers {
    internal abstract class ClashChecker : IClashChecker {
        private protected readonly RevitRepository _revitRepository;
        private protected IClashChecker _wrappee;

        public ClashChecker(RevitRepository revitRepository, IClashChecker clashChecker) {
            _revitRepository = revitRepository;
            _wrappee = clashChecker;
        }

        public string Check(ClashModel model) {
            if(_wrappee != null) {
                var message = _wrappee.Check(model);
                if(!string.IsNullOrEmpty(message)) {
                    return message;
                }
            }

            if(!CheckModel(model)) {
                return GetMessage();
            }
            return null;
        }

        public abstract bool CheckModel(ClashModel clashModel);
        public abstract string GetMessage();

        public static IClashChecker GetMepCurveWallClashChecker(RevitRepository revitRepository) {
            var mepCurveChecker = new MainElementIsMepCurveChecker(revitRepository, null);
            var volumeChecker = new ClashVolumeChecker<MEPCurve, Wall>(revitRepository, mepCurveChecker, new MepCurveClashProvider<Wall>());
            var wallChecker = new OtherElementIsWallChecker(revitRepository, volumeChecker);
            var wallIsStraight = new WallIsStraight(revitRepository, wallChecker);
            var verticalityChecker = new MepIsNotVerticalChecker(revitRepository, wallIsStraight);
            var parallelismChecker = new ElementsIsNotParallelChecker(revitRepository, verticalityChecker);
            return new WallIsNotCurtainChecker(revitRepository, parallelismChecker);
        }

        public static IClashChecker GetMepCurveFloorClashChecker(RevitRepository revitRepository) {
            var mepCurveChecker = new MainElementIsMepCurveChecker(revitRepository, null);
            var floorChecker = new OtherElementIsFloorChecker(revitRepository, mepCurveChecker);
            var intersectionChecker = new GettingIntersectionChecker<MEPCurve, CeilingAndFloor>(revitRepository, floorChecker, new MepCurveClashProvider<CeilingAndFloor>());
            var volumeChecker = new ClashVolumeChecker<MEPCurve, CeilingAndFloor>(revitRepository, intersectionChecker, new MepCurveClashProvider<CeilingAndFloor>());
            return new MepIsNotHorizontalChecker(revitRepository, volumeChecker);
        }

        public static IClashChecker GetFittingFloorClashChecker(RevitRepository revitRepository, params MepCategory[] mepCategories) {
            var fittingChecker = new FittingHasNotSupercomponentChecker(revitRepository, null);
            var floorChecker = new OtherElementIsFloorChecker(revitRepository, fittingChecker);
            var intersectionChecker = new GettingIntersectionChecker<FamilyInstance, CeilingAndFloor>(revitRepository, floorChecker, new FittingClashProvider<CeilingAndFloor>());
            var volumeChecker = new ClashVolumeChecker<FamilyInstance, CeilingAndFloor>(revitRepository, intersectionChecker, new FittingClashProvider<CeilingAndFloor>());
            var minSizeChecker = new FittingMinSizeChecker(revitRepository, volumeChecker, mepCategories);
            var horizontalityChecker = new FittingIsNotHorizontalChecker(revitRepository, minSizeChecker);
            return horizontalityChecker;
        }

        public static IClashChecker GetFittingWallClashChecker(RevitRepository revitRepository, params MepCategory[] mepCategories) {
            var fittingChecker = new FittingHasNotSupercomponentChecker(revitRepository, null);
            var intersectionChecker = new GettingIntersectionChecker<FamilyInstance, Wall>(revitRepository, fittingChecker, new FittingClashProvider<Wall>());
            var volumeChecker = new ClashVolumeChecker<FamilyInstance, Wall>(revitRepository, intersectionChecker, new FittingClashProvider<Wall>());
            var wallChecker = new OtherElementIsWallChecker(revitRepository, volumeChecker);
            var wallIsStraight = new WallIsStraight(revitRepository, wallChecker);
            var minSizeChecker = new FittingMinSizeChecker(revitRepository, wallIsStraight, mepCategories);
            var parallelismChecker = new FittingAndWallAreNotParallelChecker(revitRepository, minSizeChecker);
            return new WallIsNotCurtainChecker(revitRepository, parallelismChecker);
        }
    }
}
