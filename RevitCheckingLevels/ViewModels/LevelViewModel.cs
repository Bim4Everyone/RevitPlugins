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

        public string ToolTipInfo => GetTooltip();

        private string GetTooltip() {
            if(ErrorType.NotStandard == ErrorType) {
                return string.Join(Environment.NewLine, LevelInfo.Errors);
            } else if(ErrorType.NotElevation == ErrorType) {
                return $"Значение отметки: фактическое \"{_levelInfo.Level.GetFormattedMeterElevation()}\", в имени уровня \"{_levelInfo.GetFormattedMeterElevation()}\".";
            } else if(ErrorType.NotMillimeterElevation == ErrorType) {
                return $"Значение отметки: фактическое \"{_levelInfo.Level.GetFormattedMillimeterElevation()}\".";
            } 

            return null;
        }
    }
}