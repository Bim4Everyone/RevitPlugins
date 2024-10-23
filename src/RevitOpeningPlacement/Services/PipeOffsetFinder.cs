using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.Services {
    internal class PipeOffsetFinder : OutcomingTaskOffsetFinder<Pipe> {
        public PipeOffsetFinder(
            OpeningConfig openingConfig,
            OutcomingTaskGeometryProvider geometryProvider,
            GeometryUtils geometryUtils,
            ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

            TessellationCount = 10;
        }

        protected override int TessellationCount { get; }

        protected override MepCategory GetCategory(Pipe mepElement) {
            return OpeningConfig.Categories[Models.MepCategoryEnum.Pipe];
        }

        protected override double GetHeight(Pipe mepElement) {
            return mepElement.Diameter;
        }

        protected override double GetWidth(Pipe mepElement) {
            return mepElement.Diameter;
        }

        protected override Solid GetMepSolid(Pipe mepElement) {
            return mepElement.GetSolid();
        }
    }
}
