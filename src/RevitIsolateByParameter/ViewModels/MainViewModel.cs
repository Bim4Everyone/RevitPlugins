using System.Windows;
using System.Windows.Input;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Autodesk.Revit.UI;
using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using dosymep.Bim4Everyone.SharedParams;
using RevitIsolateByParameter.Models;

namespace RevitIsolateByParameter.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ParamViewModel _selectedParameter;
        private ParamValue _selectedValue;
        private int _selectedIndex;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            ParamViewModels = new ObservableCollection<ParamViewModel>();

            if(SharedParamsConfig.Instance.BuildingWorksLevel.IsExistsParam(revitRepository.Document))
                ParamViewModels.Add(new ParamViewModel(revitRepository.Document, SharedParamsConfig.Instance.BuildingWorksLevel));
            if(SharedParamsConfig.Instance.BuildingWorksSection.IsExistsParam(revitRepository.Document))
                ParamViewModels.Add(new ParamViewModel(revitRepository.Document, SharedParamsConfig.Instance.BuildingWorksSection));
            if(SharedParamsConfig.Instance.BuildingWorksBlock.IsExistsParam(revitRepository.Document))
                ParamViewModels.Add(new ParamViewModel(revitRepository.Document, SharedParamsConfig.Instance.BuildingWorksBlock));

            IsolateElementsCommand = new RelayCommand(IsolateElement, CanIsolate);
            GetParameterValuesCommand = new RelayCommand(GetPossibleValues, CanIsolate);

            SelectNextCommand = new RelayCommand(SelectNext, CanSelectNext);
            SelectPreviousCommand = new RelayCommand(SelectPrevious, CanSelectPrevious);
        }

        public ICommand IsolateElementsCommand { get; }
        public ICommand GetParameterValuesCommand { get; }
        public ICommand SelectNextCommand { get; }
        public ICommand SelectPreviousCommand { get; }


        public ParamViewModel SelectedParameter {
            get => _selectedParameter;
            set => RaiseAndSetIfChanged(ref _selectedParameter, value);
        }

        public ObservableCollection<ParamViewModel> ParamViewModels { get; }

        public IList<ParamValue> ParameterValues { get; set; } = new List<ParamValue>();

        public ParamValue SelectedValue {
            get => _selectedValue;
            set => RaiseAndSetIfChanged(ref _selectedValue, value);
        }

        public int SelectedIndex {
            get => _selectedIndex;
            set => RaiseAndSetIfChanged(ref _selectedIndex, value);
        }
        
        public void GetPossibleValues(object p) {
            ParameterValues = SelectedParameter.Values;
            OnPropertyChanged(nameof(ParameterValues));
        }

        private async void IsolateElement(object p) {
            await _revitRepository.IsolateElements(SelectedParameter, SelectedValue);
        }

        private bool CanIsolate(object p) { 
            if(ParamViewModels.Count != 3) {
                ErrorText = "В проекте отсутствуют параметры СМР";
                return false;
            }

            if(!_revitRepository.Document.ActiveView.CanEnableTemporaryViewPropertiesMode()) {
                ErrorText = "Должен быть открыт вид с геометрией здания";
                return false;
            }

            return true;
        }

        private async void SelectNext(object p) {
            SelectedIndex += 1;
            await _revitRepository.IsolateElements(SelectedParameter, SelectedValue);
        }

        private bool CanSelectNext(object p) {
            if(SelectedIndex >= ParameterValues.Count - 1) {
                return false;
            }

            return true;
        }

        private async void SelectPrevious(object p) {
            SelectedIndex -= 1;
            await _revitRepository.IsolateElements(SelectedParameter, SelectedValue);
        }

        private bool CanSelectPrevious(object p) {
            if(SelectedIndex <= 0) {
                return false;
            }

            return true;
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }
    }
}