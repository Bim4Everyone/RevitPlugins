using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class GeneralViewDimensionService {
    internal GeneralViewDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                         PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        FindDimensionTextOffsets();
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }

    // Смещение для размерного сегмента с маленьким текстом
    private XYZ VertDimSmallTextOffset { get; set; }
    // Смещение для размерного сегмента с маленьким текстом инвертированное (зависит от положения в размерной цепочке)
    private XYZ VertDimSmallTextOffsetInverted { get; set; }

    internal void TryCreatePylonDimensions(FamilyInstance skeletonParentRebar, List<Grid> grids,
                                           DimensionBaseService dimensionBaseService, bool isFrontView) {
        var view = ViewOfPylon.ViewElement;
        try {
            var side = isFrontView ? "фронт" : "торец";
            // Ссылки на опорные плоскости - крайние пилона
            var refArrayFormwork = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems.First() as FamilyInstance,
                                                                         '#', '/', [side, "край"]);
            // Размер по ФРОНТУ опалубка + армирование (положение снизу 1)
            var dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                 DirectionType.Bottom, 1.3);
            var refArrayFormworkRebarFront = 
                dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                      ["низ", side, "край"], oldRefArray: refArrayFormwork);
            Repository.Document.Create.NewDimension(view, dimensionLineBottomFirst, refArrayFormworkRebarFront,
                                                    ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                var dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                      DirectionType.Bottom, 1.8);
                var refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, new XYZ(0, 0, 1),
                                                                                      refArrayFormwork);
                Repository.Document.Create.NewDimension(view, dimensionLineBottomSecond, refArrayFormworkGridFront,
                                                        ViewModel.SelectedDimensionType);

                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1,
                    RightOffset = 1,
                    TopOffset = 1,
                    BottomOffset = 2.5
                };
                EditGridEnds(view, SheetInfo.HostElems.First(), grids, transverseViewGridOffsets, dimensionBaseService);
            }

            // Размер по ФРОНТУ опалубка (положение снизу 3)
            var dimensionLineBottomThird = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                 DirectionType.Bottom, 2.3);
            Repository.Document.Create.NewDimension(view, dimensionLineBottomThird, refArrayFormwork,
                                                    ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    private void EditGridEnds(View view, Element rebar, List<Grid> grids, OffsetOption offsetOption,
                              DimensionBaseService dimensionBaseService) {
        if(view is null || rebar is null) { return; }
        var rightDirection = view.RightDirection;

        foreach(var grid in grids) {
            var gridLine = grid.Curve as Line;
            var gridDir = gridLine.Direction;

            if(rightDirection.IsAlmostEqualTo(gridDir)
                || rightDirection.IsAlmostEqualTo(gridDir.Negate())) {

                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Left,
                                                                        offsetOption.LeftOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Right,
                                                                        offsetOption.RightOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Bottom,
                                                                        offsetOption.BottomOffset);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DirectionType.Top,
                                                                        offsetOption.TopOffset);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }

    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreatePylonDimensions(List<Element> hostElems, DimensionBaseService dimensionBaseService,
                                           bool isForPerpView) {
        try {
            // Если этот размер для перпендикулярного вида и Гэшка только слева, то размер нужно ставить справа
            var dimensionLineDirection = isForPerpView
                                         && !SheetInfo.RebarInfo.AllRebarAreL
                                         && SheetInfo.RebarInfo.HasLRebar
                                         && ViewModel.RebarFinder.DirectionHasLRebar(ViewOfPylon.ViewElement,
                                                                                     SheetInfo.ProjectSection,
                                                                                     DirectionType.Left)
                                         ? DirectionType.Right : DirectionType.Left;

            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance,
                                                                          dimensionLineDirection, 1.6);
            ReferenceArray refArray = default;
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }
                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                refArray = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край", "низ"], 
                                                                     oldRefArray: refArray);
            }
            if(refArray != default) {
                var lastFloor = GetLastFloor();
                if(lastFloor != null) {
                    var viewOptions = new Options {
                        View = ViewOfPylon.ViewElement,
                        ComputeReferences = true,
                        IncludeNonVisibleObjects = false
                    };
                    var lastFloorTopFace = GetTopFloorFace(lastFloor, viewOptions);
                    refArray.Append(lastFloorTopFace.Reference);
                }
                Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                        ViewModel.SelectedDimensionType);
            }
        } catch(Exception) { }
    }

    internal void TryCreateTopAdditionalDimensions(FamilyInstance rebar, DimensionBaseService dimensionBaseService,
                                                   bool isForPerpView) {
        try {
            if(SheetInfo.RebarInfo.AllRebarAreL) {
                return;
            }
            var lastFloor = GetLastFloor();
            if(lastFloor is null) {
                return;
            }
            var viewOptions = new Options {
                View = ViewOfPylon.ViewElement,
                ComputeReferences = true,
                IncludeNonVisibleObjects = false
            };
            var lastFloorTopFace = GetTopFloorFace(lastFloor, viewOptions);
            var lastFloorBottomFace = GetBottomFloorFace(lastFloor, viewOptions);

            // Если этот размер для перпендикулярного вида и Гэшка только слева, то размер нужно ставить справа
            var dimensionLineDirection = isForPerpView
                                         && !SheetInfo.RebarInfo.AllRebarAreL
                                         && SheetInfo.RebarInfo.HasLRebar
                                         && ViewModel.RebarFinder.DirectionHasLRebar(ViewOfPylon.ViewElement,
                                                                                     SheetInfo.ProjectSection,
                                                                                     DirectionType.Left)
                                         ? DirectionType.Right : DirectionType.Left;
            // Определяем размерную линию
            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                                          dimensionLineDirection, 1.1);
            // #1_горизонт_выпуск
            var refArray = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', ["горизонт", "выпуск"]);
            refArray.Append(lastFloorTopFace.Reference);
            refArray.Append(lastFloorBottomFace.Reference);
            Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                    ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    private PlanarFace GetTopFloorFace(Element floor, Options options) => GetFloorFace(floor, options)
        .FirstOrDefault(face => Math.Abs(face.FaceNormal.Z - 1) < 0.001);

    private PlanarFace GetBottomFloorFace(Element floor, Options options) => GetFloorFace(floor, options)
        .FirstOrDefault(face => Math.Abs(face.FaceNormal.Z + 1) < 0.001);


    private IEnumerable<PlanarFace> GetFloorFace(Element floor, Options options) => floor.get_Geometry(options)?
        .OfType<Solid>()
        .Where(solid => solid?.Volume > 0)
        .SelectMany(solid => solid.Faces.OfType<PlanarFace>());


    private Element GetLastFloor() {
        var lastPylon = SheetInfo.HostElems.Last();
        var bbox = lastPylon.get_BoundingBox(null);

        // Готовим фильтр для сбор плит в области вокруг верхней точки пилона
        var outline = new Outline(
            bbox.Max - new XYZ(10, 10, 5),
            bbox.Max + new XYZ(10, 10, 5)
        );
        var filter = new BoundingBoxIntersectsFilter(outline);

        return new FilteredElementCollector(Repository.Document)
            .OfCategory(BuiltInCategory.OST_Floors)
            .WherePasses(filter)
            .WhereElementIsNotElementType()
            .FirstOrDefault();
    }


    /// <summary>
    /// Создает размеры между нижней и верхней плоскостью пилона и нижней и верхней плоскостью хомутов
    /// </summary>
    internal void TryCreateClampsDimensions(List<FamilyInstance> clampsParentRebars, 
                                            DimensionBaseService dimensionBaseService,
                                            bool isForPerpView) {
        try {
            ReferenceArray refArray = null;
            // #_1_горизонт_край_низ, #_1_горизонт_край_верх
            foreach(var host in SheetInfo.HostElems) {
                refArray = dimensionBaseService.GetDimensionRefs(host as FamilyInstance, '#', '/', 
                                                                 ["горизонт", "край"], oldRefArray: refArray);
            }
            foreach(var clampsParentRebar in clampsParentRebars) {
                // В связи с особенностями семейства, нельзя задать именем опорной плоскости правила чтобы 
                // забирать именно крайнюю плоскость, т.к. плоскости смещаются и накладываются
                // Всегда отсеивать (они накладываются на #1_горизонт_доборные_низ_край): 
                // #2_горизонт_доборные_низ
                // #5_горизонт_доборные_верх
                bool additionalFirst = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 1") == 1;
                bool additionalFirstArray = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 1_Массив") == 1;

                bool additionalSecond = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 2") == 1;
                bool additionalSecondArray = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 2_Массив") == 1;
                // #3_горизонт_низ
                // мод_ФОП_Доборный 1#2_горизонт_доборные_низ
                // мод_ФОП_Доборный 1/мод_ФОП_Доборный 1_Массив#1_горизонт_доборные_низ_край
                if(additionalFirst && additionalFirstArray) {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/',
                                                                     ["горизонт", "доборные", "низ", "край"],
                                                                     oldRefArray: refArray);
                } else if(additionalFirst) {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/',
                                                                     ["горизонт", "доборные", "низ"], ["край"],
                                                                     oldRefArray: refArray);
                } else {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/', ["горизонт", "низ"],
                                                                     ["доборные", "край"], refArray);
                }

                if(additionalSecond && additionalSecondArray) {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/',
                                                                     ["горизонт", "доборные", "верх", "край"],
                                                                     oldRefArray: refArray);
                } else if(additionalSecond) {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/',
                                                                     ["горизонт", "доборные", "верх"], ["край"],
                                                                     oldRefArray: refArray);
                } else {
                    refArray = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/', ["горизонт", "верх"],
                                                                     ["доборные", "край"], refArray);
                }
            }

            // Если этот размер для перпендикулярного вида и Гэшка только слева, то размер нужно ставить справа
            var dimensionLineDirection = isForPerpView
                                         && !SheetInfo.RebarInfo.AllRebarAreL
                                         && SheetInfo.RebarInfo.HasLRebar
                                         && ViewModel.RebarFinder.DirectionHasLRebar(ViewOfPylon.ViewElement,
                                                                                     SheetInfo.ProjectSection,
                                                                                     DirectionType.Left)
                                         ? DirectionType.Right : DirectionType.Left;
            // Определяем размерную линию
            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                                          dimensionLineDirection, 1.1);
            var dimensionRebarSide =
                Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineLeft, refArray,
                                                        ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }


    /// <summary>
    /// Определяем смещения, которые будут использованы для текста размеров с учетом системы координат вида
    /// </summary>
    private void FindDimensionTextOffsets() {
        // Текст в сегментах размера нужно ставить со смещением от стандартного положения на размерной линии
        // Чтобы он не перекрывал соседние сегменты
        double offsetXYZero = 0;
        double offsetZSmall = 0.4;

        // Т.к. смещение будет зависеть от направления вида, на котором расположен размер, то берем за основу:
        var rightDirection = ViewOfPylon.ViewElement.RightDirection;
        // В зависимости от направления вида рассчитываем смещения
        if(Math.Abs(ViewOfPylon.ViewElement.RightDirection.Y) == 1) {
            VertDimSmallTextOffset = new XYZ(rightDirection.X, rightDirection.Y * offsetXYZero, offsetZSmall);
            VertDimSmallTextOffsetInverted = new XYZ(rightDirection.X, rightDirection.Y * offsetXYZero, -offsetZSmall);
        } else {
            VertDimSmallTextOffset = new XYZ(rightDirection.X * offsetXYZero, rightDirection.Y, offsetZSmall);
            VertDimSmallTextOffsetInverted = new XYZ(rightDirection.X * offsetXYZero, rightDirection.Y, -offsetZSmall);
        }
    }
}
