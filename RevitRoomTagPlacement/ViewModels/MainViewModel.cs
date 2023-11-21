using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRoomTagPlacement.Models;

namespace RevitRoomTagPlacement.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private RevitViewModel _revitViewModel;

        private string _saveProperty;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            RevitViewModels = new ObservableCollection<RevitViewModel> {
                new ViewRevitViewModel(revitRepository) { Name = "Выборка по текущему виду" },
                new SelectedRevitViewModel(revitRepository) { Name = "Выборка по выделенным элементам" }
            };

            if(_revitRepository.GetSelectedRooms().Count() > 0) {
                RevitViewModel = RevitViewModels[1];
            } 
            else {
                RevitViewModel = RevitViewModels[0];
            }

        }

        public RevitViewModel RevitViewModel {
            get => _revitViewModel;
            set => RaiseAndSetIfChanged(ref _revitViewModel, value);
        }

        public ObservableCollection<RevitViewModel> RevitViewModels { get; }

        public string SaveProperty {
            get => _saveProperty;
            set => RaiseAndSetIfChanged(ref _saveProperty, value);
        }
    }
}
