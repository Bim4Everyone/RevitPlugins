using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using dosymep.SimpleServices;

using RevitFinishing.ViewModels;

namespace RevitFinishing.Views
{
    /// <summary>
    /// Interaction logic for ErrorsWindow.xaml
    /// </summary>
    public partial class ErrorsWindow
    {
        public ErrorsWindow(
            ILoggerService loggerService,
            ISerializationService serializationService,
            ILanguageService languageService, ILocalizationService localizationService,
            IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
            : base(loggerService,
                   serializationService,
                   languageService, localizationService,
                   uiThemeService, themeUpdaterService) {
            InitializeComponent();
        }

        public override string PluginName => nameof(RevitFinishing);

        public override string ProjectConfigName => nameof(ErrorsWindow);
    }
}
