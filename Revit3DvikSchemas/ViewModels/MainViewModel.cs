using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Revit3DvikSchemas.Models;

namespace Revit3DvikSchemas.ViewModels {
    internal class MainViewModel : BaseViewModel {

        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly ViewMaker _viewMaker;

        public List<HvacSystemViewModel> RevitHVACSystems { get; }

        public ICommand CreateViewCommand { get; }

        private string _errorText;

        private bool _combineFilters;

        private bool _useFopNames;


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository, ViewMaker viewMaker) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            RevitHVACSystems = _revitRepository.GetHVACSystems();

            bool RevitUseFopNames = UseFopNames;
            bool RevitCombineFilters = CombineFilters;

            CreateViewCommand = RelayCommand.Create(CreateViews, CanCreateView);
            _viewMaker = viewMaker;
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        public bool CombineFilters {
            get => _combineFilters;
            set => this.RaiseAndSetIfChanged(ref _combineFilters, value);
        }

        public bool UseFopNames {
            get => _useFopNames;
            set => this.RaiseAndSetIfChanged(ref _useFopNames, value);
        }

        private void CreateViews() {
            _viewMaker.CreateSelectedCommand(RevitHVACSystems, _useFopNames, _combineFilters);
        }

        private bool CanCreateView() {
            if(!RevitHVACSystems.Any(x => x.IsChecked)) {
                ErrorText = "Не выбраны элементы.";
                return false;
            }
            ErrorText = "";
            return true;
        }
    }

}

