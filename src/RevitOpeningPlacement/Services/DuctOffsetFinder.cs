using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal class DuctOffsetFinder : OutcomingTaskOffsetFinder<Duct>, IOutcomingTaskOffsetFinder {
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
                _roundCategory = _roundCategory ?? OpeningConfig.Categories[Models.MepCategoryEnum.RoundDuct];
                return _roundCategory;
            } else {
                _rectangleCategory = _rectangleCategory ?? OpeningConfig.Categories[Models.MepCategoryEnum.RectangleDuct];
                return _rectangleCategory;
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

        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindHorizontalOffsetsSum(opening, mepElement as Duct);
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindVerticalOffsetsSum(opening, mepElement as Duct);
        }

        public double GetMinHorizontalOffsetSum(Element mepElement) {
            return base.GetMinHorizontalOffsetSum(mepElement as Duct);
        }

        public double GetMaxHorizontalOffsetSum(Element mepElement) {
            return base.GetMaxHorizontalOffsetSum(mepElement as Duct);
        }

        public double GetMinVerticalOffsetSum(Element mepElement) {
            return base.GetMinVerticalOffsetSum(mepElement as Duct);
        }

        public double GetMaxVerticalOffsetSum(Element mepElement) {
            return base.GetMaxVerticalOffsetSum(mepElement as Duct);
        }
    }
}
