using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;

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

        public static IClashChecker GetWallClashChecker(RevitRepository revitRepository) {
            var mepCurveChecker = new MainElementIsMepCurveChecker(revitRepository, null);
            var volumeChecker = new ClashVolumeChecker(revitRepository, mepCurveChecker);
            var wallChecker = new OtherElementIsWallChecker(revitRepository, volumeChecker);
            var verticalityChecker = new MepIsNotVerticalChecker(revitRepository, wallChecker);
            var parallelismChecker = new ElementsIsNotParallelChecker(revitRepository, verticalityChecker);
            return new WallIsNotCurtainChecker(revitRepository, parallelismChecker);
        }

        public static IClashChecker GetFloorClashChecker(RevitRepository revitRepository) {
            var mepCurveChecker = new MainElementIsMepCurveChecker(revitRepository, null);
            var volumeChecker = new ClashVolumeChecker(revitRepository, mepCurveChecker);
            var floorChecker = new OtherElementIsFloorChecker(revitRepository, volumeChecker);
            return new MepIsNotHorizontalChecker(revitRepository, floorChecker);
        }
    }

    internal class MainElementIsMepCurveChecker : ClashChecker {
        public MainElementIsMepCurveChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            return clashModel.MainElement.GetElement(_revitRepository.DocInfos) is MEPCurve;
        }

        public override string GetMessage() => "Нет элемента инженерной системы.";
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

    internal class ClashVolumeChecker : ClashChecker {
        public ClashVolumeChecker(RevitRepository revitRepository, IClashChecker clashChecker) : base(revitRepository, clashChecker) { }
        public override bool CheckModel(ClashModel clashModel) {
            var element1 = (MEPCurve) clashModel.MainElement.GetElement(_revitRepository.DocInfos);
            var element2 = clashModel.OtherElement.GetElement(_revitRepository.DocInfos);
            var transform = _revitRepository.GetTransform(element2);
            try {
                var solid = BooleanOperationsUtils.ExecuteBooleanOperation(element1.GetSolid(),
                SolidUtils.CreateTransformed(element2.GetSolid(), transform),
                BooleanOperationsType.Intersect);
                return solid.Volume > element1.GetConnectorArea() * 0.05;
            } catch {
                return true;
            }
        }
        public override string GetMessage() => "Объем пересечения элементов меньше заданного.";
    }
}
