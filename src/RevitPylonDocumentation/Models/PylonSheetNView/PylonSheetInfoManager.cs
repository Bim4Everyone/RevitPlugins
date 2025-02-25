using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.PylonSheetNView {
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
            UserSelectionSettings selectionSettings = ViewModel.SelectionSettings;

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
                if(SheetInfo.GeneralRebarView.ViewElement is null) {
                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.GeneralRebarView.ViewSectionCreator.TryCreateGeneralRebarView(ViewModel.SelectedViewFamilyType)) {
                        Repository.FindViewSectionInPj(SheetInfo.GeneralRebarView);
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
                if(SheetInfo.GeneralRebarViewPerpendicular.ViewElement is null) {
                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.GeneralRebarViewPerpendicular.ViewSectionCreator.TryCreateGeneralRebarPerpendicularView(ViewModel.SelectedViewFamilyType)) {
                        Repository.FindViewSectionInPj(SheetInfo.GeneralRebarViewPerpendicular);
                    }
                }
                // Тут точно получили вид
            }


            // ОБРАЗМЕРИВАНИЕ ОСНОВНОГО АРМАТУРНОГО ПЕРПЕНДИКУЛЯРНОГО ВИДА
            if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.GeneralRebarViewPerpendicular.ViewElement != null) {
                    SheetInfo.GeneralRebarViewPerpendicular.ViewDimensionCreator
                        .TryCreateGeneralRebarPerpendicularViewDimensions();
                }
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
                if(SheetInfo.TransverseRebarViewFirst.ViewElement is null) {
                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.TransverseRebarViewFirst.ViewSectionCreator.TryCreateTransverseRebarView(ViewModel.SelectedViewFamilyType, 1)) {
                        Repository.FindViewSectionInPj(SheetInfo.TransverseRebarViewFirst);
                    }
                }
                // Тут точно получили вид
            }

            // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД АРМИРОВАНИЯ
            if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.TransverseRebarViewSecond.ViewElement is null) {
                    // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                    if(!SheetInfo.TransverseRebarViewSecond.ViewSectionCreator.TryCreateTransverseRebarView(ViewModel.SelectedViewFamilyType, 2)) {
                        Repository.FindViewSectionInPj(SheetInfo.TransverseRebarViewSecond);
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
                if(SheetInfo.GeneralRebarView.ViewportElement is null) {
                    SheetInfo.GeneralRebarView.ViewSectionPlacer.PlaceGeneralRebarViewport();
                }
            }
            if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.TransverseRebarViewSecond.ViewportElement is null) {
                    SheetInfo.TransverseRebarViewSecond.ViewSectionPlacer.PlaceTransverseRebarSecondViewPort();
                }
            }
            if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.TransverseRebarViewFirst.ViewportElement is null) {
                    SheetInfo.TransverseRebarViewFirst.ViewSectionPlacer.PlaceTransverseRebarFirstViewPort();
                }
            }
            if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
                // Если видовой экран на листе не найден, то размещаем
                if(SheetInfo.GeneralRebarViewPerpendicular.ViewportElement is null) {
                    SheetInfo.GeneralRebarViewPerpendicular.ViewSectionPlacer.PlaceGeneralPerpendicularRebarViewport();
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



            // ОБРАЗМЕРИВАНИЕ ОСНОВНОГО АРМАТУРНОГО ВИДА
            if(selectionSettings.NeedWorkWithGeneralRebarView) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.GeneralRebarView.ViewElement != null) {
                    SheetInfo.GeneralRebarView.ViewDimensionCreator
                        .TryCreateGeneralRebarViewDimensions();
                }
            }

            // ОБРАЗМЕРИВАНИЕ ОСНОВНОГО АРМАТУРНОГО ВИДА
            if(selectionSettings.NeedWorkWithGeneralPerpendicularRebarView) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.GeneralRebarViewPerpendicular.ViewElement != null) {
                    SheetInfo.GeneralRebarViewPerpendicular.ViewDimensionCreator
                        .TryCreateGeneralRebarPerpendicularViewDimensions();
                    SheetInfo.GeneralRebarViewPerpendicular.ViewDimensionCreator
                        .TryCreateGeneralRebarPerpendicularViewAdditionalDimensions();
                }
            }

            // ОБРАЗМЕРИВАНИЕ ПЕРВОГО ПОПЕРЕЧНОГО ВИД АРМИРОВАНИЯ
            if(selectionSettings.NeedWorkWithTransverseRebarViewFirst) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.TransverseRebarViewFirst.ViewElement != null) {
                    SheetInfo.TransverseRebarViewFirst.ViewDimensionCreator
                        .TryCreateTransverseRebarViewFirstDimensions();
                }
            }

            // ОБРАЗМЕРИВАНИЕ ВТОРОГО ПОПЕРЕЧНОГО ВИД АРМИРОВАНИЯ
            if(selectionSettings.NeedWorkWithTransverseRebarViewSecond) {
                // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                if(SheetInfo.TransverseRebarViewSecond.ViewElement != null) {
                    SheetInfo.TransverseRebarViewSecond.ViewDimensionCreator
                        .TryCreateTransverseRebarViewSecondDimensions();
                }
            }
        }
    }
}
