using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView;
internal class PylonSheetInfoManager {
    public PylonSheetInfoManager(MainViewModel mvm, RevitRepository repository, PylonSheetInfo pylonSheetInfo) {
        ViewModel = mvm;
        Repository = repository;
        SheetInfo = pylonSheetInfo;
    }

    internal MainViewModel ViewModel { get; set; }
    internal RevitRepository Repository { get; set; }
    internal PylonSheetInfo SheetInfo { get; set; }

    public void WorkWithCreation() {
        var selectionSettings = ViewModel.SelectionSettings;

        // Если текущий PylonSheetInfo не выбран для работы - continue
        if(!SheetInfo.IsCheck) { return; } else {
            SheetInfo.GetViewNamesForWork();
        }

        // Если листы были в проекте (когда плагин запускают для создания/размещения видов), то мы об этом знаем из RevitRepository
        if(SheetInfo.PylonViewSheet is null) {
            SheetInfo.CreateSheet();
        } else {
            SheetInfo.FindTitleBlock();
            SheetInfo.GetTitleBlockSize();
            SheetInfo.FindViewsNViewportsOnSheet();
            SheetInfo.FindSchedulesNViewportsOnSheet();
            SheetInfo.FindNoteLegendOnSheet();
            SheetInfo.FindRebarLegendNodeOnSheet();
        }

        // Если вдруг по какой-то причине лист не был создан, то создание видов/видовых экранов не выполняем 
        if(SheetInfo.PylonViewSheet is null) { return; }

        // ОСНОВНОЙ ВИД
        if(selectionSettings.NeedWorkWithGeneralView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralView.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.GeneralView.ViewSectionCreator.TryCreateGeneralView(ViewModel.SelectedViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralView);
                }
            }
            // Тут точно получили вид
        }


        // ОСНОВНОЙ АРМАТУРНЫЙ ВИД
        if(selectionSettings.NeedWorkWithGeneralRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.GeneralViewRebar.ViewSectionCreator.TryCreateGeneralRebarView(ViewModel.SelectedViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewRebar);
                }
            }
            // Тут точно получили вид
        }



        // ОСНОВНОЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД 
        if(selectionSettings.NeedWorkWithGeneralPerpendicularView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicular.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.GeneralViewPerpendicular.ViewSectionCreator.TryCreateGeneralPerpendicularView(ViewModel.SelectedViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewPerpendicular);
                }
            }
            // Тут точно получили вид
        }


        // ОСНОВНОЙ АРМАТУРНЫЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД 
        if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.GeneralViewPerpendicularRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.GeneralViewPerpendicularRebar.ViewSectionCreator.TryCreateGeneralRebarPerpendicularView(ViewModel.SelectedViewFamilyType)) {
                    Repository.FindViewSectionInPj(SheetInfo.GeneralViewPerpendicularRebar);
                }
            }
            // Тут точно получили вид
        }

        // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirst.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.TransverseViewFirst.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 1)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewFirst);
                }
            }
            // Тут точно получили вид
        }

        // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecond.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.TransverseViewSecond.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 2)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewSecond);
                }
            }
            // Тут точно получили вид
        }

        // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД
        if(selectionSettings.NeedWorkWithTransverseViewThird) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewThird.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.TransverseViewThird.ViewSectionCreator.TryCreateTransverseView(ViewModel.SelectedViewFamilyType, 3)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewThird);
                }
            }
            // Тут точно получили вид
        }

        // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewFirstRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.TransverseViewFirstRebar.ViewSectionCreator.TryCreateTransverseRebarView(ViewModel.SelectedViewFamilyType, 1)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewFirstRebar);
                }
            }
            // Тут точно получили вид
        }

        // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
        if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.TransverseViewSecondRebar.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.TransverseViewSecondRebar.ViewSectionCreator.TryCreateTransverseRebarView(ViewModel.SelectedViewFamilyType, 2)) {
                    Repository.FindViewSectionInPj(SheetInfo.TransverseViewSecondRebar);
                }
            }
            // Тут точно получили вид
        }

        // СПЕЦИФИКАЦИЯ АРМАТУРЫ
        if(selectionSettings.NeedWorkWithRebarSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.RebarSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                if(!SheetInfo.RebarSchedule.ViewScheduleCreator.TryCreateRebarSchedule()) {
                    Repository.FindViewScheduleInPj(SheetInfo.RebarSchedule);
                }
            }
            // Тут точно получили вид
        }

        // СПЕЦИФИКАЦИЯ КАРКАСОВ
        if(selectionSettings.NeedWorkWithSkeletonSchedule) {
            // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
            if(SheetInfo.SkeletonSchedule.ViewElement is null) {
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
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
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
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
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
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
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
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
                // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
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
            if(SheetInfo.GeneralView.ViewElement != null) {
                SheetInfo.GeneralView.DimensionCreator.TryCreateViewDimensions();
                SheetInfo.GeneralView.MarkCreator.TryCreateViewMarks();
            }
        }

        //// СОЗДАНИЕ АННОТАЦИЙ ПЕРВОГО ПОПЕРЕЧНОГО ВИДА
        //if(selectionSettings.NeedWorkWithTransverseViewFirst) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.TransverseViewFirst.ViewElement != null) {
        //        SheetInfo.TransverseViewFirst.ViewDimensionCreator
        //            .TryCreateTransverseViewFirstDimensions();
        //        SheetInfo.TransverseViewFirst.ViewMarkCreator
        //            .TryCreateTransverseViewMarks();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ВТОРОГО ПОПЕРЕЧНОГО ВИДА
        //if(selectionSettings.NeedWorkWithTransverseViewSecond) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.TransverseViewSecond.ViewElement != null) {
        //        SheetInfo.TransverseViewSecond.ViewDimensionCreator
        //            .TryCreateTransverseViewSecondDimensions();
        //        SheetInfo.TransverseViewSecond.ViewMarkCreator
        //            .TryCreateTransverseViewMarks();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ТРЕТЬЕГО ПОПЕРЕЧНОГО ВИДА
        //if(selectionSettings.NeedWorkWithTransverseViewThird) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.TransverseViewThird.ViewElement != null) {
        //        SheetInfo.TransverseViewThird.ViewDimensionCreator
        //            .TryCreateTransverseViewThirdDimensions();
        //        SheetInfo.TransverseViewThird.ViewMarkCreator
        //            .TryCreateTransverseViewMarks();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО АРМАТУРНОГО ВИДА
        //if(selectionSettings.NeedWorkWithGeneralRebarView) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.GeneralRebarView.ViewElement != null) {
        //        SheetInfo.GeneralRebarView.ViewDimensionCreator
        //            .TryCreateGeneralRebarViewDimensions();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ОСНОВНОГО ПЕРПЕНДИКУЛЯРНОГО АРМАТУРНОГО ВИДА
        //if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.GeneralRebarViewPerpendicular.ViewElement != null) {
        //        SheetInfo.GeneralRebarViewPerpendicular.ViewDimensionCreator
        //            .TryCreateGeneralRebarPerpendicularViewDimensions();
        //        SheetInfo.GeneralRebarViewPerpendicular.ViewDimensionCreator
        //            .TryCreateGeneralRebarPerpendicularViewAdditionalDimensions();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ПЕРВОГО ПОПЕРЕЧНОГО ВИДА АРМИРОВАНИЯ
        //if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.TransverseRebarViewFirst.ViewElement != null) {
        //        SheetInfo.TransverseRebarViewFirst.ViewDimensionCreator
        //            .TryCreateTransverseRebarViewFirstDimensions();
        //        SheetInfo.TransverseRebarViewFirst.ViewMarkCreator
        //            .TryCreateTransverseRebarViewMarks();
        //    }
        //}

        //// СОЗДАНИЕ АННОТАЦИЙ ВТОРОГО ПОПЕРЕЧНОГО ВИДА АРМИРОВАНИЯ
        //if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
        //    // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
        //    if(SheetInfo.TransverseRebarViewSecond.ViewElement != null) {
        //        SheetInfo.TransverseRebarViewSecond.ViewDimensionCreator
        //            .TryCreateTransverseRebarViewSecondDimensions();
        //        SheetInfo.TransverseRebarViewSecond.ViewMarkCreator
        //            .TryCreateTransverseRebarViewMarks();
        //    }
        //}



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
        if(selectionSettings.NeedWorkWithTransverseViewThird) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewThird.ViewportElement is null) {
                SheetInfo.TransverseViewThird.ViewSectionPlacer.PlaceTransverseThirdViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseViewSecond) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewSecond.ViewportElement is null) {
                SheetInfo.TransverseViewSecond.ViewSectionPlacer.PlaceTransverseSecondViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseViewFirst) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewFirst.ViewportElement is null) {
                SheetInfo.TransverseViewFirst.ViewSectionPlacer.PlaceTransverseFirstViewPort();
            }
        }

        // Размещение видов арматурного каркаса
        if(selectionSettings.NeedWorkWithGeneralRebarView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralViewRebar.ViewportElement is null) {
                SheetInfo.GeneralViewRebar.ViewSectionPlacer.PlaceGeneralRebarViewport();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewSecondRebar.ViewportElement is null) {
                SheetInfo.TransverseViewSecondRebar.ViewSectionPlacer.PlaceTransverseRebarSecondViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.TransverseViewFirstRebar.ViewportElement is null) {
                SheetInfo.TransverseViewFirstRebar.ViewSectionPlacer.PlaceTransverseRebarFirstViewPort();
            }
        }
        if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.GeneralViewPerpendicularRebar.ViewportElement is null) {
                SheetInfo.GeneralViewPerpendicularRebar.ViewSectionPlacer.PlaceGeneralPerpendicularRebarViewport();
            }
        }

        // Размещение спецификаций
        if(selectionSettings.NeedWorkWithSkeletonSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.SkeletonSchedule.ViewportElement is null) {
                SheetInfo.SkeletonSchedule.ViewSchedulePlacer.PlaceSkeletonSchedule();
            }
        }
        if(selectionSettings.NeedWorkWithRebarSchedule) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.RebarSchedule.ViewportElement is null) {
                SheetInfo.RebarSchedule.ViewSchedulePlacer.PlaceRebarSchedule();
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
        if(selectionSettings.NeedWorkWithRebarNode) {
            // Если видовой экран на листе не найден, то размещаем
            if(SheetInfo.RebarNodeView.ViewportElement is null) {
                SheetInfo.RebarNodeView.LegendPlacer.PlaceRebarNodeLegend();
            }
        }
    }
}
