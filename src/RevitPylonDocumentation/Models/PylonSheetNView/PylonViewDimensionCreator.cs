using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewDimensionCreator {
        internal PylonViewDimensionCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public void TryCreateGeneralRebarViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarView.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 0.7);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1.2);
                ReferenceArray refArrayBottomEdge = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge);

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);
            } catch(Exception) { }
        }


        public void TryCreateGeneralRebarPerpendicularViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "торец" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "торец" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);
            } catch(Exception) { }
        }


        public void TryCreateGeneralRebarPerpendicularViewAdditionalDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, -1);

                ReferenceArray refArrayTop_1 = GetDimensionRefs(rebar, '#', new List<string>() { "1_торец" });
                Dimension dimensionTop_1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_1);
                if(dimensionTop_1.Value == 0) {
                    doc.Delete(dimensionTop_1.Id);
                }

                ReferenceArray refArrayTop_2 = GetDimensionRefs(rebar, '#', new List<string>() { "2_торец" });
                Dimension dimensionTop_2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_2);
                if(dimensionTop_2.Value == 0) {
                    doc.Delete(dimensionTop_2.Id);
                }

                //// Смещение выноски вправо
                //var rightDirection = GetViewDirections(view).RightDirection;
                //// .Multiply(offsetCoefficient)

                //var dimensionPoint_1 = dimensionBottom_1.LeaderEndPosition;
                //var dimensionPoint_2 = dimensionBottom_2.LeaderEndPosition;

                //dimensionPoint_1 = new XYZ(dimensionPoint_1.X, dimensionPoint_1.Y, 0);
                //dimensionPoint_2 = new XYZ(dimensionPoint_2.X, dimensionPoint_2.Y, 0);

                //var viewMin = view.CropBox.Min;
                //viewMin = new XYZ(viewMin.X, viewMin.Y, 0);

                //if(dimensionPoint_1.DistanceTo(viewMin) < dimensionPoint_2.DistanceTo(viewMin)) {
                //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 + rightDirection;
                //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 - rightDirection;
                //} else {
                //    dimensionBottom_1.LeaderEndPosition = dimensionPoint_1 - rightDirection;
                //    dimensionBottom_2.LeaderEndPosition = dimensionPoint_2 + rightDirection;
                //}
            } catch(Exception) { }
        }


        private FamilyInstance GetSkeletonRebar(View view) {
            var rebars = new FilteredElementCollector(Repository.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(Element rebar in rebars) {
                FamilySymbol rebarType = Repository.Document.GetElement(rebar.GetTypeId()) as FamilySymbol;
                if(rebarType is null) {
                    continue;
                }

                if(rebarType.FamilyName.Equals("IFC_Пилон_Верт.Арм.")) {
                    return rebar as FamilyInstance;
                }
            }
            return null;
        }



        private Line GetDimensionLine(View view, FamilyInstance rebar, DimensionOffsetType dimensionOffsetType,
                                      double offsetCoefficient = 1) {
            var pt1 = new XYZ(0, 0, 0);
            var pt2 = new XYZ(0, 100, 0);

            BoundingBoxXYZ bbox = rebar.get_BoundingBox(view);
            var upDirection = GetViewDirections(view).UpDirection;
            var downDirection = upDirection.Negate();

            switch(dimensionOffsetType) {
                case DimensionOffsetType.Top:
                    pt1 = bbox.Max + upDirection.Multiply(offsetCoefficient);
                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Bottom:
                    pt1 = bbox.Min + downDirection.Multiply(offsetCoefficient);
                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Left:
                    break;
                case DimensionOffsetType.Right:
                    break;
                default:
                    break;
            }
            return Line.CreateBound(pt1, pt2);
        }


        private (XYZ RightDirection, XYZ UpDirection) GetViewDirections(View view) {
            return (view.RightDirection.Normalize(), view.UpDirection.Normalize());
        }


        private ReferenceArray GetDimensionRefs(FamilyInstance rebar, char keyRefNamePart,
                                                List<string> importantRefNameParts) {
            var references = new List<Reference>();
            foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
                references.AddRange(rebar.GetReferences(referenceType));
            }

            // # является управляющим символом, сигнализирующим, что плоскость нужно использовать для образмеривания
            // и разделяющим имя плоскости на имя параметра проверки и остальной текст с ключевыми словами
            ReferenceArray refArray = new ReferenceArray();
            importantRefNameParts.Add(keyRefNamePart.ToString());
            foreach(Reference reference in references) {
                string referenceName = rebar.GetReferenceName(reference);
                if(!importantRefNameParts.All(namePart => referenceName.Contains(namePart))) {
                    continue;
                }

                string paramName = referenceName.Split(keyRefNamePart)[0];
                int paramValue = paramName == string.Empty ? 1 : GetParamValueAnywhere(rebar, paramName);

                if(paramValue == 1) {
                    refArray.Append(reference);
                }
            }
            return refArray;
        }


        private int GetParamValueAnywhere(Element elem, string paramName) {
            var paramValue = elem.GetParamValueOrDefault<int>(paramName, 0);
            return paramValue == 0
                ? Repository.Document.GetElement(elem.GetTypeId()).GetParamValueOrDefault<int>(paramName, 0)
                : paramValue;
        }



        public void TryCreateTransverseRebarViewFirstDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, -0.8);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, -0.3);
                ReferenceArray refArrayBottomEdge = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge);
            } catch(Exception) { }
        }

        public void TryCreateTransverseRebarViewSecondDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewSecond.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, -0.8);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);

                Line dimensionLineTopEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Top, -0.3);
                ReferenceArray refArrayTopEdge = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт", "край" });
                Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge);
            } catch(Exception) { }
        }
    }
}
