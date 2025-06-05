using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitFinishing.ViewModels
{
    internal abstract class SelectionElementVM : BaseViewModel {
        private readonly string _name;
        private readonly BuiltInParameter _bltnParam;
        private readonly bool _isEmpty;
        private bool _isChecked;

        public SelectionElementVM(string name, BuiltInParameter bltnParam) {
            if(string.IsNullOrEmpty(name)) {
                _name = "<Без имени>";
                _isEmpty = true;
            } else {
                _name = name;
                _isEmpty = false;
            }

            _bltnParam = bltnParam;
        }
        public string Name => _name;

        public bool IsChecked {
            get => _isChecked;
            set => RaiseAndSetIfChanged(ref _isChecked, value);
        }

        public ElementParameterFilter GetStringFilter() {
            ElementId paramId = new ElementId(_bltnParam);
            FilterRule filterRule;

            if(_isEmpty) {
                filterRule = ParameterFilterRuleFactory.CreateHasNoValueParameterRule(paramId);
            } else {
                ParameterValueProvider valueProvider = new ParameterValueProvider(paramId);
                FilterStringEquals ruleEvaluator = new FilterStringEquals();
                filterRule = new FilterStringRule(valueProvider, ruleEvaluator, Name);
            }

            return new ElementParameterFilter(filterRule);
        }
    }
}
