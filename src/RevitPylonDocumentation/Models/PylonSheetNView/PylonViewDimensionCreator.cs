using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;
using Reference = Autodesk.Revit.DB.Reference;
using Transform = Autodesk.Revit.DB.Transform;

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
                Line dimensionLineBottomFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 2);
                ReferenceArray refArrayFormworkFront = GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                        new List<string>() { "фронт", "край" });
                Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                           refArrayFormworkFront);

                if(grids.Count > 0) {
                    // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                    Line dimensionLineBottomSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1.5);
                    ReferenceArray refArrayFormworkGridFront = GetDimensionRefs(view, grids, new XYZ(0, 1, 0),
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

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdges = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1.5);
                ReferenceArray refArrayBottomEdges = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdges = doc.Create.NewDimension(view, dimensionLineBottomEdges, refArrayBottomEdges);

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);
            } catch(Exception) { }
        }

        public void TryCreateGeneralRebarPerpendicularViewDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
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
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
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



        public void TryCreateTransverseRebarViewFirstDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 0.5);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1);
                ReferenceArray refArrayBottomEdge = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge);

                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ армирование (положение справа 2)
                Line dimensionLineRightSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayRebarSide = GetDimensionRefs(rebar, '#',
                                                                    new List<string>() { "низ", "торец", "край" });
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayRebarSide);
            } catch(Exception) { }
        }

        public void TryCreateTransverseRebarViewSecondDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewSecond.ViewElement;

            try {
                var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
                var rebar = rebarFinder.GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 0.5);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);

                Line dimensionLineTopEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 1);
                ReferenceArray refArrayTopEdge = GetDimensionRefs(rebar, '#',
                                                                  new List<string>() { "верх", "фронт", "край" });
                Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge);

                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ армирование (положение справа 2)
                Line dimensionLineRightSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayRebarSide = GetDimensionRefs(rebar, '#',
                                                                    new List<string>() { "низ", "торец", "край" });
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayRebarSide);
            } catch(Exception) { }
        }



        private void TryCreateTransverseViewDimensions(View view, bool onTopOfRebar) {
            var doc = Repository.Document;
            string rebarPart = onTopOfRebar ? "верх" : "низ";

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
                Line dimensionLineBottomFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1);
                ReferenceArray refArrayFormworkFront = GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                        new List<string>() { "фронт", "край" });
                Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                           refArrayFormworkFront);

                if(grids.Count > 0) {
                    // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                    Line dimensionLineBottomSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 0.5);
                    ReferenceArray refArrayFormworkGridFront = GetDimensionRefs(view, grids, new XYZ(0, 1, 0),
                                                                                  refArrayFormworkFront);
                    Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                                   refArrayFormworkGridFront);
                }

                // Определяем наличие в каркасе Г-образных стержней
                var firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasFirstLRebarParamName) == 1;
                var secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasSecondLRebarParamName) == 1;

                bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
                bool hasLRebar = firstLRebarParamValue || secondLRebarParamValue;

                if(!(onTopOfRebar && allRebarAreL)) {
                    // Размер по ФРОНТУ опалубка + армирование (положение сверху 1)
                    Line dimensionLineTopFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 0.5);
                    // Добавляем ссылки на арматурные стержни
                    ReferenceArray refArrayFormworkRebarFrontFirst = GetDimensionRefs(rebar, '#',
                                                                                 new List<string>() { rebarPart, "фронт" },
                                                                                 refArrayFormworkFront);
                    Dimension dimensionFormworkRebarFrontFirst = doc.Create.NewDimension(view, dimensionLineTopFirst,
                                                                                          refArrayFormworkRebarFrontFirst);
                }


                // Размер по ФРОНТУ опалубка + армирование в случае, если есть Г-стержни (положение снизу 0)
                if(onTopOfRebar && hasLRebar) {
                    Line dimensionLineTopSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 0);
                    // Добавляем ссылки на арматурные стержни
                    ReferenceArray refArrayFormworkRebarFrontSecond = GetDimensionRefs(rebar, '#',
                                                                                 new List<string>() { "низ", "фронт" },
                                                                                 refArrayFormworkFront);
                    Dimension dimensionFormworkRebarFrontSecond = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                                          refArrayFormworkRebarFrontSecond);
                }


                //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ТОРЦУ опалубка (положение справа 1)
                Line dimensionLineRightFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Right, 1);
                ReferenceArray refArrayFormworkSide = GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                       new List<string>() { "торец", "край" });
                Dimension dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                                                                          refArrayFormworkSide);


                // Размер по ТОРЦУ опалубка + армирование (положение справа 2)
                Line dimensionLineRightSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Right, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarSide = GetDimensionRefs(rebar, '#',
                                                                            new List<string>() { rebarPart, "торец" },
                                                                            refArrayFormworkSide);
                Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                                      refArrayFormworkRebarSide);


                if(grids.Count > 0) {
                    // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                    Line dimensionLineLeft = GetDimensionLine(view, rebar, DimensionOffsetType.Left, 0.5);
                    ReferenceArray refArrayFormworkGridSide = GetDimensionRefs(view, grids, new XYZ(1, 0, 0),
                                                                               refArrayFormworkSide);
                    Dimension dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                                                                                  refArrayFormworkGridSide);
                }
            } catch(Exception) { }
        }

        private Line GetDimensionLine(View view, FamilyInstance rebar, DimensionOffsetType dimensionOffsetType,
                                      double offsetCoefficient = 1) {
            // Задаем дефолтные точки на случай, если не сработает получение
            var pt1 = new XYZ(0, 0, 0);
            var pt2 = new XYZ(0, 100, 0);

            // Если взять краевую точку рамки подрезки вида, то она будет в локальных координатах вида
            // Для перевода в глобальные координаты получим объект Transform
            // Получаем начало локальной системы координат вида в глобальной системе координат
            XYZ origin = view.Origin;
            XYZ viewDirection = view.ViewDirection;
            XYZ upDirection = view.UpDirection;
            XYZ rightDirection = view.RightDirection;

            var xUpDirectionRounded = Math.Round(upDirection.X);
            var yUpDirectionRounded = Math.Round(upDirection.Y);

            // Создаем матрицу трансформации
            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = rightDirection;
            transform.BasisY = upDirection;
            transform.BasisZ = viewDirection;

            // Получаем правую верхнюю точку рамки подрезки вида в системе координат вида
            var cropBoxMax = view.CropBox.Max;
            // Получаем левую нижнюю точку рамки подрезки вида в системе координат вида
            var cropBoxMin = view.CropBox.Min;

            // Переводим их в глобальную систему координат
            XYZ cropBoxMaxGlobal = transform.OfPoint(cropBoxMax);
            XYZ cropBoxMinGlobal = transform.OfPoint(cropBoxMin);

            BoundingBoxXYZ bbox = rebar.get_BoundingBox(view);
            switch(dimensionOffsetType) {
                case DimensionOffsetType.Top:
                    // Получаем единичный вектор вида направления вверх
                    var upDirectionNormalized = upDirection.Normalize();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetTop = upDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == -1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Min + offsetTop;
                    } else {
                        pt1 = bbox.Max + offsetTop;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.Y > cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.Y < cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X > cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X < cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Bottom:
                    // Получаем единичный вектор вида направления вниз
                    var downDirectionNormalized = upDirection.Normalize().Negate();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetBottom = downDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == -1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Max + offsetBottom;
                    } else {
                        pt1 = bbox.Min + offsetBottom;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамке подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.Y < cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.Y > cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X < cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X > cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Left:
                    // Получаем единичный вектор вида направления вверх
                    var leftDirectionNormalized = rightDirection.Normalize().Negate();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetLeft = leftDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == 1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Max + offsetLeft;
                    } else {
                        pt1 = bbox.Min + offsetLeft;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.X < cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.X > cropBoxMinGlobal.X) {
                            pt1 = new XYZ(cropBoxMinGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y > cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y < cropBoxMinGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + view.UpDirection;
                    break;
                case DimensionOffsetType.Right:
                    // Получаем единичный вектор вида направления вверх
                    var rightDirectionNormalized = rightDirection.Normalize();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetRight = rightDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(xUpDirectionRounded == 1 || (yUpDirectionRounded == -1)) {
                        pt1 = bbox.Min + offsetRight;
                    } else {
                        pt1 = bbox.Max + offsetRight;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(1.0)) {
                        if(pt1.X > cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(0.0) && yUpDirectionRounded.Equals(-1.0)) {
                        if(pt1.X < cropBoxMaxGlobal.X) {
                            pt1 = new XYZ(cropBoxMaxGlobal.X, pt1.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y < cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.Y > cropBoxMaxGlobal.Y) {
                            pt1 = new XYZ(pt1.X, cropBoxMaxGlobal.Y, pt1.Z);
                        }
                    }

                    pt2 = pt1 + view.UpDirection;
                    break;
                default:
                    break;
            }
            return Line.CreateBound(pt1, pt2);
        }


        private ReferenceArray GetDimensionRefs(FamilyInstance elem, char keyRefNamePart,
                                                List<string> importantRefNameParts, ReferenceArray oldRefArray = null) {
            var references = new List<Reference>();
            foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
                references.AddRange(elem.GetReferences(referenceType));
            }

            // # является управляющим символом, сигнализирующим, что плоскость нужно использовать для образмеривания
            // и разделяющим имя плоскости на имя параметра проверки и остальной текст с ключевыми словами
            ReferenceArray refArray = new ReferenceArray();
            if(oldRefArray != null) {
                foreach(Reference reference in oldRefArray) {
                    refArray.Append(reference);
                }
            }

            importantRefNameParts.Add(keyRefNamePart.ToString());
            foreach(Reference reference in references) {
                string referenceName = elem.GetReferenceName(reference);
                if(!importantRefNameParts.All(namePart => referenceName.Contains(namePart))) {
                    continue;
                }

                string paramName = referenceName.Split(keyRefNamePart)[0];
                int paramValue = paramName == string.Empty ? 1 : _paramValueService.GetParamValueAnywhere(elem, paramName);

                if(paramValue == 1) {
                    refArray.Append(reference);
                }
            }
            return refArray;
        }

        private ReferenceArray GetDimensionRefs(View view, List<Grid> grids, XYZ direction, ReferenceArray oldRefArray = null) {

            XYZ origin = view.Origin;
            XYZ viewDirection = view.ViewDirection;
            XYZ upDirection = view.UpDirection;
            XYZ rightDirection = view.RightDirection;

            // Создаем матрицу трансформации
            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = rightDirection;
            transform.BasisY = upDirection;
            transform.BasisZ = viewDirection;

            ReferenceArray refArray = new ReferenceArray();
            if(oldRefArray != null) {
                foreach(Reference reference in oldRefArray) {
                    refArray.Append(reference);
                }
            }

            // Нормализуем направление, заданное для проверки
            XYZ normalizedDirection = direction.Normalize();

            foreach(Grid grid in grids) {
                if(grid.Curve is Line line) {
                    // Получаем направление линии оси
                    XYZ lineDirection = line.Direction.Normalize();

                    XYZ lineDirectionByView = transform.OfVector(lineDirection);


                    if(lineDirectionByView.IsAlmostEqualTo(normalizedDirection, 0.01)
                        || lineDirectionByView.IsAlmostEqualTo(normalizedDirection.Negate(), 0.01)) {

                        Reference gridRef = new Reference(grid);
                        if(gridRef != null) {
                            refArray.Append(gridRef);
                        }
                    }
                }
            }

            return refArray;
        }
    }
}
