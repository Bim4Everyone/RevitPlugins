using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal class PipeOffsetFinder : OutcomingTaskOffsetFinder<Pipe>, IOutcomingTaskOffsetFinder {
        private MepCategory _category;

        public PipeOffsetFinder(
            OpeningConfig openingConfig,
            OutcomingTaskGeometryProvider geometryProvider,
            GeometryUtils geometryUtils,
            ILengthConverter lengthConverter) : base(openingConfig, geometryProvider, geometryUtils, lengthConverter) {

            TessellationCount = 10;
        }

        protected override int TessellationCount { get; }

        protected override MepCategory GetCategory(Pipe mepElement) {
            _category = _category ?? OpeningConfig.Categories[Models.MepCategoryEnum.Pipe];
            return _category;
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

        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindHorizontalOffsetsSum(opening, mepElement as Pipe);
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindVerticalOffsetsSum(opening, mepElement as Pipe);
        }

        public double GetMinHorizontalOffsetSum(Element mepElement) {
            return base.GetMinHorizontalOffsetSum(mepElement as Pipe);
        }

        public double GetMaxHorizontalOffsetSum(Element mepElement) {
            return base.GetMaxHorizontalOffsetSum(mepElement as Pipe);
        }

        public double GetMinVerticalOffsetSum(Element mepElement) {
            return base.GetMinVerticalOffsetSum(mepElement as Pipe);
        }

        public double GetMaxVerticalOffsetSum(Element mepElement) {
            return base.GetMaxVerticalOffsetSum(mepElement as Pipe);
        }
    }
}
