using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Results;

internal partial class GroupsEditorControl {
    public GroupsEditorControl() {
        InitializeComponent();
    }

    public GroupsEditorControl(
        ILoggerService loggerService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IUIThemeService uiThemeService,
        IUIThemeUpdaterService themeUpdaterService)
        : base(
            loggerService,
            languageService,
            localizationService,
            uiThemeService,
            themeUpdaterService) {
        InitializeComponent();
    }

    public static readonly DependencyProperty UpdateCommandProperty = DependencyProperty.Register(
        nameof(UpdateCommand),
        typeof(ICommand),
        typeof(GroupsEditorControl),
        new PropertyMetadata(default(ICommand)));

    public ICommand UpdateCommand {
        get => (ICommand) GetValue(UpdateCommandProperty);
        set => SetValue(UpdateCommandProperty, value);
    }
}
