using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using DevExpress.Xpf.Core;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;

using pyRevitLabs.Json;

namespace dosymep.WPF.Views {
    public class ThemedPlatformWindow : ThemedWindow {
        private readonly WindowInteropHelper _windowInteropHelper;

        public ThemedPlatformWindow() {
            LanguageService = GetPlatformService<ILanguageService>();
            
            UIThemeService = GetPlatformService<IUIThemeService>();
            UIThemeUpdaterService = GetPlatformService<IUIThemeUpdaterService>();
            
            UIThemeService.UIThemeChanged += OnUIThemeChanged;
            
            _windowInteropHelper = new WindowInteropHelper(this) {
                Owner = Process.GetCurrentProcess().MainWindowHandle
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
        /// Предоставляет доступ к логгеру платформы.
        /// </summary>
        protected ILoggerService LoggerService => GetPlatformService<ILoggerService>();

        /// <summary>
        /// Предоставляет доступ к текущему языку платформы.
        /// </summary>
        protected ILanguageService LanguageService { get; }
        
        /// <summary>
        /// Предоставляет доступ к настройкам темы платформы.
        /// </summary>
        protected IUIThemeService UIThemeService { get; }
        
        /// <summary>
        /// Сервис по обновлению темы у окна.
        /// </summary>
        protected IUIThemeUpdaterService UIThemeUpdaterService { get; }

        protected T GetPlatformService<T>() {
            return ServicesProvider.GetPlatformService<T>();
        }

        protected override void OnSourceInitialized(EventArgs e) {
            UpdateTheme();
            LocalizationService?.SetLocalization(LanguageService.HostLanguage, this);
            
            base.OnSourceInitialized(e);
            
            PlatformWindowConfig config = GetProjectConfig();
            if(config.WindowPlacement.HasValue) {
                this.SetPlacement(config.WindowPlacement.Value);
            }
        }

        protected override void OnClosing(CancelEventArgs e) {
            base.OnClosing(e);

            UIThemeService.UIThemeChanged -= OnUIThemeChanged;

            PlatformWindowConfig config = GetProjectConfig();
            config.WindowPlacement = this.GetPlacement();
            config.SaveProjectConfig();
        }

        protected virtual PlatformWindowConfig GetProjectConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(PluginName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(ProjectConfigName + ".json")
                .Build<PlatformWindowConfig>();
        }

        private void OnUIThemeChanged(UIThemes obj) {
            UpdateTheme();
        }

        private void UpdateTheme() {
            UIThemeUpdaterService.SetTheme(this, UIThemeService.HostTheme);
        }
    }
}
