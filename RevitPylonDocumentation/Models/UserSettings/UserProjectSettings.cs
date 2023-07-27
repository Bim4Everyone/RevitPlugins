using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings {
    class UserProjectSettings : BaseViewModel {

        public UserProjectSettings(MainViewModel mainViewModel, RevitRepository repository) {

            ViewModel = mainViewModel;
            Repository = repository;
        }

        public MainViewModel ViewModel { get; set; }
        internal RevitRepository Repository { get; set; }




        private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
        private string _markTemp = "Марка";
        private string _titleBlockNameTemp = "Создать типы по комплектам";
        private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
        private string _dispatcherGroupingSecondTemp = "_Группа видов 2";
        private string _sheetSizeTemp = "А";
        private string _sheetCoefficientTemp = "х";

        private string _sheetPrefixTemp = "Пилон ";
        private string _sheetSuffixTemp = "";

        private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
        private string _typicalPylonFilterValueTemp = "на 1 шт.";

        private string _legendNameTemp = "Указания для пилонов";


        public string PROJECT_SECTION { get; set; } = "обр_ФОП_Раздел проекта";
        public string PROJECT_SECTION_TEMP {
            get => _projectSectionTemp;
            set {
                RaiseAndSetIfChanged(ref _projectSectionTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string MARK { get; set; } = "Марка";
        public string MARK_TEMP {
            get => _markTemp;
            set {
                RaiseAndSetIfChanged(ref _markTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TITLEBLOCK_NAME { get; set; } = "Создать типы по комплектам";
        public string TITLEBLOCK_NAME_TEMP {
            get => _titleBlockNameTemp;
            set {
                RaiseAndSetIfChanged(ref _titleBlockNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        // dispatcher grouping
        public string DISPATCHER_GROUPING_FIRST { get; set; } = "_Группа видов 1";
        public string DISPATCHER_GROUPING_FIRST_TEMP {
            get => _dispatcherGroupingFirstTemp;
            set {
                RaiseAndSetIfChanged(ref _dispatcherGroupingFirstTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }
        public string DISPATCHER_GROUPING_SECOND { get; set; } = "_Группа видов 2";
        public string DISPATCHER_GROUPING_SECOND_TEMP {
            get => _dispatcherGroupingSecondTemp;
            set {
                RaiseAndSetIfChanged(ref _dispatcherGroupingSecondTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_SIZE { get; set; } = "А";
        public string SHEET_SIZE_TEMP {
            get => _sheetSizeTemp;
            set {
                RaiseAndSetIfChanged(ref _sheetSizeTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_COEFFICIENT { get; set; } = "х";
        public string SHEET_COEFFICIENT_TEMP {
            get => _sheetCoefficientTemp;
            set {
                RaiseAndSetIfChanged(ref _sheetCoefficientTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string SHEET_PREFIX { get; set; } = "Пилон ";
        public string SHEET_PREFIX_TEMP {
            get => _sheetPrefixTemp;
            set {
                RaiseAndSetIfChanged(ref _sheetPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_SUFFIX { get; set; } = "";
        public string SHEET_SUFFIX_TEMP {
            get => _sheetSuffixTemp;
            set {
                RaiseAndSetIfChanged(ref _sheetSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; } = "обр_ФОП_Фильтрация 1";
        public string TYPICAL_PYLON_FILTER_PARAMETER_TEMP {
            get => _typicalPylonFilterParameterTemp;
            set {
                RaiseAndSetIfChanged(ref _typicalPylonFilterParameterTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TYPICAL_PYLON_FILTER_VALUE { get; set; } = "на 1 шт.";
        public string TYPICAL_PYLON_FILTER_VALUE_TEMP {
            get => _typicalPylonFilterValueTemp;
            set {
                RaiseAndSetIfChanged(ref _typicalPylonFilterValueTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string LEGEND_NAME { get; set; } = "Указания для пилонов";
        public string LEGEND_NAME_TEMP {
            get => _legendNameTemp;
            set {
                RaiseAndSetIfChanged(ref _legendNameTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public void ApplyProjectSettings() {

            PROJECT_SECTION = PROJECT_SECTION_TEMP;
            MARK = MARK_TEMP;
            TITLEBLOCK_NAME = TITLEBLOCK_NAME_TEMP;
            DISPATCHER_GROUPING_FIRST = DISPATCHER_GROUPING_FIRST_TEMP;
            DISPATCHER_GROUPING_SECOND = DISPATCHER_GROUPING_SECOND_TEMP;

            SHEET_SIZE = SHEET_SIZE_TEMP;
            SHEET_COEFFICIENT = SHEET_COEFFICIENT_TEMP;
            SHEET_PREFIX = SHEET_PREFIX_TEMP;
            SHEET_SUFFIX = SHEET_SUFFIX_TEMP;


            TYPICAL_PYLON_FILTER_PARAMETER = TYPICAL_PYLON_FILTER_PARAMETER_TEMP;
            TYPICAL_PYLON_FILTER_VALUE = TYPICAL_PYLON_FILTER_VALUE_TEMP;

            LEGEND_NAME = LEGEND_NAME_TEMP;
        }

        public void CheckProjectSettings() {

            //Какиет о проблемы с проверкой прааметров диспетчера
            //    нужно подчистить классы, так чтобы изначально в не темпах ничего не было, а значение задавалось в Apply

            // Пытаемся проверить виды
            if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DISPATCHER_GROUPING_FIRST) is null) {
                ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
            }
            if(Repository.AllSectionViews.FirstOrDefault()?.LookupParameter(DISPATCHER_GROUPING_SECOND) is null) {
                ViewModel.ErrorText = "Наименование параметра диспетчера 2 некорректно";
            }
            
            // Пытаемся проверить спеки
            if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DISPATCHER_GROUPING_FIRST) is null) {
                ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
            }
            if(Repository.AllScheduleViews.FirstOrDefault()?.LookupParameter(DISPATCHER_GROUPING_SECOND) is null) {
                ViewModel.ErrorText = "Наименование параметра диспетчера 2 некорректно";
            }


            using(Transaction transaction = Repository.Document.StartTransaction("Проверка параметров на листе")) {

                // Листов в проекте может не быть или рамка может быть другая, поэтому создаем свой лист для тестов с нужной рамкой
                ViewSheet viewSheet = ViewSheet.Create(Repository.Document, ViewModel.SelectedTitleBlock.Id);
                if(viewSheet?.LookupParameter(DISPATCHER_GROUPING_FIRST) is null) {
                    ViewModel.ErrorText = "Наименование параметра диспетчера 1 некорректно";
                }

                // Ищем рамку листа
                FamilyInstance titleBlock = new FilteredElementCollector(Repository.Document, viewSheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstOrDefault() as FamilyInstance;

                if(titleBlock?.LookupParameter(SHEET_SIZE) is null) {
                    ViewModel.ErrorText = "Наименование параметра формата рамки листа некорректно";
                }
                if(titleBlock?.LookupParameter(SHEET_COEFFICIENT) is null) {
                    ViewModel.ErrorText = "Наименование параметра множителя формата рамки листа некорректно";
                }

                // Удаляем созданный лист
                Repository.Document.Delete(viewSheet.Id);
            }
        }
    }
}
