using Autodesk.Revit.DB;

namespace RevitArchitecturalDocumentation.Models.Options {
    internal sealed class SheetOptions {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public SheetOptions(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            LoadConfig();
        }

        public bool WorkWithSheets { get; set; }
        public FamilySymbol SelectedTitleBlock { get; set; }
        public string SelectedTitleBlockName { get; set; }
        public string SheetNamePrefix { get; set; }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        public void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithSheets = settings.WorkWithSheets;
            SheetNamePrefix = settings.SheetNamePrefix;
            SelectedTitleBlockName = settings.SelectedTitleBlockName;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        public void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithSheets = WorkWithSheets;
            settings.SheetNamePrefix = SheetNamePrefix;
            settings.SelectedTitleBlockName = SelectedTitleBlockName;

            _pluginConfig.SaveProjectConfig();
        }
    }
}
