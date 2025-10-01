using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Value;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.OpeningPlacement.ValueGetters;

namespace RevitOpeningPlacement.Models.OpeningPlacement.ParameterGetters.ParametersGetters;
internal class PerpendicularRoundCurveWallParamGetter : IParametersGetter {
    private readonly MepCurveClash<Wall> _clash;
    private readonly MepCategory _mepCategory;
    private readonly IPointFinder _pointFinder;
    private readonly OpeningType _openingType;
    private readonly ILevelFinder _levelFinder;

    public PerpendicularRoundCurveWallParamGetter(MepCurveClash<Wall> clash, MepCategory mepCategory, IPointFinder pointFinder, OpeningType openingType, ILevelFinder levelFinder) {
        _clash = clash ?? throw new System.ArgumentNullException(nameof(clash));
        _mepCategory = mepCategory ?? throw new System.ArgumentNullException(nameof(mepCategory));
        _pointFinder = pointFinder ?? throw new System.ArgumentNullException(nameof(pointFinder));
        _levelFinder = levelFinder ?? throw new System.ArgumentNullException(nameof(levelFinder));
        _openingType = openingType;
    }

    public IEnumerable<ParameterValuePair> GetParamValues() {
        var openingSizeGetter = new DiameterValueGetter(_clash.Element1, _mepCategory);
        bool openingTaskIsRound = OpeningTaskIsRound();
        //габариты отверстия
        if(openingTaskIsRound) {
            // отверстие круглое
            yield return new DoubleParameterGetter(RevitRepository.OpeningDiameter, openingSizeGetter).GetParamValue();

        } else {
            // отверстие прямоугольное (квадратное)
            yield return new DoubleParameterGetter(RevitRepository.OpeningHeight, openingSizeGetter).GetParamValue();
            yield return new DoubleParameterGetter(RevitRepository.OpeningWidth, openingSizeGetter).GetParamValue();
        }
        yield return new DoubleParameterGetter(RevitRepository.OpeningThickness, new WallThicknessValueGetter(_clash.Element2)).GetParamValue();

        //отметки отверстия
        IValueGetter<DoubleParamValue> bottomOffsetMmValueGetter;
        IValueGetter<DoubleParamValue> centerOffsetMmValueGetter;
        var elevationGetter = new ElevationValueGetter(_pointFinder, _clash.Element1.Document);
        if(openingTaskIsRound) {
            //отверстие круглое
            centerOffsetMmValueGetter = new CenterOffsetValueGetter(elevationGetter);
            bottomOffsetMmValueGetter = new BottomOffsetValueGetter(elevationGetter, openingSizeGetter);
        } else {
            // отверстие прямоугольное (квадратное)
            centerOffsetMmValueGetter = new CenterOffsetOfRectangleOpeningInWallValueGetter(elevationGetter, openingSizeGetter);
            bottomOffsetMmValueGetter = new BottomOffsetOfRectangleOpeningInWallValueGetter(elevationGetter);
        }
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetCenter, centerOffsetMmValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetBottom, bottomOffsetMmValueGetter).GetParamValue();
        var originOffsetMm = openingTaskIsRound ? centerOffsetMmValueGetter : bottomOffsetMmValueGetter;
        var offsetFeetFromLevelValueGetter = new OffsetFromLevelValueGetter(originOffsetMm, _levelFinder);
        var levelFeetOffsetValueGetter = new LevelOffsetValueGetter(_levelFinder);
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdsk, new OffsetInFeetValueGetter(originOffsetMm)).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetFromLevelAdsk, offsetFeetFromLevelValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningLevelOffsetAdsk, levelFeetOffsetValueGetter).GetParamValue();
        yield return new DoubleParameterGetter(RevitRepository.OpeningOffsetAdskOld, originOffsetMm).GetParamValue();
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
        return _openingType == OpeningType.WallRound;
    }
}
