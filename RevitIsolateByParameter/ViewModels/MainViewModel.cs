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
        private ObservableCollection<string> _parameterValues;
        private string _selectedValue;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            IsolateElementsCommand = new RelayCommand(IsolateElement);
            GetParameterValuesCommand = new RelayCommand(GetPossibleValues);

            Parameters = _revitRepository.GetParameters();

        }

        //public ICommand CreateFilterCommand { get; }
        public ICommand IsolateElementsCommand { get; }
        public ICommand GetParameterValuesCommand { get; }


        public ParameterElement SelectedParameter {
            get => _selectedParameter;
            set => RaiseAndSetIfChanged(ref _selectedParameter, value);
        }

        public ObservableCollection<ParameterElement> Parameters { get; }

        public ObservableCollection<string> ParameterValues { get; set; } = new ObservableCollection<string>();

        public string SelectedValue {
            get => _selectedValue;
            set => RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        private async void IsolateElement(object p) {
            List<ElementId> filteredElements = _revitRepository.GetFilteredElements(SelectedParameter, SelectedValue);
            await _revitRepository.IsolateElements(filteredElements);
        }

        public void GetPossibleValues(object p) {
            ParameterValues = _revitRepository.GetParameterValues(SelectedParameter);
            OnPropertyChanged(nameof(ParameterValues));
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}