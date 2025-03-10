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




        public void TryCreateTransverseRebarViewFirstDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseRebarViewFirst.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 2);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 2.5);
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

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 2);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);

                Line dimensionLineTopEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 2.5);
                ReferenceArray refArrayTopEdge = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт", "край" });
                Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge);
            } catch(Exception) { }
        }


        public void TryCreateTransverseViewFirstDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseViewFirst.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                var grids = new FilteredElementCollector(doc, view.Id)
                    .OfCategory(BuiltInCategory.OST_Grids)
                    .Cast<Grid>()
                    .ToList();


                //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
                // Размер по ФРОНТУ опалубка (положение снизу 1)
                Line dimensionLineBottomFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 3);
                ReferenceArray refArrayFormworkFront = GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                                                                        new List<string>() { "фронт", "край" });
                Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                           refArrayFormworkFront);

                //// Размер по ФРОНТУ опалубка + армирование (положение сверху)
                //Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 0);
                //// Добавляем ссылки на арматурные стержни
                //ReferenceArray refArrayFormworkRebarFront = GetDimensionRefs(rebar, '#',
                //                                                             new List<string>() { "низ", "фронт" },
                //                                                             refArrayFormworkFront);
                //Dimension dimensionFormworkRebarFrontBottom = doc.Create.NewDimension(view, dimensionLineTop,
                //                                                                      refArrayFormworkRebarFront);

                //if(grids.Count > 0) {
                //    // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                //    Line dimensionLineBottomSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 1.5);
                //    ReferenceArray refArrayFormworkGridFront = GetDimensionRefs(view, grids, new XYZ(0, 1, 0),
                //                                                                  refArrayFormworkFront);
                //    Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                //                                                                   refArrayFormworkGridFront);
                //}

                #region
                ////ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
                //// Размер по ТОРЦУ опалубка (положение справа 1)
                //Line dimensionLineRightFirst = GetDimensionLine(view, rebar, DimensionOffsetType.Right, 1.5);
                //ReferenceArray refArrayFormworkSide = GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#',
                //                                                       new List<string>() { "торец", "край" });
                //Dimension dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                //                                                          refArrayFormworkSide);


                //// Размер по ТОРЦУ опалубка + армирование (положение справа 2)
                //Line dimensionLineRightSecond = GetDimensionLine(view, rebar, DimensionOffsetType.Right);
                //// Добавляем ссылки на арматурные стержни
                //ReferenceArray refArrayFormworkRebarSide = GetDimensionRefs(rebar, '#',
                //                                                            new List<string>() { "низ", "торец" },
                //                                                            refArrayFormworkSide);
                //Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                //                                                                      refArrayFormworkRebarSide);

                //if(grids.Count > 0) {
                //    // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                //    Line dimensionLineLeft = GetDimensionLine(view, rebar, DimensionOffsetType.Left);
                //    ReferenceArray refArrayFormworkGridSide = GetDimensionRefs(view, grids, new XYZ(1, 0, 0),
                //                                                               refArrayFormworkSide);
                //    Dimension dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                //                                                                  refArrayFormworkGridSide);
                //}
                #endregion
            } catch(Exception) { }
        }


        public void TryCreateTransverseViewSecondDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseViewSecond.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineBottom = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 2);
                ReferenceArray refArrayBottom = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт" });
                Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom);

                Line dimensionLineBottomEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Bottom, 2.5);
                ReferenceArray refArrayBottomEdge = GetDimensionRefs(rebar, '#', new List<string>() { "низ", "фронт", "край" });
                Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge);
            } catch(Exception) { }
        }


        public void TryCreateTransverseViewThirdDimensions() {
            var doc = Repository.Document;
            View view = SheetInfo.TransverseViewThird.ViewElement;

            try {
                var rebar = GetSkeletonRebar(view);
                if(rebar is null) {
                    return;
                }

                Line dimensionLineTop = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 2);
                ReferenceArray refArrayTop = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт" });
                Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop);

                Line dimensionLineTopEdge = GetDimensionLine(view, rebar, DimensionOffsetType.Top, 2.5);
                ReferenceArray refArrayTopEdge = GetDimensionRefs(rebar, '#', new List<string>() { "верх", "фронт", "край" });
                Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge);
            } catch(Exception) { }
        }





        private FamilyInstance GetSkeletonRebar(View view) {
            var rebars = new FilteredElementCollector(Repository.Document, view.Id)
                .OfCategory(BuiltInCategory.OST_Rebar)
                .WhereElementIsNotElementType()
                .ToElements();

            foreach(Element rebar in rebars) {
                // Фильтрация по комплекту документации
                if(rebar.GetParamValue<string>(ViewModel.ProjectSettings.ProjectSection) != SheetInfo.ProjectSection) {
                    continue;
                }
                // Фильтарция по имени семейства
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

            // Создаем матрицу трансформации
            Transform transform = Transform.Identity;
            transform.Origin = origin;
            transform.BasisX = rightDirection;
            transform.BasisY = upDirection;
            transform.BasisZ = viewDirection;

            BoundingBoxXYZ bbox = rebar.get_BoundingBox(view);
            switch(dimensionOffsetType) {
                case DimensionOffsetType.Top:
                    // Получаем единичный вектор вида направления вверх
                    var upDirectionNormalized = upDirection.Normalize();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetTop = upDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем правую верхнюю точку рамки подрезки вида
                    var cropBoxMax = view.CropBox.Max;
                    // Переводим ее в глобальную систему координат + отступ
                    XYZ globalMaxPoint = transform.OfPoint(cropBoxMax) + offsetTop;

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(upDirection.X == -1 || (upDirection.Y == -1)) {
                        pt1 = bbox.Min + offsetTop;
                    } else {
                        pt1 = bbox.Max + offsetTop;
                    }

                    //// Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    //pt1 = bbox.Max + offsetTop;

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамки подрезки
                    //pt1 = pt1.Y > globalMaxPoint.Y ? new XYZ(pt1.X, globalMaxPoint.Y, pt1.Z) : pt1;

                    if(pt1.Y > globalMaxPoint.Y) {
                        pt1 = new XYZ(pt1.X, globalMaxPoint.Y, pt1.Z);
                    }


                    pt2 = pt1 + view.RightDirection;
                    break;
                case DimensionOffsetType.Bottom:
                    // Получаем единичный вектор вида направления вниз
                    var downDirectionNormalized = upDirection.Normalize().Negate();
                    // Получаем отступ для более корректного размещения размера относительно арматуры
                    var offsetBottom = downDirectionNormalized.Multiply(offsetCoefficient);

                    // Получаем левую нижнюю точку рамки подрезки вида
                    var cropBoxMin = view.CropBox.Min;
                    // Переводим ее в глобальную систему координат
                    XYZ cropBoxMinGlobal = transform.OfPoint(cropBoxMin);

                    // Получаем первую точку размерной линии по BoundingBox каркаса армирования + отступ
                    if(upDirection.X == -1 || (upDirection.Y == -1)) {
                        pt1 = bbox.Max + offsetBottom;
                    } else {
                        pt1 = bbox.Min + offsetBottom;
                    }

                    // Если точка, куда нужно поставить размерную линию находится за рамкой подрезки,
                    // то ставим по рамке подрезки
                    //pt1 = pt1.Y < globalMinPoint.Y ? new XYZ(pt1.X, globalMinPoint.Y, pt1.Z) : pt1;

                    var xUpDirectionRounded = Math.Round(upDirection.X);
                    var yUpDirectionRounded = Math.Round(upDirection.Y);

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
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.X, pt1.Z);
                        }
                    }

                    if(xUpDirectionRounded.Equals(-1.0) && yUpDirectionRounded.Equals(0.0)) {
                        if(pt1.X > cropBoxMinGlobal.X) {
                            pt1 = new XYZ(pt1.X, cropBoxMinGlobal.X, pt1.Z);
                        }
                    }

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







        private ReferenceArray GetDimensionRefs(FamilyInstance elem, char keyRefNamePart,
                                                List<string> importantRefNameParts, ReferenceArray refArray = null) {
            var references = new List<Reference>();
            foreach(FamilyInstanceReferenceType referenceType in Enum.GetValues(typeof(FamilyInstanceReferenceType))) {
                references.AddRange(elem.GetReferences(referenceType));
            }

            // # является управляющим символом, сигнализирующим, что плоскость нужно использовать для образмеривания
            // и разделяющим имя плоскости на имя параметра проверки и остальной текст с ключевыми словами
            refArray = refArray ?? new ReferenceArray();
            importantRefNameParts.Add(keyRefNamePart.ToString());
            foreach(Reference reference in references) {
                string referenceName = elem.GetReferenceName(reference);
                if(!importantRefNameParts.All(namePart => referenceName.Contains(namePart))) {
                    continue;
                }

                string paramName = referenceName.Split(keyRefNamePart)[0];
                int paramValue = paramName == string.Empty ? 1 : GetParamValueAnywhere(elem, paramName);

                if(paramValue == 1) {
                    refArray.Append(reference);
                }
            }
            return refArray;
        }


        private ReferenceArray GetDimensionRefs(View view, List<Grid> grids, XYZ direction, ReferenceArray refArray = null) {

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

            refArray = refArray ?? new ReferenceArray();
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


        private int GetParamValueAnywhere(Element elem, string paramName) {
            var paramValue = elem.GetParamValueOrDefault<int>(paramName, 0);
            return paramValue == 0
                ? Repository.Document.GetElement(elem.GetTypeId()).GetParamValueOrDefault<int>(paramName, 0)
                : paramValue;
        }
    }
}
