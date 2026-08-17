using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningSlopes.Models;

namespace RevitOpeningSlopes.ViewModels {
    internal class WallTypeExclusionsViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public WallTypeExclusionsViewModel(
            RevitRepository revitRepository,
            PluginConfig pluginConfig) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));

            WallTypes = new ObservableCollection<WallTypeExclusionViewModel>(
                _revitRepository.GetWallTypes()
                    .Select(wt => new WallTypeExclusionViewModel(wt)));
            Exclusions = new ObservableCollection<WallTypeExclusionRowViewModel>();

            var excludedIds = new HashSet<ElementId>(
                _pluginConfig.ExcludedWallTypeIds ?? new List<ElementId>());
            foreach(var wallType in WallTypes.Where(wt => excludedIds.Contains(wt.WallTypeId))) {
                Exclusions.Add(new WallTypeExclusionRowViewModel(wallType));
            }

            AddWallTypeCommand = RelayCommand.Create(AddWallType);
            RemoveWallTypeCommand = RelayCommand.Create<WallTypeExclusionRowViewModel>(RemoveWallType);
        }

        public ObservableCollection<WallTypeExclusionViewModel> WallTypes { get; }
        public ObservableCollection<WallTypeExclusionRowViewModel> Exclusions { get; }
        public ICommand AddWallTypeCommand { get; }
        public ICommand RemoveWallTypeCommand { get; }

        private void AddWallType() {
            var selectedIds = new HashSet<ElementId>(
                Exclusions
                    .Where(row => row.SelectedWallType != null)
                    .Select(row => row.SelectedWallType.WallTypeId));
            var wallType = WallTypes.FirstOrDefault(wt => !selectedIds.Contains(wt.WallTypeId));
            if(wallType != null) {
                Exclusions.Add(new WallTypeExclusionRowViewModel(wallType));
            }
        }

        private void RemoveWallType(WallTypeExclusionRowViewModel row) {
            if(row != null) {
                Exclusions.Remove(row);
            }
        }

        /// <summary>
        /// Сохраняет выбранные типоразмеры стен в конфигурацию проекта
        /// </summary>
        public void Save() {
            _pluginConfig.ExcludedWallTypeIds = Exclusions
                .Where(row => row.SelectedWallType != null)
                .Select(row => row.SelectedWallType.WallTypeId)
                .Distinct()
                .ToList();
            _pluginConfig.SaveProjectConfig();
        }
    }
}
