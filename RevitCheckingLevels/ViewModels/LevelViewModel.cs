using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitCheckingLevels.Models;
using RevitCheckingLevels.Models.LevelParser;

namespace RevitCheckingLevels.ViewModels {
    internal class LevelViewModel : BaseViewModel {
        private readonly LevelInfo _levelInfo;
        private ErrorType _errorType;

        public LevelViewModel(LevelInfo levelInfoInfo) {
            _levelInfo = levelInfoInfo;

            ErrorType = GetErrorType();
            Errors = new ObservableCollection<string>(_levelInfo.Errors);
        }

        public string Name => _levelInfo.Level.Name;
        public string Elevation => LevelExtensions.GetFormattedMeterElevation(_levelInfo.Level);

        public ErrorType ErrorType {
            get => _errorType;
            set => this.RaiseAndSetIfChanged(ref _errorType, value);
        }

        public ObservableCollection<string> Errors { get; }

        private ErrorType GetErrorType() {
            if(_levelInfo.Errors.Count > 0) {
                return ErrorType.NotStandard;
            }

            double elevation = LevelExtensions.GetMeterElevation(_levelInfo.Level);
            if(!LevelExtensions.IsAlmostEqual(_levelInfo.Elevation, elevation, 0.001)) {
                return ErrorType.NotElevation;
            }

            if(!LevelExtensions.IsAlmostEqual(elevation % 1, 0.0000001, 0.0000001)) {
                return ErrorType.NotMillimeterElevation;
            }

            return null;
        }
    }
}
