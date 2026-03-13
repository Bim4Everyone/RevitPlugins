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
using System.Windows.Navigation;
using System.Windows.Shapes;

using dosymep.SimpleServices;

using RevitMarkAllDocuments.ViewModels;

namespace RevitMarkAllDocuments.Views;

internal partial class MarkSettingsPage {
    public MarkSettingsPage(DocumentsPageViewModel viewModel, 
                      ILoggerService loggerService,
                      ILanguageService languageService, 
                      ILocalizationService localizationService,
                      IUIThemeService uiThemeService, 
                      IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
               languageService, localizationService,
               uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel;
    }
}
