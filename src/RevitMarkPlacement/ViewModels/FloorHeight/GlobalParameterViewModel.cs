
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitMarkPlacement.ViewModels {
    internal class GlobalParameterViewModel : BaseViewModel {
        public GlobalParameterViewModel(string name, double value, ElementId elementId) {
            Name = name;
            Value = value;
            ElementId = elementId;
        }
        public ElementId ElementId { get; }
        public string Name { get; }
        public double Value { get; }
    }
}
