using System;
using System.ComponentModel;
using System.Windows.Interop;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.ProjectConfigs;
using dosymep.Bim4Everyone.SimpleServices;
using dosymep.Serializers;
using dosymep.SimpleServices;
using dosymep.WPF.Views;

using Wpf.Ui.Controls;

namespace RevitPluginExample.Views {
    public partial class BaseWindow : FluentWindow {
        private readonly WindowInteropHelper _windowInteropHelper;

        public BaseWindow() {
            LocalizationService = ServicesProvider.GetPlatformService<ILocalizationService>();

            _windowInteropHelper = new WindowInteropHelper(this) {
                Owner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle
            };
        }

        public virtual string PluginName { get; }
        public virtual string ProjectConfigName { get; }
        public virtual ILocalizationService LocalizationService { get; set; }

        protected override void OnSourceInitialized(EventArgs e) {
            base.OnSourceInitialized(e);

            LocalizationService?.SetLocalization(System.Globalization.CultureInfo.CurrentUICulture, this);

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

        protected virtual PlatformWindowConfig GetProjectConfig() {
            return new ProjectConfigBuilder()
                .SetSerializer(new ConfigSerializer())
                .SetPluginName(PluginName)
                .SetRevitVersion(ModuleEnvironment.RevitVersion)
                .SetProjectConfigName(ProjectConfigName + ".json")
                .Build<PlatformWindowConfig>();
        }
    }
}
