using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
internal class PerpendicularRectangleCurveFloorParamGetter : IParametersGetter {
    private readonly MepCurveClash<CeilingAndFloor> _clash;
    private readonly MepCategory _mepCategory;
    private readonly IPointFinder _pointFinder;
    private readonly ILevelFinder _levelFinder;

    public PerpendicularRectangleCurveFloorParamGetter(MepCurveClash<CeilingAndFloor> clash, MepCategory mepCategory, IPointFinder pointFinder, ILevelFinder levelFinder) {
        _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
        _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
        _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
        _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
    }

    public IEnumerable<ParameterValuePair> GetParamValues() {
        var floorThicknessValueGetter = new FloorThicknessValueGetter(_clash.Element2);

        //габариты отверстия
        yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, new HeightValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new WidthValueGetter(_clash.Element1, _mepCategory)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, floorThicknessValueGetter).GetParamValue();

        //отметки отверстия
        var elevationGetter = new ElevationValueGetter(_pointFinder, _clash.Element1.Document);
        var bottomOffsetValueGetter = new BottomOffsetOfOpeningInFloorValueGetter(elevationGetter, floorThicknessValueGetter);
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
        yield return new StringParameterGetter(RevitRepository.OpeningDescription, new DescriptionValueGetter(_clash.Element1, _clash.Element2)).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningMepSystem, new MepSystemValueGetter(_clash.Element1)).GetParamValue();
        yield return new StringParameterGetter(RevitRepository.OpeningAuthor, new UsernameGetter(_clash.Element1.Document.Application)).GetParamValue();

        //флаг для автоматической расстановки
        yield return new IntegerParameterGetter(RevitRepository.OpeningIsManuallyPlaced, new IsManuallyPlacedValueGetter()).GetParamValue();
    }
}
