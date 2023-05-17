using System.Collections.ObjectModel;

using dosymep.WPF.ViewModels;
using dosymep.WPF.Commands;

using RevitDeleteUnused.Models;
using RevitDeleteUnused.Commands;
using Autodesk.Revit.DB;

namespace RevitDeleteUnused.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;
        private readonly Document _document;

        private ElementsToDeleteViewModel _selectedElementType;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            _document = _revitRepository.Document;

            RevitViewModels = new ObservableCollection<ElementsToDeleteViewModel>
            {
                new ElementsToDeleteViewModel(_document, ElementsCollector.GetFilters(_document), "Фильтры"),
                new ElementsToDeleteViewModel(_document, ElementsCollector.GetViewTemplates(_document),  "Шаблоны видов")
            };
            SelectedElementType = RevitViewModels[0];
        }

        public ElementsToDeleteViewModel SelectedElementType {
            get => _selectedElementType;
            set => RaiseAndSetIfChanged(ref _selectedElementType, value);
        }

        public ObservableCollection<ElementsToDeleteViewModel> RevitViewModels { get; }

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