using System;
using System.ComponentModel;
using System.Windows.Interop;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;
using dosymep.WPF.Views;

using pyRevitLabs.Json;

using Wpf.Ui.Controls;

namespace RevitPluginExample.Views {
    public partial class BaseWindow : FluentWindow {
        private readonly WindowInteropHelper _windowInteropHelper;

        public BaseWindow() {
            LanguageService = GetPlatformService<ILanguageService>();
            UIThemeService = GetPlatformService<IUIThemeService>();
            UIThemeUpdaterService = GetPlatformService<IUIThemeUpdaterService>();

            UIThemeService.UIThemeChanged += OnUIThemeChanged;

            _windowInteropHelper = new WindowInteropHelper(this) {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
        }

        /// <summary>
        /// Наименование плагина.
        /// </summary>
        public virtual string PluginName { get; }
        /// <summary>
        /// Наименование файла конфигурации.
        /// </summary>
        public virtual string ProjectConfigName { get; }
        /// <summary>
        /// Сервис локализации окон.
        /// </summary>
        public virtual ILocalizationService LocalizationService { get; set; }
        /// <summary>
        /// Предоставляет доступ к настройкам темы платформы.
        /// </summary>
        public virtual IUIThemeService UIThemeService { get; }
        /// <summary>
        /// Сервис по обновлению темы у окна.
        /// </summary>
        public virtual IUIThemeUpdaterService UIThemeUpdaterService { get; }
        /// <summary>
        /// Предоставляет доступ к текущему языку платформы.
        /// </summary>
        protected ILanguageService LanguageService { get; }


        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            LocalizationService?.SetLocalization(LanguageService.HostLanguage, this);

            base.OnSourceInitialized(e);

            var config = GetProjectConfig();
            if(config.WindowPlacement.HasValue) {
                this.SetPlacement(config.WindowPlacement.Value);
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            var config = GetProjectConfig();
            config.WindowPlacement = this.GetPlacement();
            config.SaveProjectConfig();
        }

        private void OnUIThemeChanged(UIThemes obj) {
            UpdateTheme();
        }

        public void UpdateTheme() {
            UIThemeUpdaterService.SetTheme(this, UIThemeService.HostTheme);
        }

        protected virtual BaseWindowConfig GetProjectConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(PluginName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(ProjectConfigName + ".json")
                .Build<BaseWindowConfig>();
        }

        public class BaseWindowConfig : ProjectConfig {
            [JsonIgnore] public override string ProjectConfigPath { get; set; }
            [JsonIgnore] public override IConfigSerializer Serializer { get; set; }

            public WINDOWPLACEMENT? WindowPlacement { get; set; }
        }
    }
}
