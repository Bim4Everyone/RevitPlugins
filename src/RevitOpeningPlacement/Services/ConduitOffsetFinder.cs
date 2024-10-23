using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services {
    internal class ConduitOffsetFinder : OutcomingTaskOffsetFinder<Conduit> {
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
            return OpeningConfig.Categories[Models.MepCategoryEnum.Conduit];
        }

        protected override Solid GetMepSolid(Conduit mepElement) {
            return mepElement.GetSolid();
        }
    }
}
