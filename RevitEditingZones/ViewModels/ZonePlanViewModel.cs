using System.Collections.ObjectModel;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitEditingZones.Models;

namespace RevitEditingZones.ViewModels {
    internal class ZonePlanViewModel : BaseViewModel {
        private LevelViewModel _level;
        private ObservableCollection<LevelViewModel> _levels;
        private ErrorType _errorType;

        public ZonePlanViewModel(Area area, ViewPlan areaPlan) {
            Area = area;
            AreaPlan = areaPlan;
        }

        public Area Area { get; }
        public ViewPlan AreaPlan { get; }

        public string AreaName => Area.GetParamValueOrDefault<string>(BuiltInParameter.ROOM_NAME);
        public string AreaPlanName => AreaPlan.Name;

        public LevelViewModel Level {
            get => _level;
            set => this.RaiseAndSetIfChanged(ref _level, value);
        }

        public ObservableCollection<LevelViewModel> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }

        public ErrorType ErrorType {
            get => _errorType;
            set => this.RaiseAndSetIfChanged(ref _errorType, value);
        }
    }
}