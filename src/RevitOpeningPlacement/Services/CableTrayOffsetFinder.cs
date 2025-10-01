using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services;
internal class CableTrayOffsetFinder : OutcomingTaskOffsetFinder<CableTray> {
    private MepCategory _category;

    public CableTrayOffsetFinder(
        OpeningConfig openingConfig,
        OutcomingTaskGeometryProvider geometryProvider,
        GeometryUtils geometryUtils,
        ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

        TessellationCount = 1; // кабельный лоток - прямоугольный, вообще не должно быть изогнутых линий
    }


    protected override int TessellationCount { get; }


    protected override double GetHeight(CableTray mepElement) {
        return mepElement.Height;
    }

    protected override double GetWidth(CableTray mepElement) {
        return mepElement.Width;
    }

    protected override MepCategory GetCategory(CableTray mepElement) {
        _category ??= OpeningConfig.Categories[Models.MepCategoryEnum.CableTray];
        return _category;
    }

    protected override Solid GetMepSolid(CableTray cableTray) {
        // нужен именно средний уровень детализации, на котором геометрия лотка - это параллелепипед
        var options = new Options() {
            ComputeReferences = true,
            IncludeNonVisibleObjects = false,
            DetailLevel = ViewDetailLevel.Medium
        };
        var solids = cableTray.get_Geometry(options)
            .SelectMany(item => item.GetSolids())
            .GetUnitedSolids()
            .ToList();
        return ElementExtensions.UniteSolids(solids);
    }
}
