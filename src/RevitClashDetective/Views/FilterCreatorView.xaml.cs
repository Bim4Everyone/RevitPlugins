using System.Windows;
using System.Windows.Controls;

using dosymep.SimpleServices;

namespace RevitClashDetective.Views;
public partial class FilterCreatorView {
    public FilterCreatorView(
    ILoggerService loggerService,
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

    public override string PluginName => nameof(RevitClashDetective);
    public override string ProjectConfigName => nameof(FilterCreatorView);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void CriterionControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        ((ContentControl) sender).Content = new CriterionView() { DataContext = ((ContentControl) sender).DataContext };
    }

    private void CategoryControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        ((ContentControl) sender).Content = new CategoryView() { DataContext = ((ContentControl) sender).DataContext };
    }
}
