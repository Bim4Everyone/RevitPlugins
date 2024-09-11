using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.ViewModels;

namespace RevitPylonDocumentation.Models.UserSettings {
    internal class UserSchedulesSettings : BaseViewModel {
        public UserSchedulesSettings(MainViewModel mainViewModel) {

            ViewModel = mainViewModel;
        }

        public MainViewModel ViewModel { get; set; }

        // Префиксы и суффиксы для поиска и новых спек
        private string _rebarSchedulePrefixTemp = "КЖ..._СА_";
        private string _rebarScheduleSuffixTemp = "";
        private string _materialSchedulePrefixTemp = "КЖ..._СМ_";
        private string _materialScheduleSuffixTemp = "";
        private string _systemPartsSchedulePrefixTemp = "КЖ..._ВД_";
        private string _systemPartsScheduleSuffixTemp = "_Системная";
        private string _ifcPartsSchedulePrefixTemp = "КЖ..._ВД_";
        private string _ifcPartsScheduleSuffixTemp = "_IFC";

        // Названия эталонных спек
        private string _rebarScheduleNameTemp = "(КЖ...)_СА_Базовая_(Марка пилона)";
        private string _materialScheduleNameTemp = "(КЖ...)_СМ_Базовая_(Марка пилона)";
        private string _systemPartsScheduleNameTemp = "(КЖ...)_ВД_(Марка пилона)_Системная";
        private string _ifcPartsScheduleNameTemp = "(КЖ...)_ВД_(Марка пилона)_IFC";

        // Заполнение параметров диспетчера
        private string _rebarScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _rebarScheduleDisp2Temp = "СА_Пилоны";
        private string _materialScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _materialScheduleDisp2Temp = "СМ_Пилоны";
        private string _systemPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _systemPartsScheduleDisp2Temp = "ВД_СИС_Пилоны";
        private string _ifcPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _ifcPartsScheduleDisp2Temp = "ВД_IFC_Пилоны";

        // Фильтрация спек
        private ObservableCollection<ScheduleFilterParamHelper> _paramsForScheduleFiltersTemp = new ObservableCollection<ScheduleFilterParamHelper>() {
            new ScheduleFilterParamHelper("обр_ФОП_Форма_номер", ""),
            new ScheduleFilterParamHelper("обр_ФОП_Раздел проекта", "обр_ФОП_Раздел проекта"),
            new ScheduleFilterParamHelper("обр_ФОП_Орг. уровень", "обр_ФОП_Орг. уровень"),
            new ScheduleFilterParamHelper("обр_Метка основы_универсальная", "Марка"),
            new ScheduleFilterParamHelper("Марка", "Марка")
        };

        public string RebarSchedulePrefix { get; set; }
        public string RebarSchedulePrefixTemp {
            get => _rebarSchedulePrefixTemp;
            set => RaiseAndSetIfChanged(ref _rebarSchedulePrefixTemp, value);
        }

        public string RebarScheduleSuffix { get; set; }
        public string RebarScheduleSuffixTemp {
            get => _rebarScheduleSuffixTemp;
            set => RaiseAndSetIfChanged(ref _rebarScheduleSuffixTemp, value);
        }


        public string MaterialSchedulePrefix { get; set; }
        public string MaterialSchedulePrefixTemp {
            get => _materialSchedulePrefixTemp;
            set => RaiseAndSetIfChanged(ref _materialSchedulePrefixTemp, value);
        }

        public string MaterialScheduleSuffix { get; set; }
        public string MaterialScheduleSuffixTemp {
            get => _materialScheduleSuffixTemp;
            set => RaiseAndSetIfChanged(ref _materialScheduleSuffixTemp, value);
        }

        public string SystemPartsSchedulePrefix { get; set; }
        public string SystemPartsSchedulePrefixTemp {
            get => _systemPartsSchedulePrefixTemp;
            set => RaiseAndSetIfChanged(ref _systemPartsSchedulePrefixTemp, value);
        }

        public string SystemPartsScheduleSuffix { get; set; }
        public string SystemPartsScheduleSuffixTemp {
            get => _systemPartsScheduleSuffixTemp;
            set => RaiseAndSetIfChanged(ref _systemPartsScheduleSuffixTemp, value);
        }

        public string IfcPartsSchedulePrefix { get; set; }
        public string IfcPartsSchedulePrefixTemp {
            get => _ifcPartsSchedulePrefixTemp;
            set => RaiseAndSetIfChanged(ref _ifcPartsSchedulePrefixTemp, value);
        }

        public string IfcPartsScheduleSuffix { get; set; }
        public string IfcPartsScheduleSuffixTemp {
            get => _ifcPartsScheduleSuffixTemp;
            set => RaiseAndSetIfChanged(ref _ifcPartsScheduleSuffixTemp, value);
        }

        public string RebarScheduleName { get; set; }
        public string RebarScheduleNameTemp {
            get => _rebarScheduleNameTemp;
            set => RaiseAndSetIfChanged(ref _rebarScheduleNameTemp, value);
        }

        public string MaterialScheduleName { get; set; }
        public string MaterialScheduleNameTemp {
            get => _materialScheduleNameTemp;
            set => RaiseAndSetIfChanged(ref _materialScheduleNameTemp, value);
        }

        public string SytemPartsScheduleName { get; set; }
        public string SytemPartsScheduleNameTemp {
            get => _systemPartsScheduleNameTemp;
            set => RaiseAndSetIfChanged(ref _systemPartsScheduleNameTemp, value);
        }

        public string IfcPartsScheduleName { get; set; }
        public string IfcPartsScheduleNameTemp {
            get => _ifcPartsScheduleNameTemp;
            set => RaiseAndSetIfChanged(ref _ifcPartsScheduleNameTemp, value);
        }

        public string RebarScheduleDisp1 { get; set; }
        public string RebarScheduleDisp1Temp {
            get => _rebarScheduleDisp1Temp;
            set => RaiseAndSetIfChanged(ref _rebarScheduleDisp1Temp, value);
        }
        public string MaterialScheduleDisp1 { get; set; }
        public string MaterialScheduleDisp1Temp {
            get => _materialScheduleDisp1Temp;
            set => RaiseAndSetIfChanged(ref _materialScheduleDisp1Temp, value);
        }
        public string SystemPartsScheduleDisp1 { get; set; }
        public string SystemPartsScheduleDisp1Temp {
            get => _systemPartsScheduleDisp1Temp;
            set => RaiseAndSetIfChanged(ref _systemPartsScheduleDisp1Temp, value);
        }
        public string IfcPartsScheduleDisp1 { get; set; }
        public string IfcPartsScheduleDisp1Temp {
            get => _ifcPartsScheduleDisp1Temp;
            set => RaiseAndSetIfChanged(ref _ifcPartsScheduleDisp1Temp, value);
        }

        public string RebarScheduleDisp2 { get; set; }
        public string RebarScheduleDisp2Temp {
            get => _rebarScheduleDisp2Temp;
            set => RaiseAndSetIfChanged(ref _rebarScheduleDisp2Temp, value);
        }
        public string MaterialScheduleDisp2 { get; set; }
        public string MaterialScheduleDisp2Temp {
            get => _materialScheduleDisp2Temp;
            set => RaiseAndSetIfChanged(ref _materialScheduleDisp2Temp, value);
        }
        public string SystemPartsScheduleDisp2 { get; set; }
        public string SystemPartsScheduleDisp2Temp {
            get => _systemPartsScheduleDisp2Temp;
            set => RaiseAndSetIfChanged(ref _systemPartsScheduleDisp2Temp, value);
        }

        public string IfcPartsScheduleDisp2 { get; set; }
        public string IfcPartsScheduleDisp2Temp {
            get => _ifcPartsScheduleDisp2Temp;
            set => RaiseAndSetIfChanged(ref _ifcPartsScheduleDisp2Temp, value);
        }

        public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters { get; set; } = new ObservableCollection<ScheduleFilterParamHelper>();
        public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFiltersTemp {
            get => _paramsForScheduleFiltersTemp;
            set => RaiseAndSetIfChanged(ref _paramsForScheduleFiltersTemp, value);
        }


        public void ApplySchedulesSettings() {
            RebarSchedulePrefix = RebarSchedulePrefixTemp;
            RebarScheduleSuffix = RebarScheduleSuffixTemp;

            MaterialSchedulePrefix = MaterialSchedulePrefixTemp;
            MaterialScheduleSuffix = MaterialScheduleSuffixTemp;

            SystemPartsSchedulePrefix = SystemPartsSchedulePrefixTemp;
            SystemPartsScheduleSuffix = SystemPartsScheduleSuffixTemp;

            IfcPartsSchedulePrefix = IfcPartsSchedulePrefixTemp;
            IfcPartsScheduleSuffix = IfcPartsScheduleSuffixTemp;

            RebarScheduleName = RebarScheduleNameTemp;
            MaterialScheduleName = MaterialScheduleNameTemp;
            SytemPartsScheduleName = SytemPartsScheduleNameTemp;
            IfcPartsScheduleName = IfcPartsScheduleNameTemp;

            RebarScheduleDisp1 = RebarScheduleDisp1Temp;
            MaterialScheduleDisp1 = MaterialScheduleDisp1Temp;
            SystemPartsScheduleDisp1 = SystemPartsScheduleDisp1Temp;
            IfcPartsScheduleDisp1 = IfcPartsScheduleDisp1Temp;

            RebarScheduleDisp2 = RebarScheduleDisp2Temp;
            MaterialScheduleDisp2 = MaterialScheduleDisp2Temp;
            SystemPartsScheduleDisp2 = SystemPartsScheduleDisp2Temp;
            IfcPartsScheduleDisp2 = IfcPartsScheduleDisp2Temp;

            ParamsForScheduleFilters = ParamsForScheduleFiltersTemp;
        }
    }
}
