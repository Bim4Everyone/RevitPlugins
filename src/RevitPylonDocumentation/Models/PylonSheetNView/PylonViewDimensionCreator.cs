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

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

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


        private FamilyInstance GetSkeletonRebar(View view) {
            return new FilteredElementCollector(Repository.Document, view.Id)
                    .OfCategory(BuiltInCategory.OST_Rebar)
                    .WhereElementIsNotElementType()
                    .ToElements()
                    .FirstOrDefault(e => e.GetParam("обр_ФОП_Форма_номер").AsInteger().Equals(10000)) as FamilyInstance;
        }


        private Line GetDimensionLine(View view, FamilyInstance rebar, DimensionOffsetType dimensionOffsetType) {
            var pt1 = new XYZ(0, 0, 0);
            var pt2 = new XYZ(0, 100, 0);

            BoundingBoxXYZ bbox = rebar.get_BoundingBox(view);
            var upDirection = view.UpDirection.Normalize();
            var downDirection = upDirection.Negate();


            switch(dimensionOffsetType) {
                case DimensionOffsetType.Top:
                    pt1 = bbox.Max + upDirection.Multiply(1);
                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Bottom:
                    pt1 = bbox.Min + downDirection.Multiply(1);
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
    }
}
