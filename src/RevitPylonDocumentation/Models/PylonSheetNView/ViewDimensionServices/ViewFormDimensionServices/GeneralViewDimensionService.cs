using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewFormDimensionServices;
internal class GeneralViewDimensionService {
    internal GeneralViewDimensionService(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo, 
                                         PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    /// <summary>
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreatePylonDimensions(List<Element> hostElems, DimensionBaseService dimensionBaseService) {
        try {
            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(hostElems.First() as FamilyInstance,
                                                           DimensionOffsetType.Left, 1.7);
            foreach(var item in hostElems) {
                if(item is not FamilyInstance hostElem) { return; }

                // Собираем опорные плоскости по опалубке, например:
                // #_1_горизонт_край_низ
                // #_1_горизонт_край_верх
                var refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край"]);
                Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineLeft, refArraySide,
                                                        ViewModel.SelectedDimensionType);
            }
        } catch(Exception) { }
    }

    internal void TryCreatePylonDimensions(FamilyInstance skeletonParentRebar, List<Grid> grids,
                                           DimensionBaseService dimensionBaseService, bool isFrontView) {
        var view = ViewOfPylon.ViewElement;
        try {
            // Размер по ФРОНТУ опалубка (положение снизу 1)
            var side = isFrontView ? "фронт" : "торец";
            var dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                 DimensionOffsetType.Bottom, 2);
            var refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems.First() as FamilyInstance, 
                                                                              '#', '/', [side, "край"]);
            Repository.Document.Create.NewDimension(view, dimensionLineBottomFirst, refArrayFormworkFront, 
                                                    ViewModel.SelectedDimensionType);
            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
                var dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                                      DimensionOffsetType.Bottom, 1.5);
                var refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, new XYZ(0, 0, 1),
                                                                                      refArrayFormworkFront);
                Repository.Document.Create.NewDimension(view, dimensionLineBottomSecond, refArrayFormworkGridFront,
                                                        ViewModel.SelectedDimensionType);

                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1,
                    RightOffset = 1,
                    TopOffset = 1,
                    BottomOffset = 2.2
                };
                EditGridEnds(view, SheetInfo.HostElems.First(), grids, transverseViewGridOffsets, dimensionBaseService);
            }
        } catch(Exception) { }
    }

    /// <summary>
    /// Метод по созданию размера по хомутам на основном виде опалубки
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размер</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств хомутов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreateClampsDimensions(List<FamilyInstance> clampsParentRebars, 
                                            DimensionBaseService dimensionBaseService) {
        try {
            var refArraySide = new ReferenceArray();
            // Собираем опорные плоскости по опалубке, например:
            // #_1_горизонт_край_низ
            // #_1_горизонт_край_верх
            foreach(var item in SheetInfo.HostElems) {
                if(item is FamilyInstance hostElem) {
                    refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край"],
                                                                         oldRefArray: refArraySide);
                }
            }
            // Собираем опорные плоскости по арматуре и заполняем список опций изменений сегментов размера
            var dimSegmentOpts = new List<DimensionSegmentOption>();
            foreach(var clampsParentRebar in clampsParentRebars) {
                refArraySide = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/', ["горизонт"],
                                                                     oldRefArray: refArraySide);
                // Получаем настройки для изменения сегментов размеров
                dimSegmentOpts = GetClampsDimensionSegmentOptions(clampsParentRebar, dimSegmentOpts);
            }
            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                                          DimensionOffsetType.Left, 1);
            var dimensionRebarSide =
                Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineLeft, refArraySide,
                                                        ViewModel.SelectedDimensionType);
            // Применяем опции изменений сегментов размера
            var dimensionSegments = dimensionRebarSide.Segments;
            for(int i = 0; i < dimensionSegments.Size; i++) {
                var dimSegmentMod = dimSegmentOpts[i];

                if(dimSegmentMod.ModificationNeeded) {
                    var segment = dimensionSegments.get_Item(i);
                    segment.Prefix = dimSegmentMod.Prefix;

                    var oldTextPosition = segment.TextPosition;
                    segment.TextPosition = oldTextPosition + dimSegmentMod.TextOffset;
                }
            }
        } catch(Exception) { }
    }

    internal void TryCreateTopAdditionalDimensions(FamilyInstance rebar, DimensionBaseService dimensionBaseService) {
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

            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                               DimensionOffsetType.Left, 1);
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
    /// Данный метод настраивает опции по переопределению сегментов размера на основе параметров семейства армирования
    /// </summary>
    /// <param name="clampsParentRebar">Экземпляр родительского семейства хомутов</param>
    /// <param name="dimSegmentOpts">Список опций, в который можно дописать новые</param>
    /// <returns></returns>
    private List<DimensionSegmentOption> GetClampsDimensionSegmentOptions(FamilyInstance clampsParentRebar,
                                                                          List<DimensionSegmentOption> dimSegmentOpts = null) {
        // Можно передать список для того, чтобы в него дописать новые опции. Если не передали, сделаем новый
        dimSegmentOpts ??= [];

        // Предполагается, что для анализа будет передано родительское семейство хомутов
        // В этому случае нужно учесть, что данное семейство имеет основной массив хомутов, а также доборные массивы
        // хомутов с двух сторон
        // Запрос значений параметров, отвечающих за дополнительный массив хомутов, расположенный перед основным
        int additional1 = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 1");
        double additional1Count = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 1_Количество");
        double additional1Step = clampsParentRebar.GetParamValue<double>("мод_ФОП_Доборный 1_Шаг");
        additional1Step = UnitUtilsHelper.ConvertFromInternalValue(additional1Step);

        // Запрос значений параметров, отвечающих за дополнительный массив хомутов, расположенный после основного
        int additional2 = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 2");
        double additional2Count = clampsParentRebar.GetParamValue<int>("мод_ФОП_Доборный 2_Количество");
        double additional2Step = clampsParentRebar.GetParamValue<double>("мод_ФОП_Доборный 2_Шаг");
        additional2Step = UnitUtilsHelper.ConvertFromInternalValue(additional2Step);

        // Запрос значений параметров, отвечающих за основной массив хомутов
        double generalLen = clampsParentRebar.GetParamValue<double>("мод_ФОП_Массив_Длина");
        generalLen = UnitUtilsHelper.ConvertFromInternalValue(generalLen);
        double generalStep = clampsParentRebar.GetParamValue<double>("мод_ФОП_Массив_шаг");
        generalStep = UnitUtilsHelper.ConvertFromInternalValue(generalStep);

        // Последовательность сегментов размеров имеет строгий порядок. В связи с ограничениями API нет возможности 
        // узнать к каким опорным плоскостям привязаны сегменты размеров, но можно получить порядок сегментов как 
        // в модели. Соответственно ниже составляем последовательность опций, в соответствии с которыми будут 
        // изменяться сегменты размера. Есть два варианта изменений - указание префикса и изменение положения текста.

        // Пояснение к опциям сегментов размеров:
        // - первый сегмент: префикс не пишем
        // - второй сегмент: вписать префикс и изменить положение текста,
        //      если "мод_ФОП_Доборный 1" == true и "мод_ФОП_Доборный 1_Количество" > 1
        // - третий сегмент: никак не меняем
        // - четвертый сегмент: всегда вписывать префикс, но не менять положение текста
        // - пятый сегмент: никак не меняем
        // - шестой сегмент: вписать префикс и изменить положение текста,
        //      если "мод_ФОП_Доборный 2" == true и "мод_ФОП_Доборный 2_Количество" > 1
        // - седьмой сегмент: никак не меняем
        // - восьмой сегмент: никак не меняем


        // Текст в сегментах размера нужно ставить со смещением от стандартного положения на размерной линии
        // Чтобы он не перекрывал соседние сегменты
        double offsetXY = -0.3;
        double offsetZSmall = 0.2;
        double offsetZBig = 0.6;

        // Смещение для размерного сегмента с маленьким текстом
        XYZ vertDimSmallTextOffset;
        // Смещение для размерного сегмента с маленьким текстом инвертированное (зависит от положения в размерной цепочке)
        XYZ vertDimSmallTextOffsetInverted;
        // Смещение для размерного сегмента с большим текстом
        XYZ vertDimBigTextOffset;
        // Смещение для размерного сегмента с большим текстом инвертированное (зависит от положения в размерной цепочке)
        XYZ vertDimBigTextOffsetInverted;

        // Т.к. смещение будет зависеть от направления вида, на котором расположен размер, то берем за основу:
        var rightDirection = ViewOfPylon.ViewElement.RightDirection;
        // В зависимости от направления вида рассчитываем смещения
        if(Math.Abs(ViewOfPylon.ViewElement.RightDirection.Y) == 1) {
            vertDimSmallTextOffset = new XYZ(rightDirection.X, rightDirection.Y * offsetXY, offsetZSmall);
            vertDimSmallTextOffsetInverted = new XYZ(rightDirection.X, rightDirection.Y * offsetXY, -offsetZSmall);

            vertDimBigTextOffset = new XYZ(rightDirection.X, rightDirection.Y * offsetXY, offsetZBig);
            vertDimBigTextOffsetInverted = new XYZ(rightDirection.X, rightDirection.Y * offsetXY, -offsetZBig);
        } else {
            vertDimSmallTextOffset = new XYZ(rightDirection.X * offsetXY, rightDirection.Y, offsetZSmall);
            vertDimSmallTextOffsetInverted = new XYZ(rightDirection.X * offsetXY, rightDirection.Y, -offsetZSmall);

            vertDimBigTextOffset = new XYZ(rightDirection.X * offsetXY, rightDirection.Y, offsetZBig);
            vertDimBigTextOffsetInverted = new XYZ(rightDirection.X * offsetXY, rightDirection.Y, -offsetZBig);
        }

        // Формируем опцию смещения сегментов размера для семейства хомутов
        dimSegmentOpts.Add(new DimensionSegmentOption(true, "", vertDimSmallTextOffset));
        if(additional1 == 1) {
            if(additional1Count > 1) {
                dimSegmentOpts.Add(new DimensionSegmentOption(true,
                                                              $"{additional1Count - 1}х{additional1Step}=",
                                                              vertDimBigTextOffset));
            }
            dimSegmentOpts.Add(new DimensionSegmentOption(false));
        }
        dimSegmentOpts.Add(new DimensionSegmentOption(true,
                                                      $"{generalStep}х{Math.Round(generalLen / generalStep)}=",
                                                      new XYZ()));
        if(additional2 == 1) {
            dimSegmentOpts.Add(new DimensionSegmentOption(false));
            if(additional2Count > 1) {
                dimSegmentOpts.Add(new DimensionSegmentOption(true,
                                                              $"{additional2Count - 1}х{additional2Step}=",
                                                              vertDimBigTextOffsetInverted));
            }
        }
        dimSegmentOpts.Add(new DimensionSegmentOption(true, "", vertDimSmallTextOffsetInverted));
        dimSegmentOpts.Add(new DimensionSegmentOption(false));

        return dimSegmentOpts;
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

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Left,
                                                                        offsetOption.LeftOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right,
                                                                        offsetOption.RightOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                var offsetLine1 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom,
                                                                        offsetOption.BottomOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                var offsetLine2 = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top,
                                                                        offsetOption.TopOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
