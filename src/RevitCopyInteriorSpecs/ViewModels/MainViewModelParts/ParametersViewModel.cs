using dosymep.WPF.ViewModels;

namespace RevitCopyInteriorSpecs.ViewModels.MainViewModelParts {
    internal class ParametersViewModel : BaseViewModel {
        private string _groupTypeParamName = "ФОП_Тип квартиры";
        private string _levelParamName = "Уровень";
        private string _levelShortNameParamName = "ФОП_Этаж";
        private string _firstDispatcherGroupingLevelParamName = "_Стадия Проекта";
        private string _secondDispatcherGroupingLevelParamName = "_Группа Видов";
        private string _thirdDispatcherGroupingLevelParamName = "Назначение вида";

        public string GroupTypeParamName {
            get => _groupTypeParamName;
            set => this.RaiseAndSetIfChanged(ref _groupTypeParamName, value);
        }

        public string LevelParamName {
            get => _levelParamName;
            set => this.RaiseAndSetIfChanged(ref _levelParamName, value);
        }

        public string LevelShortNameParamName {
            get => _levelShortNameParamName;
            set => this.RaiseAndSetIfChanged(ref _levelShortNameParamName, value);
        }

        public string FirstDispatcherGroupingLevelParamName {
            get => _firstDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _firstDispatcherGroupingLevelParamName, value);
        }

        public string SecondDispatcherGroupingLevelParamName {
            get => _secondDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _secondDispatcherGroupingLevelParamName, value);
        }

        public string ThirdDispatcherGroupingLevelParamName {
            get => _thirdDispatcherGroupingLevelParamName;
            set => this.RaiseAndSetIfChanged(ref _thirdDispatcherGroupingLevelParamName, value);
        }
    }
}
