using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services;
internal class ConduitOffsetFinder : OutcomingTaskOffsetFinder<Conduit> {
    private MepCategory _category;

    public ConduitOffsetFinder(
        OpeningConfig openingConfig,
        OutcomingTaskGeometryProvider geometryProvider,
        GeometryUtils geometryUtils,
        ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

        TessellationCount = 10;
    }


    protected override int TessellationCount { get; }


    protected override double GetHeight(Conduit mepElement) {
        return mepElement.Diameter;
    }

    protected override double GetWidth(Conduit mepElement) {
        return mepElement.Diameter;
    }

    protected override MepCategory GetCategory(Conduit mepElement) {
        _category ??= OpeningConfig.Categories[Models.MepCategoryEnum.Conduit];
        return _category;
    }

    protected override Solid GetMepSolid(Conduit mepElement) {
        return mepElement.GetSolid();
    }
}
