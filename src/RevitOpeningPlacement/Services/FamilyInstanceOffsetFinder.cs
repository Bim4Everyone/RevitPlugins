using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services;
internal class FamilyInstanceOffsetFinder : OutcomingTaskOffsetFinder<FamilyInstance> {
    public FamilyInstanceOffsetFinder(
        OpeningConfig openingConfig,
        OutcomingTaskGeometryProvider geometryProvider,
        GeometryUtils geometryUtils,
        ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

        TessellationCount = 10;
    }


    protected override int TessellationCount { get; }


    protected override MepCategory GetCategory(FamilyInstance mepElement) {
        return mepElement.Category.GetBuiltInCategory() switch {
            BuiltInCategory.OST_PipeFitting => OpeningConfig.Categories[Models.MepCategoryEnum.Pipe],
            BuiltInCategory.OST_DuctFitting or BuiltInCategory.OST_DuctAccessory => mepElement.MEPModel.ConnectorManager.Connectors
                                    .OfType<Connector>()
                                    .All(c => c.Shape == ConnectorProfileType.Round)
                                    ? OpeningConfig.Categories[Models.MepCategoryEnum.RoundDuct]
                                    : OpeningConfig.Categories[Models.MepCategoryEnum.RectangleDuct],
            BuiltInCategory.OST_CableTrayFitting => OpeningConfig.Categories[Models.MepCategoryEnum.CableTray],
            BuiltInCategory.OST_ConduitFitting => OpeningConfig.Categories[Models.MepCategoryEnum.Conduit],
            _ => throw new InvalidOperationException(),
        };
    }

    protected override double GetHeight(FamilyInstance mepElement) {
        var box = mepElement.GetBoundingBox();
        // будем брать наибольшее расстояние по осям OY и OZ. Такой точности сейчас хватает.
        return Math.Max(box.Max.Z - box.Min.Z, box.Max.Y - box.Min.Y);
    }

    protected override double GetWidth(FamilyInstance mepElement) {
        var box = mepElement.GetBoundingBox();
        return box.Max.X - box.Min.X;
    }

    protected override Solid GetMepSolid(FamilyInstance mepElement) {
        return mepElement.GetSolid();
    }
}
