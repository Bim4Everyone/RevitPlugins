using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models.Configs;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    internal class SizeViewModel : BaseViewModel {
        private double _value;
        private string _name;

        public SizeViewModel(Size size) {
            Name = size.Name;
            Value = size.Value;
        }

        public SizeViewModel() {

        }

        public string Name {
            get => _name;
            set => this.RaiseAndSetIfChanged(ref _name, value);
        }
        public double Value {
            get => _value;
            set => this.RaiseAndSetIfChanged(ref _value, value);
        }

        public string GetErrorText() {
            if(Value < 0) {
                return $"значение параметра \"{Name}\" должно быть неотрицательным.";
            }
            return null;
        }

        public Size GetSize() {
            return new Size() { Name = Name, Value = Value };
        }
    }
}
