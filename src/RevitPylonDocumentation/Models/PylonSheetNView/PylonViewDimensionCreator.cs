using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
    public class PylonViewDimensionCreator {
        private readonly string _hasFirstLRebarParamName = "ст_Г_1_ВКЛ";
        private readonly string _hasSecondLRebarParamName = "ст_Г_2_ВКЛ";

        private readonly ParamValueService _paramValueService;

        internal PylonViewDimensionCreator(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
            ViewModel = mvm;
            Repository = repository;
            SheetInfo = pylonSheetInfo;

            _paramValueService = new ParamValueService(repository);
        }

        internal MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }
        internal PylonSheetInfo SheetInfo { get; set; }


        public void TryCreateGeneralViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralView.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                var grids = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .Cast<Grid>()
                    .ToList();


                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ФРОНТУ опалубка (положение снизу 1)
                Line dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 2);
                ReferenceArray refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                        new List<string>() { "фронт", "край" });
                Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                           refArrayFormworkFront);

                if(grids.Count > 0) {
                    // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                    Line dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 1.5);
                    ReferenceArray refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, new XYZ(0, 1, 0),
                                                                                  refArrayFormworkFront);
                    Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                                   refArrayFormworkGridFront);
                }
            } catch(Exception) { }
        }

        public void TryCreateTransverseViewFirstDimensions() {
            View view = SheetInfo.TransverseViewFirst.ViewElement;
            TryCreateTransverseViewDimensions(view, false);
        }

        public void TryCreateTransverseViewSecondDimensions() {
            View view = SheetInfo.TransverseViewSecond.ViewElement;
            TryCreateTransverseViewDimensions(view, false);
        }

        public void TryCreateTransverseViewThirdDimensions() {
            View view = SheetInfo.TransverseViewThird.ViewElement;
            TryCreateTransverseViewDimensions(view, true);
        }



        public void TryCreateGeneralRebarViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarView.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom);
                ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdges = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 1.5);
                ReferenceArray refArrayBottomEdges = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdges = doc.Create.NewDimension(view, dimensionLineBottomEdges, refArrayBottomEdges);

                Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top);
                ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);
            } catch(Exception) { }
        }


        public void TryCreateGeneralRebarPerpendicularViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom);
                ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "низ", "торец" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top);
                ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "верх", "торец" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);
            } catch(Exception) { }
        }


        public void TryCreateGeneralRebarPerpendicularViewAdditionalDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, -1);

                ReferenceArray refArrayTop_1 = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "1_торец" });
                Dimension dimensionTop_1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_1);
                if(dimensionTop_1.Value == 0) {
                    doc.Delete(dimensionTop_1.Id);
                }

                ReferenceArray refArrayTop_2 = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "2_торец" });
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



        public void TryCreateTransverseRebarViewFirstDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0.5);
                ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 1);
                ReferenceArray refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge);

                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ армирование (положение справа 2)
                Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                    new List<string>() { "низ", "торец", "край" });
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayRebarSide);
            } catch(Exception) { }
        }

        public void TryCreateTransverseRebarViewSecondDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewSecond.ViewElement;
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 0.5);
                ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);

                Line dimensionLineTopEdge = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 1);
                ReferenceArray refArrayTopEdge = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                  new List<string>() { "верх", "фронт", "край" });
                Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge);

                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ армирование (положение справа 2)
                Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                    new List<string>() { "низ", "торец", "край" });
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayRebarSide);
            } catch(Exception) { }
        }



        private void TryCreateTransverseViewDimensions(View view, bool onTopOfRebar) {
            var doc = Repository.Document;
            string rebarPart = onTopOfRebar ? "верх" : "низ";
            var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                var grids = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .Cast<Grid>()
                    .ToList();


                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ФРОНТУ опалубка (положение снизу 1)
                Line dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 1);
                ReferenceArray refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                        new List<string>() { "фронт", "край" });
                Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                           refArrayFormworkFront);

                if(grids.Count > 0) {
                    // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                    Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 0.5);
                    ReferenceArray refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, new XYZ(0, 1, 0),
                                                                                  refArrayFormworkFront);
                    Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                                   refArrayFormworkGridFront);
                }

                // Определяем наличие в каркасе Г-образных стержней
                var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasFirstLRebarParamName) == 1;
                var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasSecondLRebarParamName) == 1;

                bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
                bool hasLRebar = firstLRebarParamValue || secondLRebarParamValue;

                if(!(onTopOfRebar && allRebarAreL)) {
                    // Размер по ФРОНТУ опалубка + армирование (положение снизу 2)
                    Line dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0.5);
                    // Добавляем ссылки на арматурные стержни
                    ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                                 new List<string>() { rebarPart, "фронт" },
                                                                                 refArrayFormworkFront);
                    Dimension dimensionFormworkRebarFrontFirst = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                                          refArrayFormworkRebarFrontSecond);
                }


                // Размер по ФРОНТУ опалубка + армирование в случае, если есть Г-стержни (положение снизу 0)
                if(onTopOfRebar && hasLRebar) {
                    Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0);
                    // Добавляем ссылки на арматурные стержни
                    ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                                 new List<string>() { "низ", "фронт" },
                                                                                 refArrayFormworkFront);
                    Dimension dimensionFormworkRebarFrontSecond = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                                          refArrayFormworkRebarFrontSecond);
                }


                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ опалубка (положение справа 1)
                Line dimensionLineRightFirst = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 1);
                ReferenceArray refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                       new List<string>() { "торец", "край" });
                Dimension dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                                                                          refArrayFormworkSide);


                // Размер по ТОРЦУ опалубка + армирование (положение справа 2)
                Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#',
                                                                            new List<string>() { rebarPart, "торец" },
                                                                            refArrayFormworkSide);
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayFormworkRebarSide);


                if(grids.Count > 0) {
                    // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                    Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Left, 1.2);
                    ReferenceArray refArrayFormworkGridSide = dimensionBaseService.GetDimensionRefs(grids, new XYZ(1, 0, 0),
                                                                               refArrayFormworkSide);
                    Dimension dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                                                                                  refArrayFormworkGridSide);
                }
            } catch(Exception) { }
        }
    }
}
