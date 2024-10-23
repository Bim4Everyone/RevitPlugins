using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services {
    internal class DuctOffsetFinder : OutcomingTaskOffsetFinder<Duct> {
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
                return OpeningConfig.Categories[Models.MepCategoryEnum.RoundDuct];
            } else {
                return OpeningConfig.Categories[Models.MepCategoryEnum.RectangleDuct];
            }
        }

        protected override double GetHeight(Duct mepElement) {
            if(mepElement.DuctType.Shape == ConnectorProfileType.Round) {
                return mepElement.Diameter;
            } else {
                return mepElement.Height;
            }
        }

        protected override double GetWidth(Duct mepElement) {
            if(mepElement.DuctType.Shape == ConnectorProfileType.Round) {
                return mepElement.Diameter;
            } else {
                return mepElement.Width;
            }
        }

        protected override Solid GetMepSolid(Duct mepElement) {
            return mepElement.GetSolid();
        }
    }
}
