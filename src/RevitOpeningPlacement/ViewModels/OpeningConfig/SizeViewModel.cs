using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class SizeViewModel : BaseViewModel {
        private double _value;
        private string _displayName;
        private readonly string _sizeName;

        public SizeViewModel(Size size) {
            _sizeName = size.Name;
            DisplayName = size.Name;
            Value = size.Value;
        }

        public SizeViewModel() {

        }

        public string DisplayName {
            get => _displayName;
            set => this.RaiseAndSetIfChanged(ref _displayName, value);
        }
        public double Value {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public string GetErrorText() {
            if(Value < 0) {
                return $"значение параметра \"{DisplayName}\" должно быть неотрицательным.";
            }
            return null;
        }

        public Size GetSize() {
            return new Size() { Name = _sizeName, Value = Value };
        }
    }
}
