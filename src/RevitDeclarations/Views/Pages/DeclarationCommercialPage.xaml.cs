using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;

namespace RevitDeclarations.Views;
internal partial class DeclarationCommercialPage {
    public DeclarationCommercialPage(CommercialMainVM viewModel, ILoggerService loggerService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService) 
        : base(loggerService,
               languageService, localizationService,
               uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContext = viewModel.DeclarationViewModel;
    }

    private void IndentValidation(object sender, TextCompositionEventArgs e) {
        e.Handled = !e.Text.All(char.IsDigit);
    }

    private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e) {
        if(e.Key == Key.Space) {
            e.Handled = true;
        }
    }
}
