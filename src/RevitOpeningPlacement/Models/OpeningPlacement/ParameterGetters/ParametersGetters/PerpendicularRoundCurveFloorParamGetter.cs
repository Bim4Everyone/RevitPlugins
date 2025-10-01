using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
internal class PerpendicularRoundCurveFloorParamGetter : IParametersGetter {
    private readonly MepCurveClash<CeilingAndFloor> _clash;
    private readonly MepCategory _mepCategory;
    private readonly IPointFinder _pointFinder;
    private readonly OpeningType _openingType;
    private readonly ILevelFinder _levelFinder;

    public PerpendicularRoundCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory, IPointFinder pointFinder, OpeningType openingType, ILevelFinder levelFinder) {
        _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
        _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
        _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
        _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
        _openingType = openingType;
    }

    public IEnumerable<ParameterValuePair> GetParamValues() {
        var floorThicknessValueGetter = new FloorThicknessValueGetter(_clash.Element2);
        var diameterValueGetter = new DiameterValueGetter(_clash.Element1, _mepCategory);

        //габариты отверстия
        bool isRound = OpeningTaskIsRound();
        if(isRound) {
            // круглое задание на отверстие
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, diameterValueGetter).GetParamValue();

        } else {
            // прямоугольное (квадратное) задание на отверстие
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, diameterValueGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, diameterValueGetter).GetParamValue();
        }
        yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, floorThicknessValueGetter).GetParamValue();

        //отметки отверстия
        var elevationGetter = new ElevationValueGetter(_pointFinder, _clash.Element1.Document);
        var bottomOffsetMmValueGetter = new BottomOffsetOfOpeningInFloorValueGetter(elevationGetter, floorThicknessValueGetter);
        IValueGetter<DoubleParamValue> centerOffsetMmValueGetter = new CenterOffsetValueGetter(elevationGetter);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, centerOffsetMmValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetMmValueGetter).GetParamValue();
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
        yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_clash.Element1.Document.Application)).GetParamValue();

        //флаг для автоматической расстановки
        yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
    }

    /// <summary>
    /// True - если размещаемое задание на отверстие - круглое, иначе False - задание на отверстие прямоугольное (квадратное)
    /// </summary>
    private bool OpeningTaskIsRound() {
        return _openingType == OpeningType.FloorRound;
    }
}
