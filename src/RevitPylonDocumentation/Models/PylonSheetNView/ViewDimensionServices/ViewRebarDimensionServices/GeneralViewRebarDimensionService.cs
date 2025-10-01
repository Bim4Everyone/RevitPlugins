using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitPylonDocumentation.Models.PluginOptions;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionServices.ViewRebarDimensionServices;
internal class GeneralViewRebarDimensionService {
    private readonly DimensionSegmentsService _dimensionSegmentsService;
    
    internal GeneralViewRebarDimensionService(MainViewModel mvm, RevitRepository repository, 
                                              PylonSheetInfo pylonSheetInfo, PylonView pylonView) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
        ViewOfPylon = pylonView;

        _dimensionSegmentsService = new DimensionSegmentsService(pylonView.ViewElement);
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }
    internal PylonView ViewOfPylon { get; set; }


    /// <summary>
    /// Создание размерной цепочки по всем вертикальным стержням арматурного каркаса сверху
    /// </summary>
    internal void TryCreateAllTopRebarDimensions(FamilyInstance skeletonParentRebar,
                                                 DimensionBaseService dimensionBaseService) {
        try {
            // Если все стержни Г-образные,тогда нет смысла ставить этот размер
            if(SheetInfo.RebarInfo.AllRebarAreL) { return; }
            var dimensionLineTop = dimensionBaseService.GetDimensionLine(skeletonParentRebar, DirectionType.Top, 0.5);
            var refArrayTop = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["верх", "фронт"]);
            var dimension = Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineTop, refArrayTop,
                                                    ViewModel.SelectedDimensionType);
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размерной цепочки по всем вертикальным стержням арматурного каркаса снизу
    /// </summary>
    internal void TryCreateAllBottomRebarDimensions(FamilyInstance skeletonParentRebar, 
                                                    DimensionBaseService dimensionBaseService) {
        try {
            var dimensionLineBottom = dimensionBaseService.GetDimensionLine(skeletonParentRebar, 
                                                                            DirectionType.Bottom, 0.6);
            var refArrayBottom = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["низ", "фронт"]);
            var dimension = Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineBottom, 
                                                                    refArrayBottom, ViewModel.SelectedDimensionType);
            dimension.SetParamValue(BuiltInParameter.DIM_DISPLAY_EQ, 2);
        } catch(Exception) { }
    }

    /// <summary>
    /// Создание размерной цепочки по крайним вертикальным стержням арматурного каркаса снизу
    /// </summary>
    internal void TryCreateEdgeRebarDimensions(FamilyInstance skeletonParentRebar, 
                                               DimensionBaseService dimensionBaseService) {
        try {
            var dimensionLineBottomEdges = dimensionBaseService.GetDimensionLine(skeletonParentRebar,
                                                                     DirectionType.Bottom, 1.1);
            var refArrayBottomEdges = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/',
                                                                            ["низ", "фронт", "край"]);
            Repository.Document.Create.NewDimension(ViewOfPylon.ViewElement, dimensionLineBottomEdges,
                                                    refArrayBottomEdges, ViewModel.SelectedDimensionType);
        } catch(Exception) { }
    }

    /// <summary>
    /// Метод по созданию размера по хомутам на основном виде опалубки
    /// </summary>
    /// <param name="view">Вид, на котором нужно создать размер</param>
    /// <param name="clampsParentRebars">Список экземпляров семейств хомутов</param>
    /// <param name="dimensionBaseService">Сервис по анализу основ размеров</param>
    internal void TryCreateClampsDimensions(List<FamilyInstance> clampsParentRebars, FamilyInstance skeletonParentRebar,
                                            DimensionBaseService dimensionBaseService) {
        try {
            // Собираем опорные плоскости по арматуре и заполняем список опций изменений сегментов размера
            // Добавляем нижнюю горизонтальную опорную плоскость от верт стержней "#_1_горизонт_край_низ"
            var refArraySide = 
                dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', ["горизонт", "край", "низ"]);
            // Создаем коллекцию опций изменений будущего размера и добавляем запись про "#_1_горизонт_край_низ"
            var dimSegmentOpts = new List<DimensionSegmentOption> {
                new DimensionSegmentOption(true, "", _dimensionSegmentsService.HorizSmallUpDirectDimTextOffset)
            };
            foreach(var clampsParentRebar in clampsParentRebars) {
                refArraySide = dimensionBaseService.GetDimensionRefs(clampsParentRebar, '#', '/', ["горизонт"],
                                                                     oldRefArray: refArraySide);
                // Получаем настройки для изменения сегментов размеров
                dimSegmentOpts = GetClampsDimensionSegmentOptions(clampsParentRebar, dimSegmentOpts);

                // У промежуточного сегмента между массивами хомутов не будет изменений
                dimSegmentOpts.Add(new DimensionSegmentOption(false));
            }

            if(SheetInfo.RebarInfo.AllRebarAreL) {
                // Дополняем плоскостью на Гэшках вертикальных стержней "#1_горизонт_Г-стержень"
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                     ["горизонт", "Г-стержень"],
                                                                     oldRefArray: refArraySide);
            } else {
                // Дополняем плоскостью на выпусках от вертикальных стержней "#1_горизонт_выпуск"
                refArraySide = dimensionBaseService.GetDimensionRefs(skeletonParentRebar, '#', '/', 
                                                                     ["горизонт", "выпуск"],
                                                                     oldRefArray: refArraySide);
            }
            dimSegmentOpts.Add(new DimensionSegmentOption(false));

            var dimensionLineLeft = dimensionBaseService.GetDimensionLine(SheetInfo.HostElems.First() as FamilyInstance,
                                                                          DirectionType.Left, 1.1);
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
        //// - первый сегмент: префикс не пишем
        // - второй сегмент: вписать префикс и изменить положение текста,
        //      если "мод_ФОП_Доборный 1" == true и "мод_ФОП_Доборный 1_Количество" > 1
        // - третий сегмент: никак не меняем
        // - четвертый сегмент: всегда вписывать префикс, но не менять положение текста
        // - пятый сегмент: никак не меняем
        // - шестой сегмент: вписать префикс и изменить положение текста,
        //      если "мод_ФОП_Доборный 2" == true и "мод_ФОП_Доборный 2_Количество" > 1
        //// - седьмой сегмент: никак не меняем
        // - восьмой сегмент: никак не меняем

        // Формируем опцию смещения сегментов размера для семейства хомутов
        if(additional1 == 1) {
            if(additional1Count > 1) {
                dimSegmentOpts.Add(new DimensionSegmentOption(true,
                                                              $"{additional1Count - 1}х{additional1Step}=",
                                                              _dimensionSegmentsService.HorizBigUpDirectDimTextOffset));
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
                                                              _dimensionSegmentsService.HorizBigUpInvertedDimTextOffset));
            }
        }
        return dimSegmentOpts;
    }
}
