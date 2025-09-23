using System.Windows;
using System.Windows.Data;

using dosymep.SimpleServices;

using RevitAxonometryViews.ViewModels;

namespace RevitAxonometryViews.Views;
public partial class MainWindow {
    protected CollectionViewSource SystemsCollection;

    /// <summary>
    /// Иницализирует главное окно плагина.
    /// </summary>
    public MainWindow(
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


    // override-ы нужны потому что мы удалили конфиг, в котором переопределяются эти вещи
    public override string PluginName => nameof(RevitAxonometryViews);

    public override string ProjectConfigName => nameof(MainWindow);

    private void Button_Click_Ok(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }

    private void CheckBox_Checked(object sender, RoutedEventArgs e) {
        ChangeSelected(true);
    }

    private void CheckBox_Unchecked(object sender, RoutedEventArgs e) {
        ChangeSelected(false);
    }

    private void ChangeSelected(bool state) {
        foreach(HvacSystemViewModel hvacSystem in dgSystems.SelectedItems) {
            hvacSystem.IsSelected = state;
        }
    }
}

