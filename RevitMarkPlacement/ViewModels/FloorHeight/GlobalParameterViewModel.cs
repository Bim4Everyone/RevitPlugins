
using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class GlobalParameterViewModel : BaseViewModel {
        public GlobalParameterViewModel(string name, double value) {
            Name = name;
            Value = value;
        }
        public string Name { get; }
        public double Value { get; }
    }
}
