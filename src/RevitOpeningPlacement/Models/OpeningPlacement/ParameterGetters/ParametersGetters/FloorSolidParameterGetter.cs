using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    internal class FloorSolidParameterGetter : IParametersGetter {
        private readonly ISolidProvider _solidProvider;
        private readonly IPointFinder _pointFinder;
        private readonly ILevelFinder _levelFinder;
        private readonly OpeningsGroup _openingsGroup;
        private readonly MepCategory[] _mepCategories;
        private readonly Element _mepElement;
        private readonly Element _structureElement;
        private readonly bool _createdByGroup = false;

        /// <summary>
        /// Конструктор для получения параметров задания на отверстие из группы заданий на отверстия
        /// </summary>
        /// <param name="solidProvider"></param>
        /// <param name="pointFinder"></param>
        /// <param name="levelFinder"></param>
        /// <param name="openingsGroup">Группа заданий на отверстия</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FloorSolidParameterGetter(
            ISolidProvider solidProvider,
            IPointFinder pointFinder,
            ILevelFinder levelFinder,
            OpeningsGroup openingsGroup
            ) {
            _solidProvider = solidProvider ?? throw new System.ArgumentNullException(nameof(solidProvider));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
            _openingsGroup = openingsGroup ?? throw new System.ArgumentNullException(nameof(openingsGroup));
            _createdByGroup = true;
        }

        /// <summary>
        /// Конструктор для получения параметров задания на отверстие из пересечения элемента ВИС и конструкции
        /// </summary>
        /// <param name="solidProvider"></param>
        /// <param name="pointFinder"></param>
        /// <param name="levelFinder"></param>
        /// <param name="mepElement">Элемент ВИС</param>
        /// <param name="structureElement">Элемент конструкции</param>
        /// <param name="mepCategories"></param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public FloorSolidParameterGetter(
            ISolidProvider solidProvider,
            IPointFinder pointFinder,
            ILevelFinder levelFinder,
            Element mepElement,
            Element structureElement,
            params MepCategory[] mepCategories) {

            _solidProvider = solidProvider ?? throw new System.ArgumentNullException(nameof(solidProvider));
            _mepCategories = mepCategories ?? throw new System.ArgumentNullException(nameof(mepCategories));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
            _structureElement = structureElement ?? throw new System.ArgumentNullException(nameof(structureElement));
            _mepElement = mepElement ?? throw new System.ArgumentNullException(nameof(mepElement));
            _createdByGroup = false;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new FloorOpeningSizeInitializer(_solidProvider.GetSolid(), _mepCategories);

            //габариты отверстия
            if(_createdByGroup && _openingsGroup.IsCylinder) {
                yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, sizeInit.GetHeight()).GetParamValue();
            } else {
                yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            }
            if((_structureElement != null) && (_structureElement is Floor ceilingAndFloor)) {
                yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new FloorThicknessValueGetter(ceilingAndFloor)).GetParamValue();
            } else {
                yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
            }

            //отметки отверстия
            var bottomOffsetValueGetter = new BottomOffsetOfOpeningInFloorValueGetter(_pointFinder, sizeInit.GetThickness());
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottomAdsk, new BottomOffsetInFeetValueGetter(bottomOffsetValueGetter)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdsk, new BottomOffsetFromLevelValueGetter(bottomOffsetValueGetter, _levelFinder)).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdsk, new LevelOffsetValueGetter(_levelFinder)).GetParamValue();

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, GetDescriptionValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, GetMepSystemValueGetter()).GetParamValue();
            if(_createdByGroup && _openingsGroup.Elements.Any()) {
                yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_openingsGroup.Elements.First().GetFamilyInstance().Document.Application)).GetParamValue();
            } else {
                yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_mepElement.Document.Application)).GetParamValue();
            }

            //флаг для автоматической расстановки
            yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
        }


        private IValueGetter<StringParamValue> GetDescriptionValueGetter() {
            if(_createdByGroup) {
                return new DescriptionValueGetter(_openingsGroup);
            } else {
                return new DescriptionValueGetter(_mepElement, _structureElement);
            }
        }

        private IValueGetter<StringParamValue> GetMepSystemValueGetter() {
            if(_createdByGroup) {
                return new MepSystemValueGetter(_openingsGroup);
            } else {
                return new MepSystemValueGetter(_mepElement);
            }
        }
    }
}
