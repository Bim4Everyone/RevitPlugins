using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services {
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
            switch(mepElement.Category.GetBuiltInCategory()) {
                case BuiltInCategory.OST_PipeFitting:
                    return OpeningConfig.Categories[Models.MepCategoryEnum.Pipe];
                case BuiltInCategory.OST_DuctFitting:
                case BuiltInCategory.OST_DuctAccessory:
                    if(mepElement.MEPModel.ConnectorManager.Connectors
                        .OfType<Connector>()
                        .All(c => c.Shape == ConnectorProfileType.Round)) {
                        return OpeningConfig.Categories[Models.MepCategoryEnum.RoundDuct];
                    } else {
                        return OpeningConfig.Categories[Models.MepCategoryEnum.RectangleDuct];
                    }
                case BuiltInCategory.OST_CableTrayFitting:
                    return OpeningConfig.Categories[Models.MepCategoryEnum.CableTray];
                case BuiltInCategory.OST_ConduitFitting:
                    return OpeningConfig.Categories[Models.MepCategoryEnum.Conduit];
                default:
                    throw new InvalidOperationException();
            }
        }

        protected override double GetHeight(FamilyInstance mepElement) {
            throw new NotImplementedException();
        }

        protected override double GetWidth(FamilyInstance mepElement) {
            throw new NotImplementedException();
        }

        protected override Solid GetMepSolid(FamilyInstance mepElement) {
            return mepElement.GetSolid();
        }
    }
}
