using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows.Controls;
using System.Windows.Media;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Revit;

using RevitPylonDocumentation.ViewModels;

using Grid = Autodesk.Revit.DB.Grid;
using Line = Autodesk.Revit.DB.Line;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
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
        var view = SheetInfo.GeneralView.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var skeletonParentRebar = rebarFinder.GetSkeletonParentRebar(view);
            if(skeletonParentRebar is null) {
                return;
            }

            var clampsParentRebars = rebarFinder.GetClampsParentRebars(view);
            if(clampsParentRebars is null) { 
                return; 
            }

            var grids = new FilteredElementCollector(doc, view.Id)
                .OfCategory(BuiltInCategory.OST_Grids)
                .Cast<Grid>()
                .ToList();

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            CreateGeneralViewPylonDimensions(view, skeletonParentRebar, grids, dimensionBaseService);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            CreateGeneralViewClampsDimensions(view, clampsParentRebars, dimensionBaseService);

            CreateGeneralViewTopAdditionalDimensions(view, skeletonParentRebar, dimensionBaseService);

            CreateGeneralViewPylonDimensions(view, SheetInfo.HostElems, dimensionBaseService);
        } catch(Exception) { }
    }


    private void CreateGeneralViewTopAdditionalDimensions(View view,
                                               FamilyInstance rebar,
                                               DimensionBaseService dimensionBaseService) {
        // Определяем наличие в каркасе Г-образных стержней
        bool firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasFirstLRebarParamName) == 1;
        bool secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasSecondLRebarParamName) == 1;

        bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
        if(allRebarAreL) {
            return;
        }

        var lastFloor = GetLastFloor();
        if(lastFloor is null) {
            return;
        }

        var viewOptions = new Options {
            View = view,
            ComputeReferences = true,
            IncludeNonVisibleObjects = false
        };
        var lastFloorTopFace = GetTopFloorFace(lastFloor, viewOptions);
        var lastFloorBottomFace = GetBottomFloorFace(lastFloor, viewOptions);

        Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                           DimensionOffsetType.Left, 1);
        // #1_горизонт_выпуск
        ReferenceArray refArray = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', ["горизонт", "выпуск"]);
        refArray.Append(lastFloorTopFace.Reference);
        refArray.Append(lastFloorBottomFace.Reference);
        Dimension dimensionRebarSide = Repository.Document.Create.NewDimension(view, dimensionLineLeft, refArray, ViewModel.SelectedDimensionType);
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
    /// Метод по созданию размеров по опалубке пилонов
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размеры</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств пилонов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    private void CreateGeneralViewPylonDimensions(View view, 
                                                   List<Element> hostElems,
                                                   DimensionBaseService dimensionBaseService) {
        Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(hostElems[0] as FamilyInstance,
                                                                   DimensionOffsetType.Left, 1.7);
        foreach(var item in hostElems) {
            if(item is not FamilyInstance hostElem) { return; }

            // Собираем опорные плоскости по опалубке, например:
            // #_1_горизонт_край_низ
            // #_1_горизонт_край_верх
            ReferenceArray refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край"]);
            Dimension dimensionRebarSide = Repository.Document.Create.NewDimension(view, dimensionLineLeft, refArraySide, ViewModel.SelectedDimensionType);
        }
    }


    private void CreateGeneralViewPylonDimensions(View view,
                                                  FamilyInstance skeletonParentRebar, 
                                                  List<Grid> grids,
                                                  DimensionBaseService dimensionBaseService) {
        // Размер по ФРОНТУ опалубка (положение снизу 1)
        Line dimensionLineBottomFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 2);
        ReferenceArray refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#', '/',
                                                                    new List<string>() { "фронт", "край" });
        Dimension dimensionFormworkFront = Repository.Document.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                   refArrayFormworkFront, ViewModel.SelectedDimensionType);

        if(grids.Count > 0) {
            // Размер по ФРОНТУ опалубка + оси (положение снизу 2)
            Line dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1.5);
            ReferenceArray refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, 
                                                                                             new XYZ(0, 0, 1),
                                                                                             refArrayFormworkFront);
            Dimension dimensionFormworkGridFront = Repository.Document.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                           refArrayFormworkGridFront, ViewModel.SelectedDimensionType);
        }
    }


    /// <summary>
    /// Метод по созданию размера по хомутам на основном виде опалубки
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размер</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств хомутов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    private void CreateGeneralViewClampsDimensions(View view,
                                                   List<FamilyInstance> clampsParentRebars,
                                                   DimensionBaseService dimensionBaseService) {
        Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems[0] as FamilyInstance,
                                                                   DimensionOffsetType.Left, 1);

        ReferenceArray refArraySide = new ReferenceArray();

        // Собираем опорные плоскости по опалубке, например:
        // #_1_горизонт_край_низ
        // #_1_горизонт_край_верх
        foreach(var item in SheetInfo.HostElems) {
            if(item is FamilyInstance hostElem) {
                refArraySide = dimensionBaseService.GetDimensionRefs(hostElem, '#', '/', ["горизонт", "край"],
                                                                     refArraySide);
            }
        }

        // Собираем опорные плоскости по арматуре и заполняем список опций изменений сегментов размера
        var dimSegmentOpts = new List<DimensionSegmentOption>();
        foreach(FamilyInstance clampsParentRebar in clampsParentRebars) {
            refArraySide = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/', ["горизонт"], refArraySide);

            // Получаем настройки для изменения сегментов размеров
            dimSegmentOpts = GetClampsDimensionSegmentOptions(view, clampsParentRebar, dimSegmentOpts);
        }

        Dimension dimensionRebarSide = Repository.Document.Create.NewDimension(view, dimensionLineLeft, refArraySide, ViewModel.SelectedDimensionType);

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
    }



    /// <summary>
    /// Данный метод настраивает опции по переопределению сегментов размера на основе параметров семейства армирования
    /// </summary>
    /// <param name="clampsParentRebar">Экземпляр родительского семейства хомутов</param>
    /// <param name="dimSegmentOpts">Список опций, в который можно дописать новые</param>
    /// <returns></returns>
    private List<DimensionSegmentOption> GetClampsDimensionSegmentOptions(View view, FamilyInstance clampsParentRebar, 
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
        var rightDirection = view.RightDirection;
        // В зависимости от направления вида рассчитываем смещения
        if(Math.Abs(view.RightDirection.Y) == 1) {
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
            var skeletonParentRebar = rebarFinder.GetSkeletonParentRebar(view);
            if(skeletonParentRebar is null) {
                return;
            }

            Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            Line dimensionLineBottomEdges = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom, 1.5);
            ReferenceArray refArrayBottomEdges = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            Dimension dimensionBottomEdges = doc.Create.NewDimension(view, dimensionLineBottomEdges, refArrayBottomEdges, ViewModel.SelectedDimensionType);

            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "фронт" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);


            List<Element> plates = rebarFinder.GetSimpleRebars(view, 2001);
            CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, DimensionOffsetType.Left, dimensionBaseService);
        } catch(Exception) { }
    }



    private void CreateGeneralRebarViewPlateDimensions(View view, FamilyInstance skeletonParentRebar, List<Element> platesArray,
                                                       DimensionOffsetType dimensionOffsetType, DimensionBaseService dimensionBaseService) {
        // Определяем наличие в каркасе Г-образных стержней
        bool firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonParentRebar, _hasFirstLRebarParamName) == 1;
        bool secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonParentRebar, _hasSecondLRebarParamName) == 1;

        bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
        bool hasLRebar = firstLRebarParamValue || secondLRebarParamValue;

        ReferenceArray refArraySide;
        if(allRebarAreL) {
            // #1_горизонт_Г-стержень
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "Г-стержень"]);
        } else {
            // #1_горизонт_выпуск
            refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "выпуск"]);
        }

        // #_1_горизонт_край_низ
        refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"], refArraySide);

        Line dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, dimensionOffsetType, 1.3);
        Dimension dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);


        double lengthTemp = 0.0;
        Element neededPlates = default;
        foreach(var plates in platesArray) {
            var length = plates.GetParamValue<double>("мод_Длина");
            if(length > lengthTemp) {
                lengthTemp = length;
                neededPlates = plates;
            }
        }

        var viewOptions = new Options {
            View = view,
            ComputeReferences = true,
            IncludeNonVisibleObjects = false
        };

        var plateRefs = neededPlates.get_Geometry(viewOptions)?
            .OfType<GeometryInstance>()
            .SelectMany(ge => ge.GetSymbolGeometry())
            .OfType<Solid>()
            .Where(solid => solid?.Volume > 0)
            .SelectMany(solid => solid.Faces.OfType<PlanarFace>())
            .Where(face => Math.Abs(face.FaceNormal.Z + 1) < 0.001)
            .ToList();

        foreach(var plateRef in plateRefs) {
            refArraySide.Append(plateRef.Reference);
        }

        Line dimensionLineLeftSecond = dimensionBaseService.GetDimensionLine(skeletonParentRebar, dimensionOffsetType, 0.7);

        Dimension dimensionRebarSideSecond = Repository.Document.Create.NewDimension(view, dimensionLineLeftSecond,
                                                        refArraySide, ViewModel.SelectedDimensionType);
    }




    public void TryCreateGeneralRebarPerpendicularViewDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var skeletonParentRebar = rebarFinder.GetSkeletonParentRebar(view);
            if(skeletonParentRebar is null) {
                return;
            }

            Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Bottom);
            ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "низ", "торец" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DimensionOffsetType.Top);
            ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', new List<string>() { "верх", "торец" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);




            // Определяем наличие в каркасе Г-образных стержней
            bool firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonParentRebar, _hasFirstLRebarParamName) == 1;
            bool secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(skeletonParentRebar, _hasSecondLRebarParamName) == 1;

            bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
            bool hasLRebar = firstLRebarParamValue || secondLRebarParamValue;

            var defaultDimensionOffsetType = DimensionOffsetType.Right;
            // Будем ставить по дефолту справа
            // Слева будем ставить только если есть гэшка (но не все) и она справа

            if(hasLRebar && LRebarIsRight(view, rebarFinder)) {
                defaultDimensionOffsetType = DimensionOffsetType.Left;
            }


            List<Element> plates = rebarFinder.GetSimpleRebars(view, 2001);
            CreateGeneralRebarViewPlateDimensions(view, skeletonParentRebar, plates, defaultDimensionOffsetType, dimensionBaseService);


             


            if(!allRebarAreL && hasLRebar) {
                // #1_горизонт_Г-стержень
                ReferenceArray refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "Г-стержень"]);
                // #_1_горизонт_край_низ
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"], refArraySide);

                defaultDimensionOffsetType = defaultDimensionOffsetType == DimensionOffsetType.Left ? DimensionOffsetType.Right : DimensionOffsetType.Left;

                Line dimensionLineLeftFirst = dimensionBaseService.GetDimensionLine(skeletonParentRebar, defaultDimensionOffsetType, 1.3);
                Dimension dimensionRebarSideFirst = Repository.Document.Create.NewDimension(view, dimensionLineLeftFirst, refArraySide, ViewModel.SelectedDimensionType);
            }


        } catch(Exception) { }
    }


    private bool LRebarIsRight(View view, RebarFinder rebarFinder) {
        // Гэшка
        var lRebar = rebarFinder.GetSimpleRebars(view, 1101).FirstOrDefault();
        // Бутылка
        var bottleRebar = rebarFinder.GetSimpleRebars(view, 1204).FirstOrDefault();

        if(lRebar is null || bottleRebar is null) {
            return false;
        }

        var lRebarLocation = lRebar.Location as LocationPoint;
        var lRebarPt = lRebarLocation.Point;

        var bottleRebarLocation = bottleRebar.Location as LocationPoint;
        var bottleRebarPt = bottleRebarLocation.Point;

        var transform = view.CropBox.Transform;
        var inverseTransform = transform.Inverse;
        // Получаем координаты точек вставки в координатах вида
        var lRebarPtTransformed = inverseTransform.OfPoint(lRebarPt);
        var bottleRebarPtTransformed = inverseTransform.OfPoint(bottleRebarPt);

        return lRebarPtTransformed.X > bottleRebarPtTransformed.X;
    }


    public void TryCreateGeneralRebarPerpendicularViewAdditionalDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.GeneralRebarViewPerpendicular.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var rebar = rebarFinder.GetSkeletonParentRebar(view);
            if(rebar is null) {
                return;
            }

            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, -1);

            ReferenceArray refArrayTop_1 = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', new List<string>() { "1_торец" });
            Dimension dimensionTop_1 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_1, ViewModel.SelectedDimensionType);
            if(dimensionTop_1.Value == 0) {
                doc.Delete(dimensionTop_1.Id);
            }

            ReferenceArray refArrayTop_2 = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', new List<string>() { "2_торец" });
            Dimension dimensionTop_2 = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop_2, ViewModel.SelectedDimensionType);
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
        var view = SheetInfo.TransverseRebarViewFirst.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var rebar = rebarFinder.GetSkeletonParentRebar(view);
            if(rebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineBottom = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0.5);
            ReferenceArray refArrayBottom = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', new List<string>() { "низ", "фронт" });
            Dimension dimensionBottom = doc.Create.NewDimension(view, dimensionLineBottom, refArrayBottom, ViewModel.SelectedDimensionType);

            Line dimensionLineBottomEdge = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 1);
            ReferenceArray refArrayBottomEdge = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', new List<string>() { "низ", "фронт", "край" });
            Dimension dimensionBottomEdge = doc.Create.NewDimension(view, dimensionLineBottomEdge, refArrayBottomEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    public void TryCreateTransverseRebarViewSecondDimensions() {
        var doc = Repository.Document;
        var view = SheetInfo.TransverseRebarViewSecond.ViewElement;
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var rebar = rebarFinder.GetSkeletonParentRebar(view);
            if(rebar is null) {
                return;
            }

            //ВЕРТИКАЛЬНЫЕ РАЗМЕРЫ
            Line dimensionLineTop = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 0.5);
            ReferenceArray refArrayTop = dimensionBaseService.GetDimensionRefs(rebar, '#', '/', new List<string>() { "верх", "фронт" });
            Dimension dimensionTop = doc.Create.NewDimension(view, dimensionLineTop, refArrayTop, ViewModel.SelectedDimensionType);

            Line dimensionLineTopEdge = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 1);
            ReferenceArray refArrayTopEdge = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                              new List<string>() { "верх", "фронт", "край" });
            Dimension dimensionTopEdge = doc.Create.NewDimension(view, dimensionLineTopEdge, refArrayTopEdge, ViewModel.SelectedDimensionType);

            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                                new List<string>() { "низ", "торец", "край" });
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayRebarSide, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }


    private void TryCreateTransverseViewDimensions(View view, bool onTopOfRebar) {
        var doc = Repository.Document;
        string rebarPart = onTopOfRebar ? "верх" : "низ";
        var dimensionBaseService = new DimensionBaseService(view, _paramValueService);

        try {
            var rebarFinder = new RebarFinder(ViewModel, Repository, SheetInfo);
            var rebar = rebarFinder.GetSkeletonParentRebar(view);
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
            ReferenceArray refArrayFormworkFront = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#', '/',
                                                                    new List<string>() { "фронт", "край" });
            Dimension dimensionFormworkFront = doc.Create.NewDimension(view, dimensionLineBottomFirst,
                                                                   refArrayFormworkFront, ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                // Размер по ФРОНТУ опалубка + оси (положение сверху 1)
                Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Top, 0.5);
                ReferenceArray refArrayFormworkGridFront = dimensionBaseService.GetDimensionRefs(grids, view, 
                                                                                                 new XYZ(0, 1, 0),
                                                                                                 refArrayFormworkFront);
                Dimension dimensionFormworkGridFront = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                           refArrayFormworkGridFront, ViewModel.SelectedDimensionType);
            }

            // Определяем наличие в каркасе Г-образных стержней
            bool firstLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasFirstLRebarParamName) == 1;
            bool secondLRebarParamValue = _paramValueService.GetParamValueAnywhere(rebar, _hasSecondLRebarParamName) == 1;

            bool allRebarAreL = firstLRebarParamValue && secondLRebarParamValue;
            bool hasLRebar = firstLRebarParamValue || secondLRebarParamValue;

            if(!(onTopOfRebar && allRebarAreL)) {
                // Размер по ФРОНТУ опалубка + армирование (положение снизу 2)
                Line dimensionLineBottomSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0.5);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                                             new List<string>() { rebarPart, "фронт" },
                                                                         refArrayFormworkFront);
                Dimension dimensionFormworkRebarFrontFirst = doc.Create.NewDimension(view, dimensionLineBottomSecond,
                                                                                  refArrayFormworkRebarFrontSecond, ViewModel.SelectedDimensionType);
            }


            // Размер по ФРОНТУ опалубка + армирование в случае, если есть Г-стержни (положение снизу 0)
            if(onTopOfRebar && hasLRebar) {
                Line dimensionLineTopSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Bottom, 0);
                // Добавляем ссылки на арматурные стержни
                ReferenceArray refArrayFormworkRebarFrontSecond = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                                             new List<string>() { "низ", "фронт" },
                                                                         refArrayFormworkFront);
                Dimension dimensionFormworkRebarFrontSecond = doc.Create.NewDimension(view, dimensionLineTopSecond,
                                                                                  refArrayFormworkRebarFrontSecond, ViewModel.SelectedDimensionType);
            }


            //ГОРИЗОНТАЛЬНЫЕ РАЗМЕРЫ
            // Размер по ТОРЦУ опалубка (положение справа 1)
            Line dimensionLineRightFirst = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 1);
            ReferenceArray refArrayFormworkSide = dimensionBaseService.GetDimensionRefs(SheetInfo.HostElems[0] as FamilyInstance, '#', '/',
                                                                   new List<string>() { "торец", "край" });
            Dimension dimensionFormworkSide = doc.Create.NewDimension(view, dimensionLineRightFirst,
                                                                  refArrayFormworkSide, ViewModel.SelectedDimensionType);


            // Размер по ТОРЦУ опалубка + армирование (положение справа 2)
            Line dimensionLineRightSecond = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Right, 0.5);
            // Добавляем ссылки на арматурные стержни
            ReferenceArray refArrayFormworkRebarSide = dimensionBaseService.GetDimensionRefs(rebar, '#', '/',
                                                                        new List<string>() { rebarPart, "торец" },
                                                                    refArrayFormworkSide);
            Dimension dimensionFormworkRebarSide = doc.Create.NewDimension(view, dimensionLineRightSecond,
                                                                              refArrayFormworkRebarSide, ViewModel.SelectedDimensionType);

            if(grids.Count > 0) {
                // Размер по ТОРЦУ опалубка + оси (положение слева 1)
                Line dimensionLineLeft = dimensionBaseService.GetDimensionLine(rebar, DimensionOffsetType.Left, 1.2);
                ReferenceArray refArrayFormworkGridSide = dimensionBaseService.GetDimensionRefs(grids, view, 
                                                                                                new XYZ(1, 0, 0),
                                                                                                refArrayFormworkSide);
                Dimension dimensionFormworkGridSide = doc.Create.NewDimension(view, dimensionLineLeft,
                                                                          refArrayFormworkGridSide, ViewModel.SelectedDimensionType);

                // Корректируем концы осей, приближая их на виде к опалубке пилона, чтобы сократить габариты
                // видового экрана
                var transverseViewGridOffsets = new OffsetOption() {
                    LeftOffset = 1.5,
                    RightOffset = 0.5,
                    TopOffset = 0.6,
                    BottomOffset = 1.1
                };
                EditGridEnds(view, SheetInfo.HostElems.First() as FamilyInstance, grids, transverseViewGridOffsets, dimensionBaseService);
            }
        } catch(Exception) { }
    }


    private void EditGridEnds(View view, FamilyInstance familyInstance, 
                              List<Grid> grids, OffsetOption offsetOption, DimensionBaseService dimensionBaseService) {
        var rightDirection = view.RightDirection;

        foreach(var grid in grids) {
            var gridLine = grid.Curve as Line;
            var gridDir = gridLine.Direction;

            if(rightDirection.IsAlmostEqualTo(gridDir)
                || rightDirection.IsAlmostEqualTo(gridDir.Negate())) {

                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                Line offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Left,
                                                                         offsetOption.LeftOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                Line offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Right,
                                                                         offsetOption.RightOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);

            } else {
                var curve = grid.GetCurvesInView(DatumExtentType.ViewSpecific, view).First();

                Line offsetLine1 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Bottom,
                                                                         offsetOption.BottomOffset, false);
                var pt1 = curve.Project(offsetLine1.Origin).XYZPoint;

                Line offsetLine2 = dimensionBaseService.GetDimensionLine(familyInstance,
                                                                         DimensionOffsetType.Top,
                                                                         offsetOption.TopOffset, false);
                var pt2 = curve.Project(offsetLine2.Origin).XYZPoint;

                var newLine = Line.CreateBound(pt1, pt2);
                grid.SetCurveInView(DatumExtentType.ViewSpecific, view, newLine);
            }
        }
    }
}
