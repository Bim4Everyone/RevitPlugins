using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;

namespace RevitArchitecturalDocumentation.ViewModels.Components {
    internal class ViewOptionsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;


        private List<ViewFamilyType> _viewFamilyTypes;
        private ViewFamilyType _selectedViewFamilyType;
        private string _selectedViewFamilyTypeName;
        private List<ElementType> _viewportTypes;
        private bool _workWithViews = true;
        private string _selectedViewportTypeName;
        private string _viewNamePrefix = string.Empty;
        private ElementType _selectedViewportType;


        public ViewOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, ViewOptions viewOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            WorkWithViews = viewOptions.WorkWithViews;
            ViewNamePrefix = viewOptions.ViewNamePrefix;

            LoadConfig();

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
            ViewportTypes = _revitRepository.ViewportTypes;

            SelectedViewFamilyType = ViewFamilyTypes.FirstOrDefault(a => a.Name.Equals(SelectedViewFamilyTypeName));
            SelectedViewportType = ViewportTypes.FirstOrDefault(a => a.Name.Equals(SelectedViewportTypeName));
        }


        public bool WorkWithViews {
            get => _workWithViews;
            set => this.RaiseAndSetIfChanged(ref _workWithViews, value);
        }

        public List<ViewFamilyType> ViewFamilyTypes {
            get => _viewFamilyTypes;
            set => this.RaiseAndSetIfChanged(ref _viewFamilyTypes, value);
        }

        public ViewFamilyType SelectedViewFamilyType {
            get => _selectedViewFamilyType;
            set => this.RaiseAndSetIfChanged(ref _selectedViewFamilyType, value);
        }

        public string SelectedViewFamilyTypeName {
            get => _selectedViewFamilyTypeName;
            set => this.RaiseAndSetIfChanged(ref _selectedViewFamilyTypeName, value);
        }


        public List<ElementType> ViewportTypes {
            get => _viewportTypes;
            set => this.RaiseAndSetIfChanged(ref _viewportTypes, value);
        }

        public ElementType SelectedViewportType {
            get => _selectedViewportType;
            set => this.RaiseAndSetIfChanged(ref _selectedViewportType, value);
        }

        public string SelectedViewportTypeName {
            get => _selectedViewportTypeName;
            set => this.RaiseAndSetIfChanged(ref _selectedViewportTypeName, value);
        }

        public string ViewNamePrefix {
            get => _viewNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _viewNamePrefix, value);
        }


        




        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        private void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithViews = settings.WorkWithViews;
            SelectedViewFamilyTypeName = settings.SelectedViewFamilyTypeName;
            SelectedViewportTypeName = settings.SelectedViewportTypeName;
            ViewNamePrefix = settings.ViewNamePrefix;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithViews = WorkWithViews;
            settings.SelectedViewFamilyTypeName = SelectedViewFamilyType.Name;
            settings.SelectedViewportTypeName = SelectedViewportType.Name;
            settings.ViewNamePrefix = ViewNamePrefix;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
