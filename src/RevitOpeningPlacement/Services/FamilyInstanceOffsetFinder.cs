using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Configs;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services {
    internal class FamilyInstanceOffsetFinder : OutcomingTaskOffsetFinder<FamilyInstance>, IOutcomingTaskOffsetFinder {
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

        public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindHorizontalOffsetsSum(opening, mepElement as FamilyInstance);
        }

        public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
            return base.FindVerticalOffsetsSum(opening, mepElement as FamilyInstance);
        }

        public double GetMinHorizontalOffsetSum(Element mepElement) {
            return base.GetMinHorizontalOffsetSum(mepElement as FamilyInstance);
        }

        public double GetMaxHorizontalOffsetSum(Element mepElement) {
            return base.GetMaxHorizontalOffsetSum(mepElement as FamilyInstance);
        }

        public double GetMinVerticalOffsetSum(Element mepElement) {
            return base.GetMinVerticalOffsetSum(mepElement as FamilyInstance);
        }

        public double GetMaxVerticalOffsetSum(Element mepElement) {
            return base.GetMaxVerticalOffsetSum(mepElement as FamilyInstance);
        }
    }
}
