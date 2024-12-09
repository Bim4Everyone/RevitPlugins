using dosymep.SimpleServices;

using Wpf.Ui.Controls;

namespace RevitPluginExample.Views {
    public partial class BaseWindow : FluentWindow {

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
    }
}
