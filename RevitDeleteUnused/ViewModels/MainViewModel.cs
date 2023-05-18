using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using RevitDeleteUnused.Models;
using Autodesk.Revit.DB;
using System.Windows.Input;

namespace RevitDeleteUnused.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private ElementsToDeleteViewModel _selectedElementType;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            RevitViewModels = new ObservableCollection<ElementsToDeleteViewModel>
            {
                new ElementsToDeleteViewModel(_revitRepository, _revitRepository.GetFilters(), "Фильтры"),
                new ElementsToDeleteViewModel(_revitRepository, _revitRepository.GetViewTemplates(),  "Шаблоны видов")
            };
            SelectedElementType = RevitViewModels[0];

            CheckAllCommand = new RelayCommand(CheckAll);
            UnCheckAllCommand = new RelayCommand(UnCheckAll);
            InvertAllCommand = new RelayCommand(InvertAll);
        }

        public ICommand CheckAllCommand { get; }
        public ICommand UnCheckAllCommand { get; }
        public ICommand InvertAllCommand { get; }

        public ElementsToDeleteViewModel SelectedElementType {
            get => _selectedElementType;
            set => RaiseAndSetIfChanged(ref _selectedElementType, value);
        }

        public ObservableCollection<ElementsToDeleteViewModel> RevitViewModels { get; }

        private void CheckAll(object p) {
            _revitRepository.SetAll(_selectedElementType.ElementsToDelete, true);
        }

        private void UnCheckAll(object p) {
            _revitRepository.SetAll(_selectedElementType.ElementsToDelete, false);
        }

        private void InvertAll(object p) {
            _revitRepository.InvertAll(_selectedElementType.ElementsToDelete);
        }
    }
}