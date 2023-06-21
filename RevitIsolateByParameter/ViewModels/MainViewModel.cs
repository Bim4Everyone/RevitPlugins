using System.Collections.Generic;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using RevitIsolateByParameter.Models;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows;

namespace RevitIsolateByParameter.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ParameterElement _selectedParameter;
        private string _selectedValue;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            IsolateElementsCommand = new RelayCommand(IsolateElement);
            GetParameterValuesCommand = new RelayCommand(GetPossibleValues);

            Parameters = _revitRepository.GetParameters();
            ParametersValues = _revitRepository.GetParameterValues(Parameters);

        }

        public ICommand IsolateElementsCommand { get; }
        public ICommand GetParameterValuesCommand { get; }


        public ParameterElement SelectedParameter {
            get => _selectedParameter;
            set => RaiseAndSetIfChanged(ref _selectedParameter, value);
        }

        public ObservableCollection<ParameterElement> Parameters { get; }
        public Dictionary<string, List<string>> ParametersValues { get; }

        public List<string> ParameterValues { get; set; } = new List<string>();

        public string SelectedValue {
            get => _selectedValue;
            set => RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        private async void IsolateElement(object p) {
            await _revitRepository.IsolateElements(SelectedParameter, SelectedValue);
        }

        public void GetPossibleValues(object p) {
            ParameterValues = ParametersValues[SelectedParameter.GetDefinition().Name];
            OnPropertyChanged(nameof(ParameterValues));
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}