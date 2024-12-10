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

using Wpf.Ui.Appearance;
using Wpf.Ui.Controls;

namespace RevitPluginExample.Views {
    public partial class BaseWindow : FluentWindow {
        private readonly WindowInteropHelper _windowInteropHelper;

        public BaseWindow() {
            LanguageService = GetPlatformService<ILanguageService>();

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
        /// Предоставляет доступ к текущему языку платформы.
        /// </summary>
        protected ILanguageService LanguageService { get; }

        private ApplicationTheme _currentTheme = ApplicationThemeManager.GetAppTheme();

        /// <summary>
        /// Переключает текущую тему между светлой и тёмной.
        /// </summary>
        public void ChangeTheme() {
            var newTheme = _currentTheme == ApplicationTheme.Dark ? ApplicationTheme.Light : ApplicationTheme.Dark;
            SetTheme(newTheme);
        }

        /// <summary>
        /// Устанавливает указанную тему.
        /// </summary>
        /// <param name="theme">Тема, которую нужно установить.</param>
        public void SetTheme(ApplicationTheme theme) {
            _currentTheme = theme;
            ApplicationThemeManager.Apply(_currentTheme, WindowBackdropType.Mica, true);
        }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            LocalizationService?.SetLocalization(System.Globalization.CultureInfo.CurrentUICulture, this);

            var config = GetProjectConfig();
            if(config.WindowPlacement.HasValue) {
                this.SetPlacement(config.WindowPlacement.Value);
            }

            SetTheme(_currentTheme);
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            var config = GetProjectConfig();
            config.WindowPlacement = this.GetPlacement();
            config.SaveProjectConfig();
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
