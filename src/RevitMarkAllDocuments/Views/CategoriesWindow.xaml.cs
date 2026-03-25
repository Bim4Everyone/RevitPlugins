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
using Wpf.Ui.Abstractions;

namespace RevitMarkAllDocuments.Views
{
    /// <summary>
    /// Interaction logic for CategoriesWindow.xaml
    /// </summary>
    public partial class CategoriesWindow
    {
        public CategoriesWindow(ILoggerService loggerService,
                                ISerializationService serializationService,
                                ILanguageService languageService,
                                ILocalizationService localizationService,
                                IUIThemeService uiThemeService,
                                IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
            serializationService,
            languageService, localizationService,
            uiThemeService, themeUpdaterService) {
            InitializeComponent();
        }
    }
}
