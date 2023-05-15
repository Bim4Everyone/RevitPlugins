using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using RevitDeleteUnused.Models;
using RevitDeleteUnused.Commands;

namespace RevitDeleteUnused.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ElementsToDeleteViewModel _selectedElementType;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            RevitViewModels = new ObservableCollection<ElementsToDeleteViewModel>
            {
                new ElementsToDeleteViewModel(_revitRepository.Document, ElementsCollector.GetFilters(_revitRepository.Document), "Фильтры"),
                new ElementsToDeleteViewModel(_revitRepository.Document, ElementsCollector.GetViewTemplates(_revitRepository.Document),  "Шаблоны видов")
            };
        }

        public ElementsToDeleteViewModel SelectedElementType {
            get => _selectedElementType;
            set => RaiseAndSetIfChanged(ref _selectedElementType, value);
        }

        public ObservableCollection<ElementsToDeleteViewModel> RevitViewModels { get; }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }

        private RelayCommand checkAll;
        public RelayCommand CheckAllCommand {
            get {
                return checkAll ?? new RelayCommand(obj => { CheckBoxCommands.SetAll(_selectedElementType.ElementsToDelete, true); });
            }
        }

        private RelayCommand unCheckAll;
        public RelayCommand UnCheckAllCommand {
            get {
                return unCheckAll ?? new RelayCommand(obj => { CheckBoxCommands.SetAll(_selectedElementType.ElementsToDelete, false); });
            }
        }

        private RelayCommand invertAll;
        public RelayCommand InvertAllCommand {
            get {
                return invertAll ?? new RelayCommand(obj => { CheckBoxCommands.InvertAll(_selectedElementType.ElementsToDelete); });
            }
        }
    }
}