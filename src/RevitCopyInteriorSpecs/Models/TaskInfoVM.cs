using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitCopyInteriorSpecs.Models {
    internal class TaskInfoVM : BaseViewModel {
        private string _groupType;
        private Level _level;
        private string _levelShortName;
        private Phase _phase;
        private string _firstDispatcherGroupingLevel;
        private string _secondDispatcherGroupingLevel;
        private string _thirdDispatcherGroupingLevel;

        public TaskInfoVM() { }


        public string GroupType {
            get => _groupType;
            set => this.RaiseAndSetIfChanged(ref _groupType, value);
        }

        public Level Level {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        public string LevelShortName {
            get => _levelShortName;
            set => this.RaiseAndSetIfChanged(ref _levelShortName, value);
        }

        public Phase Phase {
            get => _phase;
            set => this.RaiseAndSetIfChanged(ref _phase, value);
        }

        public string FirstDispatcherGroupingLevel {
            get => _firstDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _firstDispatcherGroupingLevel, value);
        }

        public string SecondDispatcherGroupingLevel {
            get => _secondDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _secondDispatcherGroupingLevel, value);
        }

        public string ThirdDispatcherGroupingLevel {
            get => _thirdDispatcherGroupingLevel;
            set => this.RaiseAndSetIfChanged(ref _thirdDispatcherGroupingLevel, value);
        }
    }
}
