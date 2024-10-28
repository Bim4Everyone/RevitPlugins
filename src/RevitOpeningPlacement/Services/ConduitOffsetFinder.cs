using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal class ConduitOffsetFinder : OutcomingTaskOffsetFinder<Conduit>, IOutcomingTaskOffsetFinder {
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
            _category = _category ?? OpeningConfig.Categories[Models.MepCategoryEnum.Conduit];
            return _category;
        }

        protected override Solid GetMepSolid(Conduit mepElement) {
            return mepElement.GetSolid();
        }

        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindHorizontalOffsetsSum(opening, mepElement as Conduit);
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindVerticalOffsetsSum(opening, mepElement as Conduit);
        }

        public double GetMinHorizontalOffsetSum(Element mepElement) {
            return base.GetMinHorizontalOffsetSum(mepElement as Conduit);
        }

        public double GetMaxHorizontalOffsetSum(Element mepElement) {
            return base.GetMaxHorizontalOffsetSum(mepElement as Conduit);
        }

        public double GetMinVerticalOffsetSum(Element mepElement) {
            return base.GetMinVerticalOffsetSum(mepElement as Conduit);
        }

        public double GetMaxVerticalOffsetSum(Element mepElement) {
            return base.GetMaxVerticalOffsetSum(mepElement as Conduit);
        }
    }
}
