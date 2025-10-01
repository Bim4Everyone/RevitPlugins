using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
internal class InclinedRectangleCurveWallParameterGetter : IParametersGetter {
    private readonly MepCurveClash<Wall> _clash;
    private readonly MepCategory _mepCategory;
    private readonly IPointFinder _pointFinder;
    private readonly ILevelFinder _levelFinder;

    public InclinedRectangleCurveWallParameterGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder, ILevelFinder levelFinder) {
        _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
        _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
        _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
        _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
    }

    public IEnumerable<ParameterValuePair> GetParamValues() {
        var heightValueGetter = new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepHeightGetter();

        //габариты отверстия
        yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, heightValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, new InclinedSizeInitializer(_clash, _mepCategory).GetRectangleMepWidthGetter()).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash.Element2)).GetParamValue();

        //отметки отверстия
        var elevationGetter = new ElevationValueGetter(_pointFinder, _clash.Element1.Document);
        var bottomOffsetValueGetter = new BottomOffsetOfRectangleOpeningInWallValueGetter(elevationGetter);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, new CenterOffsetOfRectangleOpeningInWallValueGetter(elevationGetter, heightValueGetter)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetValueGetter).GetParamValue();
        var offsetFeetFromLevelValueGetter = new OffsetFromLevelValueGetter(bottomOffsetValueGetter, _levelFinder);
        var levelFeetOffsetValueGetter = new LevelOffsetValueGetter(_levelFinder);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdsk, new OffsetInFeetValueGetter(bottomOffsetValueGetter)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdsk, offsetFeetFromLevelValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdsk, levelFeetOffsetValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdskOld, bottomOffsetValueGetter).GetParamValue();
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
