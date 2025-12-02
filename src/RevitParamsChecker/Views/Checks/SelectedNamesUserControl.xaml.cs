using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Checks;

internal partial class SelectedNamesUserControl {
    public SelectedNamesUserControl()
        : base() {
        InitializeComponent();
    }

    public SelectedNamesUserControl(
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

    public static readonly DependencyProperty SelectedNamesProperty = DependencyProperty.Register(
        nameof(SelectedNames),
        typeof(ICollection<string>),
        typeof(SelectedNamesUserControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public static readonly DependencyProperty SelectNamesCommandProperty = DependencyProperty.Register(
        nameof(SelectNamesCommand),
        typeof(ICommand),
        typeof(SelectedNamesUserControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SelectNamesCommandParameterProperty = DependencyProperty.Register(
        nameof(SelectNamesCommandParameter),
        typeof(object),
        typeof(SelectedNamesUserControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty SelectNamesPromptProperty = DependencyProperty.Register(
        nameof(SelectNamesPrompt),
        typeof(string),
        typeof(SelectedNamesUserControl),
        new PropertyMetadata(default(string)));

    public ICollection<string> SelectedNames {
        get => (ICollection<string>) GetValue(SelectedNamesProperty);
        set => SetValue(SelectedNamesProperty, value);
    }

    public ICommand SelectNamesCommand {
        get => (ICommand) GetValue(SelectNamesCommandProperty);
        set => SetValue(SelectNamesCommandProperty, value);
    }

    public object SelectNamesCommandParameter {
        get => (object) GetValue(SelectNamesCommandParameterProperty);
        set => SetValue(SelectNamesCommandParameterProperty, value);
    }

    public string SelectNamesPrompt {
        get => (string) GetValue(SelectNamesPromptProperty);
        set => SetValue(SelectNamesPromptProperty, value);
    }
}
