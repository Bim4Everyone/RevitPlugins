using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
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
        public string Elevation => _levelInfo.Level.GetFormattedMeterElevation();

        public LevelInfo LevelInfo => _levelInfo;

        public ErrorType ErrorType {
            get => _errorType;
            set => this.RaiseAndSetIfChanged(ref _errorType, value);
        }

        public ObservableCollection<string> Errors { get; }

        private ErrorType GetErrorType() {
            if(_levelInfo.Errors.Count > 0) {
                return ErrorType.NotStandard;
            }

            double meterElevation = _levelInfo.Level.GetMeterElevation();
            if(!LevelExtensions.IsAlmostEqual(_levelInfo.Elevation, meterElevation, 0.001)) {
                return ErrorType.NotElevation;
            }

            double millimeterElevation = _levelInfo.Level.GetMillimeterElevation();
            if(!LevelExtensions.IsAlmostEqual(millimeterElevation % 1, 0.0000001, 0.0000001)) {
                return ErrorType.NotMillimeterElevation;
            }

            return null;
        }
    }
}
