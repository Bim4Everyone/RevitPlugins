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

internal partial class DocumentsPage {
    public DocumentsPage(MainViewModel viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService,
               languageService, localizationService,
               uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.DocumentsPageViewModel;
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
        ChangeSelected(true);
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
        ChangeSelected(false);
    }

    private void ChangeSelected(bool state) {
        var dataGrid = (DataGrid) FindName("Documents");
        var docs = dataGrid.SelectedItems;
        foreach(DocumentViewModel doc in docs) {
            doc.IsChecked = state;
        }
    }
}
