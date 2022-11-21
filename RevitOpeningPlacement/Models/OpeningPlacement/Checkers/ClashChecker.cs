
using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Extensions;
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
            var verticalityChecker = new MepIsNotVerticalChecker(revitRepository, wallChecker);
            var parallelismChecker = new ElementsIsNotParallelChecker(revitRepository, verticalityChecker);
            return new WallIsNotCurtainChecker(revitRepository, parallelismChecker);
        }

        public static IClashChecker GetMepCurveFloorClashChecker(RevitRepository revitRepository) {
            var mepCurveChecker = new MainElementIsMepCurveChecker(revitRepository, null);
            var intersectionChecker = new GettingIntersectionChecker<MEPCurve, CeilingAndFloor>(revitRepository, mepCurveChecker, new MepCurveClashProvider<CeilingAndFloor>());
            var volumeChecker = new ClashVolumeChecker<MEPCurve, CeilingAndFloor>(revitRepository, intersectionChecker, new MepCurveClashProvider<CeilingAndFloor>());
            var floorChecker = new OtherElementIsFloorChecker(revitRepository, volumeChecker);
            return new MepIsNotHorizontalChecker(revitRepository, floorChecker);
        }

        public static IClashChecker GetFittingFloorClashChecker(RevitRepository revitRepository, params MepCategory[] mepCategories) {
            var fittingChecker = new FittingHasNotSupercomponentChecker(revitRepository, null);
            var intersectionChecker = new GettingIntersectionChecker<FamilyInstance, CeilingAndFloor>(revitRepository, fittingChecker, new FittingClashProvider<CeilingAndFloor>());
            var volumeChecker = new ClashVolumeChecker<FamilyInstance, CeilingAndFloor>(revitRepository, intersectionChecker, new FittingClashProvider<CeilingAndFloor>());
            var floorChecker = new OtherElementIsFloorChecker(revitRepository, volumeChecker);
            var minSizeChecker = new FittingMinSizeChecker(revitRepository, floorChecker, mepCategories);
            var horizontalityChecker = new FittingIsNotHorizontalChecker(revitRepository, minSizeChecker);
            return horizontalityChecker;
        }

        public static IClashChecker GetFittingWallClashChecker(RevitRepository revitRepository, params MepCategory[] mepCategories) {
            var fittingChecker = new FittingHasNotSupercomponentChecker(revitRepository, null);
            var intersectionChecker = new GettingIntersectionChecker<FamilyInstance, Wall>(revitRepository, fittingChecker, new FittingClashProvider<Wall>());
            var volumeChecker = new ClashVolumeChecker<FamilyInstance, Wall>(revitRepository, intersectionChecker, new FittingClashProvider<Wall>());
            var wallChecker = new OtherElementIsWallChecker(revitRepository, volumeChecker);
            var minSizeChecker = new FittingMinSizeChecker(revitRepository, wallChecker, mepCategories);
            var parallelismChecker = new FittingAndWallAreNotParallelChecker(revitRepository, minSizeChecker);
            return parallelismChecker;
        }
    }

    internal class MainElementIsMepCurveChecker : ClashChecker {
        public MainElementIsMepCurveChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos) is MEPCurve;
        }

        public override string GetMessage() => "Нет элемента инженерной системы.";
    }

    internal class MainElementIsFittingChecker : ClashChecker {
        public MainElementIsFittingChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos) is FamilyInstance;
        }

        public override string GetMessage() => "Элемент не является соединительной арматурой.";
    }

    internal class OtherElementIsWallChecker : ClashChecker {
        public OtherElementIsWallChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.OtherElement.GetElement(_revitRepository.DocInfos) is Wall;
        }

        public override string GetMessage() => "Задание на отверстие в стене: Нет элемента стены.";
    }

    internal class MepIsNotVerticalChecker : ClashChecker {
        public MepIsNotVerticalChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return !((MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos)).IsVertical();
        }

        public override string GetMessage() => "Задание на отверстие в стене: Инженерная система расположена вертикально.";
    }

    internal class ElementsIsNotParallelChecker : ClashChecker {
        public ElementsIsNotParallelChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            var curve = (MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            var wall = (Wall) clashModel.OtherElement.GetElement(_revitRepository.DocInfos);
            return !curve.IsParallel(wall) && !curve.RunAlongWall(wall);
        }
        public override string GetMessage() => "Задание на отверстие в стене: Инженерная система расположена параллельно стене.";
    }

    internal class WallIsNotCurtainChecker : ClashChecker {
        public WallIsNotCurtainChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return ((Wall) clashModel.OtherElement.GetElement(_revitRepository.DocInfos)).WallType.Kind != WallKind.Curtain;
        }
        public override string GetMessage() => "Задание на отверстие в стене: Стена относится к витражной системе.";
    }

    internal class OtherElementIsFloorChecker : ClashChecker {
        public OtherElementIsFloorChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.OtherElement.GetElement(_revitRepository.DocInfos) is CeilingAndFloor;
        }
        public override string GetMessage() => "Задание на отверстие в перекрытии: Нет элемента перекрытия.";
    }

    internal class MepIsNotHorizontalChecker : ClashChecker {
        public MepIsNotHorizontalChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return !((MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos)).IsHorizontal();
        }
        public override string GetMessage() => "Задание на отверстие в перекрытии: Инженерная система расположена горизонтально.";
    }

    internal class ClashVolumeChecker<T1, T2> : ClashChecker where T1 : Element where T2 : Element {
        private readonly IClashProvider<T1, T2> _clashProvider;

        public ClashVolumeChecker(RevitRepository revitRepository, IClashChecker clashChecker, IClashProvider<T1, T2> clashProvider) : base(revitRepository, clashChecker) {
            _clashProvider = clashProvider;
        }

        public override bool CheckModel(ClashModel clashModel) {
            var clash = _clashProvider.GetClash(_revitRepository, clashModel);
            try {
                var solid = clash.GetIntersection();
                return solid.Volume > clash.GetConnectorArea() * 0.05;
            } catch {
                return true;
            }
        }
        public override string GetMessage() => "Объем пересечения элементов меньше заданного.";
    }

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
        public override string GetMessage() => "Задание на отверстие в перекрытии: Соединительная арматура расположена горизонтально.";
    }

    internal class FittingAndWallAreNotParallelChecker : ClashChecker {
        public FittingAndWallAreNotParallelChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }

        public override bool CheckModel(ClashModel clashModel) {
            var clash = new FittingClash<Wall>(_revitRepository, clashModel);
            return !clash.Element1.IsParallelToWall(clash.Element2);
        }
        public override string GetMessage() => "Задание на отверстие в стене: Соединительная арматура расположена параллельно стене.";
    }

    internal class FittingHasNotSupercomponentChecker : ClashChecker {
        public FittingHasNotSupercomponentChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            var fitting = (FamilyInstance) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            return fitting.SuperComponent == null;
        }
        public override string GetMessage() => RevitRepository.SystemCheck;
        //public override string GetMessage() => nameof(FittingHasNotSupercomponentChecker);
    }

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

    internal class GettingIntersectionChecker<T1, T2> : ClashChecker where T1 : Element where T2 : Element {
        private readonly IClashProvider<T1, T2> _clashProvider;

        public GettingIntersectionChecker(RevitRepository revitRepository, IClashChecker clashChecker, IClashProvider<T1, T2> clashProvider) : base(revitRepository, clashChecker) {
            _clashProvider = clashProvider;
        }

        public override bool CheckModel(ClashModel clashModel) {
            var clash = _clashProvider.GetClash(_revitRepository, clashModel);
            try {
                var solid = clash.GetIntersection();
            } catch {
                return false;
            }
            return true;
        }
        public override string GetMessage() => "Невозможно получить пересечение элементов.";
    }
}
