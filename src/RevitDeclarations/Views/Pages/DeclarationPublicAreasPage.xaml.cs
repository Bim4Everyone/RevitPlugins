using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;

using RevitDeclarations.ViewModels;
using RevitDeclarations.ViewModels.DeclarationPageViewModels;

namespace RevitDeclarations.Views;
internal partial class DeclarationPublicAreasPage {
    public DeclarationPublicAreasPage(PublicAreasMainVM viewModel, ILoggerService loggerService,
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
