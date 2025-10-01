using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services;
internal class DuctOffsetFinder : OutcomingTaskOffsetFinder<Duct> {
    private MepCategory _roundCategory;
    private MepCategory _rectangleCategory;

    public DuctOffsetFinder(
        OpeningConfig openingConfig,
        OutcomingTaskGeometryProvider geometryProvider,
        GeometryUtils geometryUtils,
        ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

        TessellationCount = 10;
    }


    protected override int TessellationCount { get; }


    protected override MepCategory GetCategory(Duct mepElement) {
        if(mepElement.DuctType.Shape == ConnectorProfileType.Round) {
            _roundCategory ??= OpeningConfig.Categories[Models.MepCategoryEnum.RoundDuct];
            return _roundCategory;
        } else {
            _rectangleCategory ??= OpeningConfig.Categories[Models.MepCategoryEnum.RectangleDuct];
            return _rectangleCategory;
        }
    }

    protected override double GetHeight(Duct mepElement) {
        return mepElement.DuctType.Shape == ConnectorProfileType.Round ? mepElement.Diameter : mepElement.Height;
    }

    protected override double GetWidth(Duct mepElement) {
        return mepElement.DuctType.Shape == ConnectorProfileType.Round ? mepElement.Diameter : mepElement.Width;
    }

    protected override Solid GetMepSolid(Duct mepElement) {
        return mepElement.GetSolid();
    }
}
