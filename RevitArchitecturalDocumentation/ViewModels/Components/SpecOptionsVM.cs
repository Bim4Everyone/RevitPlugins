using System.Collections.Generic;
using System.Linq;

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
        /// Метод перебирает все выбранные спеки во всех заданиях и собирает список параметров фильтрации. принадлежащий всем одновременно
        /// </summary>
        public void GetFilterNames(IEnumerable<TaskInfo> tasksForWork) {

            FilterNamesFromSpecs.Clear();
            foreach(TaskInfo task in tasksForWork) {

                foreach(SpecHelper spec in task.ListSpecHelpers) {
                    if(FilterNamesFromSpecs.Count == 0) {
                        FilterNamesFromSpecs.AddRange(spec.GetFilterNames());
                    } else {
                        FilterNamesFromSpecs = FilterNamesFromSpecs.Intersect(spec.GetFilterNames()).ToList();
                    }
                }
            }
        }

        public SpecOptions GetSpecOption() {

            SpecOptions specOptions = new SpecOptions(_pluginConfig, _revitRepository) {
                WorkWithSpecs = WorkWithSpecs,
                SelectedFilterNameForSpecs = SelectedFilterNameForSpecs,
            };
            specOptions.SaveConfig();

            return specOptions;
        }
    }
}
