using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;
using RevitOpeningPlacement.Models.OpeningUnion;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
internal class FloorSolidParameterGetter : IParametersGetter {
    private readonly ISolidProvider _solidProvider;
    private readonly IPointFinder _pointFinder;
    private readonly ILevelFinder _levelFinder;
    private readonly OpeningsGroup _openingsGroup;
    private readonly OpeningConfig _config;
    private readonly MepCategory[] _mepCategories;
    private readonly Element _mepElement;
    private readonly Element _structureElement;
    private readonly bool _createdByGroup = false;

    /// <summary>
    /// Конструктор для получения параметров задания на отверстие из группы заданий на отверстия
    /// </summary>
    /// <param name="openingsGroup">Группа заданий на отверстия</param>
    /// <param name="config">Настройки расстановки заданий на отверстия</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public FloorSolidParameterGetter(
        ISolidProvider solidProvider,
        IPointFinder pointFinder,
        ILevelFinder levelFinder,
        OpeningsGroup openingsGroup,
        OpeningConfig config
        ) {
        _solidProvider = solidProvider ?? throw new System.ArgumentNullException(nameof(solidProvider));
        _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
        _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
        _openingsGroup = openingsGroup ?? throw new System.ArgumentNullException(nameof(openingsGroup));
        _config = config ?? throw new System.ArgumentNullException(nameof(config));
        _createdByGroup = true;
    }

    /// <summary>
    /// Конструктор для получения параметров задания на отверстие из пересечения элемента ВИС и конструкции
    /// </summary>
    /// <param name="mepElement">Элемент ВИС</param>
    /// <param name="structureElement">Элемент конструкции</param>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null</exception>
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
        var sizeInit = _createdByGroup
            ? new FloorOpeningSizeInitializer(_solidProvider.GetSolid(), _config.UnitedTasksSizeRounding)
            : new FloorOpeningSizeInitializer(_solidProvider.GetSolid(), 0, _mepCategories);

        //габариты отверстия
        if(_createdByGroup && _openingsGroup.IsCylinder) {
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, sizeInit.GetHeight()).GetParamValue();
        } else {
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, sizeInit.GetHeight()).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, sizeInit.GetWidth()).GetParamValue();
        }
        var thicknessValueGetter = _structureElement is not null and Floor ceilingAndFloor
            ? new FloorThicknessValueGetter(ceilingAndFloor)
            : sizeInit.GetThickness();
        yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, thicknessValueGetter).GetParamValue();

        //отметки отверстия
        var elevationGetter = _createdByGroup
            ? new ElevationValueGetter(_pointFinder, _openingsGroup.Elements.First().GetFamilyInstance().Document)
            : new ElevationValueGetter(_pointFinder, _mepElement.Document);
        var bottomOffsetValueGetter = new BottomOffsetOfOpeningInFloorValueGetter(elevationGetter, thicknessValueGetter);
        var centerOffsetMmValueGetter = new CenterOffsetValueGetter(elevationGetter);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, centerOffsetMmValueGetter).GetParamValue();
        var offsetFeetFromLevelValueGetter = new OffsetFromLevelValueGetter(centerOffsetMmValueGetter, _levelFinder);
        var levelFeetOffsetValueGetter = new LevelOffsetValueGetter(_levelFinder);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdsk, new OffsetInFeetValueGetter(centerOffsetMmValueGetter)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdsk, offsetFeetFromLevelValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdsk, levelFeetOffsetValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdskOld, centerOffsetMmValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdskOld, new OffsetInMmValueGetter(offsetFeetFromLevelValueGetter)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdskOld, new OffsetInMmValueGetter(levelFeetOffsetValueGetter)).GetParamValue();

        //текстовые данные отверстия
        yield return new StringParameterGetter(RevitRepository.OpeningDate, new DateValueGetter()).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningDescription, GetDescriptionValueGetter()).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, GetMepSystemValueGetter()).GetParamValue();
        yield return _createdByGroup && _openingsGroup.Elements.Any()
            ? new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_openingsGroup.Elements.First().GetFamilyInstance().Document.Application)).GetParamValue()
            : new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_mepElement.Document.Application)).GetParamValue();

        //флаг для автоматической расстановки
        yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
    }


    private IValueGetter<StringParamValue> GetDescriptionValueGetter() {
        return _createdByGroup ? new DescriptionValueGetter(_openingsGroup) : new DescriptionValueGetter(_mepElement, _structureElement);
    }

    private IValueGetter<StringParamValue> GetMepSystemValueGetter() {
        return _createdByGroup ? new MepSystemValueGetter(_openingsGroup) : new MepSystemValueGetter(_mepElement);
    }
}
