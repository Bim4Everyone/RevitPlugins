using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;

namespace RevitArchitecturalDocumentation.ViewModels.Components {
    internal class SpecOptionsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private bool _workWithSpecs = true;
        private string _selectedFilterNameForSpecs;
        private List<string> _filterNamesFromSpecs = new List<string>();

        public SpecOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, SpecOptions specOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            WorkWithSpecs = specOptions.WorkWithSpecs;
            SelectedFilterNameForSpecs = specOptions.SelectedFilterNameForSpecs;
        }

        public bool WorkWithSpecs {
            get => _workWithSpecs;
            set => this.RaiseAndSetIfChanged(ref _workWithSpecs, value);
        }

        public List<string> FilterNamesFromSpecs {
            get => _filterNamesFromSpecs;
            set => this.RaiseAndSetIfChanged(ref _filterNamesFromSpecs, value);
        }

        public string SelectedFilterNameForSpecs {
            get => _selectedFilterNameForSpecs;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterNameForSpecs, value);
        }

        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        public void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithSpecs = settings.WorkWithSpecs;
            SelectedFilterNameForSpecs = settings.SelectedFilterNameForSpecs;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        public void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithSpecs = WorkWithSpecs;
            settings.SelectedFilterNameForSpecs = SelectedFilterNameForSpecs;

            _pluginConfig.SaveProjectConfig();
        }

        public SpecOptions GetSpecOption() {
            return new SpecOptions() {
                WorkWithSpecs = WorkWithSpecs,
                SelectedFilterNameForSpecs = SelectedFilterNameForSpecs,
            };
        }
    }
}
