using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Options;

namespace RevitArchitecturalDocumentation.ViewModels.Components {
    internal class ViewOptionsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private bool _workWithViews;
        private List<ViewFamilyType> _viewFamilyTypes;
        private ViewFamilyType _selectedViewFamilyType;
        private List<ElementType> _viewportTypes;
        private ElementType _selectedViewportType;
        private string _viewNamePrefix = string.Empty;


        public ViewOptionsVM(PluginConfig pluginConfig, RevitRepository revitRepository, ViewOptions viewOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            WorkWithViews = viewOptions.WorkWithViews;
            ViewNamePrefix = viewOptions.ViewNamePrefix;

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
            ViewportTypes = _revitRepository.ViewportTypes;
            SelectedViewFamilyType = ViewFamilyTypes?.FirstOrDefault(a => a.Name.Equals(viewOptions.SelectedViewFamilyTypeName));
            SelectedViewportType = ViewportTypes?.FirstOrDefault(a => a.Name.Equals(viewOptions.SelectedViewportTypeName));
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

        public List<ElementType> ViewportTypes {
            get => _viewportTypes;
            set => this.RaiseAndSetIfChanged(ref _viewportTypes, value);
        }

        public ElementType SelectedViewportType {
            get => _selectedViewportType;
            set => this.RaiseAndSetIfChanged(ref _selectedViewportType, value);
        }

        public string ViewNamePrefix {
            get => _viewNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _viewNamePrefix, value);
        }


        public ViewOptions GetViewOption() {

            ViewOptions viewOptions = new ViewOptions(_pluginConfig, _revitRepository) {
                WorkWithViews = WorkWithViews,
                SelectedViewFamilyType = SelectedViewFamilyType,
                SelectedViewFamilyTypeName = SelectedViewFamilyType.Name,
                SelectedViewportType = SelectedViewportType,
                SelectedViewportTypeName = SelectedViewportType.Name,
                ViewNamePrefix = ViewNamePrefix
            };

            viewOptions.SaveConfig();

            return viewOptions;
        }
    }
}
