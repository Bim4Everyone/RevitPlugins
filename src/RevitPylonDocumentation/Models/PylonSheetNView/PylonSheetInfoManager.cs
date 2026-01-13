namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonSheetInfoManager {
    public PylonSheetInfoManager(CreationSettings settings, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        Settings = settings;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal CreationSettings Settings { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }

    public void WorkWithCreation() {
        var selectionSettings = Settings.SelectionSettings;

        // Если листы были в проекте (когда плагин запускают для создания/размещения видов),
        // то мы об этом знаем из RevitRepository
        if(SheetInfo.PylonViewSheet is null) {
            SheetInfo.CreateSheet();
        } else {
            SheetInfo.FindTitleBlock();
            SheetInfo.GetTitleBlockSize();
            SheetInfo.FindViewsNViewportsOnSheet();
            SheetInfo.FindSchedulesNViewportsOnSheet();
            SheetInfo.FindNoteLegendOnSheet();
        }

        // Если вдруг по какой-то причине лист не был создан, то создание видов/видовых экранов не выполняем 
        if(SheetInfo.PylonViewSheet is null) { return; }

        // Получаем суммарный BoundingBox по всем элементам, принадлежащим пилону
        // (нужен для формирования рамки подрезки видов)
        SheetInfo.ElemsInfo.FindPylonHostVectors();
        SheetInfo.ElemsInfo.FindPylonHostOrigin();
        SheetInfo.ElemsInfo.FindHostDimensions();
        SheetInfo.ElemsInfo.FindHostMaxMinByZ();
        SheetInfo.ElemsInfo.FindElemsBoundingBox();
        SheetInfo.ElemsInfo.FindElemsBoundingBoxProps();

        SheetInfo.RebarInfo.TrySetSimpleRebarMarks();

        // ОСНОВНОЙ ВИД
        if(selectionSettings.NeedWorkWithGeneralView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralView.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.GeneralView.ViewSectionCreator.TryCreateGeneralView(
                        Settings.VerticalViewSettings.SelectedGeneralViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralView);
                }
            }
            // Тут точно получили вид
        }


        // ОСНОВНОЙ АРМАТУРНЫЙ ВИД
        if(selectionSettings.NeedWorkWithGeneralRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.GeneralViewRebar.ViewSectionCreator.TryCreateGeneralRebarView(
                        Settings.VerticalViewSettings.SelectedGeneralViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewRebar);
                }
            }
            // Тут точно получили вид
        }



        // ОСНОВНОЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД 
        if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicular.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.GeneralViewPerpendicular.ViewSectionCreator.TryCreateGeneralPerpendicularView(
                        Settings.VerticalViewSettings.SelectedGeneralViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewPerpendicular);
                }
            }
            // Тут точно получили вид
        }


        // ОСНОВНОЙ АРМАТУРНЫЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД 
        if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicularRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.GeneralViewPerpendicularRebar.ViewSectionCreator.TryCreateGeneralRebarPerpendicularView(
                        Settings.VerticalViewSettings.SelectedGeneralViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewPerpendicularRebar);
                }
            }
            // Тут точно получили вид
        }

        // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirst.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewFirst.ViewSectionCreator.TryCreateTransverseView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        1)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewFirst);
                }
            }
            // Тут точно получили вид
        }

        // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecond.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewSecond.ViewSectionCreator.TryCreateTransverseView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        2)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewSecond);
                }
            }
            // Тут точно получили вид
        }

        // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewThird) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewThird.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewThird.ViewSectionCreator.TryCreateTransverseView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        3)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewThird);
                }
            }
            // Тут точно получили вид
        }

        // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirstRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewFirstRebar.ViewSectionCreator.TryCreateTransverseRebarView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        1)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewFirstRebar);
                }
            }
            // Тут точно получили вид
        }

        // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecondRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewSecondRebar.ViewSectionCreator.TryCreateTransverseRebarView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        2)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewSecondRebar);
                }
            }
            // Тут точно получили вид
        }

        // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewThird) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewThirdRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.TransverseViewThirdRebar.ViewSectionCreator.TryCreateTransverseRebarView(
                        Settings.HorizontalViewSettings.SelectedTransverseViewFamilyType,
                        3)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewThirdRebar);
                }
            }
            // Тут точно получили вид
        }

        // СПЕЦИФИКАЦИЯ КАРКАСОВ
        if(selectionSettings.NeedWorkWithSkeletonSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.SkeletonSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.SkeletonSchedule.ViewScheduleCreator.TryCreateSkeletonSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.SkeletonSchedule);
                }
            }
            // Тут точно получили вид
        }

        // СПЕЦИФИКАЦИЯ ЭЛЕМЕНТОВ КАРКАСОВ
        if(selectionSettings.NeedWorkWithSkeletonByElemsSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.SkeletonByElemsSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.SkeletonByElemsSchedule.ViewScheduleCreator.TryCreateSkeletonByElemsSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.SkeletonByElemsSchedule);
                }
            }
            // Тут точно получили вид
        }

        // СПЕЦИФИКАЦИЯ МАТЕРИАЛОВ
        if(selectionSettings.NeedWorkWithMaterialSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.MaterialSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.MaterialSchedule.ViewScheduleCreator.TryCreateMaterialSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.MaterialSchedule);
                }
            }
            // Тут точно получили вид
        }

        // ВЕДОМОСТЬ СИСТЕМНЫХ ДЕТАЛЕЙ
        if(selectionSettings.NeedWorkWithSystemPartsSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.SystemPartsSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.SystemPartsSchedule.ViewScheduleCreator.TryCreateSystemPartsSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.SystemPartsSchedule);
                }
            }
            // Тут точно получили вид
        }

        // ВЕДОМОСТЬ IFC ДЕТАЛЕЙ
        if(selectionSettings.NeedWorkWithIfcPartsSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.IfcPartsSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно -
                // будем искать в проекте
                if(!SheetInfo.IfcPartsSchedule.ViewScheduleCreator.TryCreateIfcPartsSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.IfcPartsSchedule);
                }
            }
            // Тут точно получили вид

        }



        // Принудительно регеним документ, иначе запрашиваемые габариты видовых экранов будут некорректны
        Repository.Document.Regenerate();



        // СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО ВИДА
        if(selectionSettings.NeedWorkWithGeneralView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralView.ViewportElement is null
                    && SheetInfo.GeneralView.ViewElement != null) {
                SheetInfo.GeneralView.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО ПЕРПЕНДИКУЛЯРНОГО ВИДА
        if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement is null
                    && SheetInfo.GeneralViewPerpendicular.ViewElement != null) {
                SheetInfo.GeneralViewPerpendicular.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ПЕРВОГО ПОПЕРЕЧНОГО ВИДА
        if(selectionSettings.NeedWorkWithTransverseViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirst.ViewportElement is null
                    && SheetInfo.TransverseViewFirst.ViewElement != null) {
                SheetInfo.TransverseViewFirst.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ВТОРОГО ПОПЕРЕЧНОГО ВИДА
        if(selectionSettings.NeedWorkWithTransverseViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecond.ViewportElement is null
                    && SheetInfo.TransverseViewSecond.ViewElement != null) {
                SheetInfo.TransverseViewSecond.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ТРЕТЬЕГО ПОПЕРЕЧНОГО ВИДА
        if(selectionSettings.NeedWorkWithTransverseViewThird) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewThird.ViewportElement is null
                    && SheetInfo.TransverseViewThird.ViewElement != null) {
                SheetInfo.TransverseViewThird.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО АРМАТУРНОГО ВИДА
        if(selectionSettings.NeedWorkWithGeneralRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewRebar.ViewportElement is null
                    && SheetInfo.GeneralViewRebar.ViewElement != null) {
                SheetInfo.GeneralViewRebar.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО ПЕРПЕНДИКУЛЯРНОГО АРМАТУРНОГО ВИДА
        if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicularRebar.ViewportElement is null
                    && SheetInfo.GeneralViewPerpendicularRebar.ViewElement != null) {
                SheetInfo.GeneralViewPerpendicularRebar.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ПЕРВОГО ПОПЕРЕЧНОГО ВИДА АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirstRebar.ViewportElement is null
                    && SheetInfo.TransverseViewFirstRebar.ViewElement != null) {
                SheetInfo.TransverseViewFirstRebar.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ВТОРОГО ПОПЕРЕЧНОГО ВИДА АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecondRebar.ViewportElement is null
                    && SheetInfo.TransverseViewSecondRebar.ViewElement != null) {
                SheetInfo.TransverseViewSecondRebar.AnnotationCreator.TryCreateViewAnnotations();
            }
        }

        // СОЗДАНИЕ АННОТАЦИЙ ТРЕТЬЕГО ПОПЕРЕЧНОГО ВИДА АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewThird) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewThirdRebar.ViewportElement is null
                    && SheetInfo.TransverseViewThirdRebar.ViewElement != null) {
                SheetInfo.TransverseViewThirdRebar.AnnotationCreator.TryCreateViewAnnotations();
            }
        }


        // Размещение видов арматурного каркаса
        if(selectionSettings.NeedWorkWithGeneralRebarView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralViewRebar.ViewportElement is null) {
                SheetInfo.GeneralViewRebar.ViewSectionPlacer.PlaceGeneralRebarViewport();
            }
        }
        if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralViewPerpendicularRebar.ViewportElement is null) {
                SheetInfo.GeneralViewPerpendicularRebar.ViewSectionPlacer.PlaceGeneralPerpendicularRebarViewport();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewFirstRebar.ViewportElement is null) {
                SheetInfo.TransverseViewFirstRebar.ViewSectionPlacer.PlaceTransverseRebarFirstViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewSecondRebar.ViewportElement is null) {
                SheetInfo.TransverseViewSecondRebar.ViewSectionPlacer.PlaceTransverseRebarSecondViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseRebarViewThird) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewThirdRebar.ViewportElement is null) {
                SheetInfo.TransverseViewThirdRebar.ViewSectionPlacer.PlaceTransverseRebarThirdViewPort();
            }
        }


        // Размещение видов опалубки
        if(selectionSettings.NeedWorkWithGeneralView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralView.ViewportElement is null) {
                SheetInfo.GeneralView.ViewSectionPlacer.PlaceGeneralViewport();
            }
        }
        if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralViewPerpendicular.ViewportElement is null) {
                SheetInfo.GeneralViewPerpendicular.ViewSectionPlacer.PlaceGeneralPerpendicularViewport();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseViewFirst) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewFirst.ViewportElement is null) {
                SheetInfo.TransverseViewFirst.ViewSectionPlacer.PlaceTransverseFirstViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseViewSecond) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewSecond.ViewportElement is null) {
                SheetInfo.TransverseViewSecond.ViewSectionPlacer.PlaceTransverseSecondViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseViewThird) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewThird.ViewportElement is null) {
                SheetInfo.TransverseViewThird.ViewSectionPlacer.PlaceTransverseThirdViewPort();
            }
        }


        // Размещение спецификаций
        if(selectionSettings.NeedWorkWithSkeletonSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.SkeletonSchedule.ViewportElement is null) {
                SheetInfo.SkeletonSchedule.ViewSchedulePlacer.PlaceSkeletonSchedule();
            }
        }
        if(selectionSettings.NeedWorkWithMaterialSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.MaterialSchedule.ViewportElement is null) {
                SheetInfo.MaterialSchedule.ViewSchedulePlacer.PlaceMaterialSchedule();
            }
        }
        if(selectionSettings.NeedWorkWithSkeletonByElemsSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.SkeletonByElemsSchedule.ViewportElement is null) {
                SheetInfo.SkeletonByElemsSchedule.ViewSchedulePlacer.PlaceSkeletonByElemsSchedule();
            }
        }
        if(selectionSettings.NeedWorkWithSystemPartsSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.SystemPartsSchedule.ViewportElement is null) {
                SheetInfo.SystemPartsSchedule.ViewSchedulePlacer.PlaceSystemPartsSchedule();
            }
        }
        if(selectionSettings.NeedWorkWithIfcPartsSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.IfcPartsSchedule.ViewportElement is null) {
                SheetInfo.IfcPartsSchedule.ViewSchedulePlacer.PlaceIfcPartsSchedule();
            }
        }

        // Размещение примечания и узла армирования
        if(selectionSettings.NeedWorkWithLegend) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.LegendView.ViewportElement is null) {
                SheetInfo.LegendView.LegendPlacer.PlaceNoteLegend();
            }
        }
    }
}
