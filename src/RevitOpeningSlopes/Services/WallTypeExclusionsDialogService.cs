using System;
using System.Linq;
using System.Windows;

using RevitOpeningSlopes.Models;
using RevitOpeningSlopes.ViewModels;
using RevitOpeningSlopes.Views;

namespace RevitOpeningSlopes.Services {
    internal class WallTypeExclusionsDialogService {
        private readonly RevitRepository _revitRepository;
        private readonly PluginConfig _pluginConfig;
        private Window _owner;
        private bool _openExclusionsOnNextLoad;

        public WallTypeExclusionsDialogService(
            RevitRepository revitRepository,
            PluginConfig pluginConfig) {
            _revitRepository = revitRepository
                ?? throw new ArgumentNullException(nameof(revitRepository));
            _pluginConfig = pluginConfig
                ?? throw new ArgumentNullException(nameof(pluginConfig));
        }

        /// <summary>
        /// Открывает окно выбора исключаемых типоразмеров стен
        /// </summary>
        /// <returns>True, если пользователь сохранил изменения</returns>
        public bool ShowDialog(Window owner) {
            SelectionInModelRequested = false;
            var viewModel = new WallTypeExclusionsViewModel(
                _revitRepository,
                _pluginConfig);
            var window = new WallTypeExclusionsWindow(viewModel);
            _owner = owner;
            if(_owner != null) {
                window.Owner = _owner;
            }
            bool result = window.ShowDialog() == true;
            SelectionInModelRequested = window.SelectionInModelRequested;
            return result;
        }

        public bool SelectionInModelRequested { get; private set; }

        /// <summary>
        /// Закрывает основное окно перед выбором стен в модели
        /// </summary>
        public void CloseMainWindowForSelection() {
            if(_owner != null) {
                _owner.DialogResult = true;
                if(_owner.IsVisible) {
                    _owner.Close();
                }
            }
        }

        /// <summary>
        /// Запрашивает выбор стен в модели и сохраняет их типоразмеры
        /// </summary>
        public void SelectWallsInModel() {
            try {
                var selectedTypeIds = _revitRepository.SelectWallsOnView()
                    .Select(w => w.GetTypeId());
                _pluginConfig.ExcludedWallTypeIds = (
                    _pluginConfig.ExcludedWallTypeIds
                    ?? new System.Collections.Generic.List<Autodesk.Revit.DB.ElementId>())
                    .Concat(selectedTypeIds)
                    .Distinct()
                    .ToList();
                _pluginConfig.SaveProjectConfig();
            } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            } finally {
                SelectionInModelRequested = false;
                _openExclusionsOnNextLoad = true;
                _owner = null;
            }
        }

        /// <summary>
        /// Возвращает запрос повторного открытия окна исключений
        /// </summary>
        /// <returns>True, если окно нужно открыть</returns>
        public bool ConsumeOpenExclusionsRequest() {
            bool result = _openExclusionsOnNextLoad;
            _openExclusionsOnNextLoad = false;
            return result;
        }
    }
}
