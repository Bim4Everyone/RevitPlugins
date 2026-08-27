using System.Windows;

using dosymep.SimpleServices;

namespace RevitClashDetective.Views.Common;

public partial class EntityNameView {
    public EntityNameView(
        ILoggerService loggerService,
        ISerializationService serializationService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            serializationService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }

    public override string PluginName => nameof(RevitClashDetective);
    public override string ProjectConfigName => nameof(EntityNameView);

    private void ButtonOk_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}
