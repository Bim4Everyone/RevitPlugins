using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models
{
    class UserProjectSettings : BaseViewModel {

        public UserProjectSettings(MainViewModel mainViewModel) {

            ViewModel = mainViewModel;
        }

        public MainViewModel ViewModel { get; set; }




        private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
        private string _markTemp = "Марка";
        private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
        private string _dispatcherGroupingSecondTemp = "_Группа видов 1";
        private string _sheetSizeTemp = "А";
        private string _sheetCoefficientTemp = "х";

        private string _sheetPrefixTemp = "Пилон ";
        private string _sheetSuffixTemp = "";

        private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
        private string _typicalPylonFilterValueTemp = "на 1 шт.";

        public string DEF_TITLEBLOCK_NAME = "Создать типы по комплектам";


        public string PROJECT_SECTION { get; set; } = "обр_ФОП_Раздел проекта";
        public string PROJECT_SECTION_TEMP {
            get => _projectSectionTemp;
            set {
                this.RaiseAndSetIfChanged(ref _projectSectionTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string MARK { get; set; } = "Марка";
        public string MARK_TEMP {
            get => _markTemp;
            set {
                this.RaiseAndSetIfChanged(ref _markTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        // dispatcher grouping
        public string DISPATCHER_GROUPING_FIRST { get; set; } = "_Группа видов 1";
        public string DISPATCHER_GROUPING_FIRST_TEMP {
            get => _dispatcherGroupingFirstTemp;
            set {
                this.RaiseAndSetIfChanged(ref _dispatcherGroupingFirstTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }
        public string DISPATCHER_GROUPING_SECOND { get; set; } = "_Группа видов 2";
        public string DISPATCHER_GROUPING_SECOND_TEMP {
            get => _dispatcherGroupingSecondTemp;
            set {
                this.RaiseAndSetIfChanged(ref _dispatcherGroupingSecondTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_SIZE { get; set; } = "А";
        public string SHEET_SIZE_TEMP {
            get => _sheetSizeTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetSizeTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_COEFFICIENT { get; set; } = "х";
        public string SHEET_COEFFICIENT_TEMP {
            get => _sheetCoefficientTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetCoefficientTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string SHEET_PREFIX { get; set; } = "Пилон ";
        public string SHEET_PREFIX_TEMP {
            get => _sheetPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetPrefixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string SHEET_SUFFIX { get; set; } = "";
        public string SHEET_SUFFIX_TEMP {
            get => _sheetSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetSuffixTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }

        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; } = "обр_ФОП_Фильтрация 1";
        public string TYPICAL_PYLON_FILTER_PARAMETER_TEMP {
            get => _typicalPylonFilterParameterTemp;
            set {
                this.RaiseAndSetIfChanged(ref _typicalPylonFilterParameterTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }


        public string TYPICAL_PYLON_FILTER_VALUE { get; set; } = "на 1 шт.";
        public string TYPICAL_PYLON_FILTER_VALUE_TEMP {
            get => _typicalPylonFilterValueTemp;
            set {
                this.RaiseAndSetIfChanged(ref _typicalPylonFilterValueTemp, value);
                ViewModel.SettingsEdited = true;
            }
        }
    }
}
