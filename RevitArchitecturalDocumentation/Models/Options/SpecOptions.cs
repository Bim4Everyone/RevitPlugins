namespace RevitArchitecturalDocumentation.Models.Options {
    internal class SpecOptions {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        public SpecOptions(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;
            LoadConfig();
        }

        public bool WorkWithSpecs { get; set; }
        public string SelectedFilterNameForSpecs { get; set; }

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
    }
}
