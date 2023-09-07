using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters {
    /// <summary>
    /// Класс, предоставляющий значения параметров для заполнения при размещении заданий на отверстия для тех заданий на отверстия,
    /// которые заданы пересечением стены и группы заданий на отверстия, которые надо объединить,
    /// или пересечением стены и нелинейного элемента инженерной системы
    /// </summary>
    internal class WallSolidParameterGetter : IParametersGetter {
        private readonly ISolidProvider _solidProvider;
        private readonly MepCategory[] _mepCategories;
        private readonly IPointFinder _pointFinder;
        private readonly Element _mepElement;
        private readonly Element _structureElement;
        private readonly OpeningsGroup _openingsGroup;
        private readonly bool _createdByOpeningGroup = false;


        public WallSolidParameterGetter(
            ISolidProvider solidProvider,
            IPointFinder pointFinder,
            OpeningsGroup openingsGroup
            ) {
            _solidProvider = solidProvider ?? throw new System.ArgumentNullException(nameof(solidProvider));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _openingsGroup = openingsGroup ?? throw new System.ArgumentNullException(nameof(openingsGroup));
            _createdByOpeningGroup = true;
        }

        public WallSolidParameterGetter(
            ISolidProvider solidProvider,
            IPointFinder pointFinder,
            Element mepElement,
            Element structureElement,
            params MepCategory[] mepCategories) {

            _solidProvider = solidProvider ?? throw new System.ArgumentNullException(nameof(solidProvider));
            _mepCategories = mepCategories ?? throw new System.ArgumentNullException(nameof(mepCategories));
            _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
            _mepElement = mepElement ?? throw new System.ArgumentNullException(nameof(mepElement));
            _structureElement = structureElement ?? throw new System.ArgumentNullException(nameof(structureElement));
            _createdByOpeningGroup = false;
        }


        public IEnumerable<ParameterValuePair> GetParamValues() {
            var sizeInit = new WallOpeningSizeInitializer(_solidProvider.GetSolid(), _mepCategories);
            //габариты отверстия
            if(_createdByOpeningGroup && _openingsGroup.IsCylinder) {
                yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, sizeInit.GetHeight()).GetParamValue();
            } else {
                yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
            }
            if((_structureElement != null) && (_structureElement is Wall wall)) {
                yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(wall)).GetParamValue();
            } else {
                yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, sizeInit.GetThickness()).GetParamValue();
            }

            //отметки отверстия
            if(_createdByOpeningGroup && _openingsGroup.IsCylinder) {
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetValueGetter(_pointFinder)).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetValueGetter(_pointFinder, sizeInit.GetHeight())).GetParamValue();
            } else {
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetOfRectangleOpeningInWallValueGetter(_pointFinder, sizeInit.GetHeight())).GetParamValue();
                yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, new BottomOffsetOfRectangleOpeningInWallValueGetter(_pointFinder)).GetParamValue();
            }

            //текстовые данные отверстия
            yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningDescription, GetDescriptionValueGetter()).GetParamValue();
            yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, GetMepSystemValueGetter()).GetParamValue();
            if(_createdByOpeningGroup && _openingsGroup.Elements.Any()) {
                yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_openingsGroup.Elements.First().GetFamilyInstance().Document.Application)).GetParamValue();
            } else {
                yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_mepElement.Document.Application)).GetParamValue();
            }

            //флаг для автоматической расстановки
            yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
        }


        private IValueGetter<StringParamValue> GetDescriptionValueGetter() {
            if(_createdByOpeningGroup) {
                return new DescriptionValueGetter(_openingsGroup);
            } else {
                return new DescriptionValueGetter(_mepElement, _structureElement);
            }
        }

        private IValueGetter<StringParamValue> GetMepSystemValueGetter() {
            if(_createdByOpeningGroup) {
                return new MepSystemValueGetter(_openingsGroup);
            } else {
                return new MepSystemValueGetter(_mepElement);
            }
        }
    }
}
