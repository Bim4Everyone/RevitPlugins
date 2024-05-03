using System;
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
        private string _toolTipInfo;

        public LevelViewModel(LevelInfo levelInfoInfo) {
            _levelInfo = levelInfoInfo;
        }

        public string Name => _levelInfo.Level.Name;
        public string MeterElevation => _levelInfo.Level.GetFormattedMeterElevation();
        public string MillimeterElevation => _levelInfo.Level.GetFormattedMillimeterElevation();

        public LevelInfo LevelInfo => _levelInfo;

        public ErrorType ErrorType {
            get => _errorType;
            set => this.RaiseAndSetIfChanged(ref _errorType, value);
        }

        public string ToolTipInfo {
            get => _toolTipInfo;
            set => this.RaiseAndSetIfChanged(ref _toolTipInfo, value);
        }
    }
}