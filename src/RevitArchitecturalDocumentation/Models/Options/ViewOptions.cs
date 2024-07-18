using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models.Options {
    internal class ViewOptions {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public ViewOptions(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            LoadConfig();
        }

        public bool WorkWithViews { get; set; }
        public ViewFamilyType SelectedViewFamilyType { get; set; }
        public string SelectedViewFamilyTypeName { get; set; }
        public ElementType SelectedViewportType { get; set; }
        public string SelectedViewportTypeName { get; set; }
        public string ViewNamePrefix { get; set; }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        public void LoadConfig() {

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
        public void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithViews = WorkWithViews;
            settings.SelectedViewFamilyTypeName = SelectedViewFamilyTypeName;
            settings.SelectedViewportTypeName = SelectedViewportTypeName;
            settings.ViewNamePrefix = ViewNamePrefix;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
