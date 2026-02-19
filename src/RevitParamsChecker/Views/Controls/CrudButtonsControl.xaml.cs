using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Controls;

internal partial class CrudButtonsControl {
    public CrudButtonsControl()
        : base() {
        InitializeComponent();
    }

    public CrudButtonsControl(
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

    public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
        nameof(AddCommand),
        typeof(ICommand),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CopyCommandProperty = DependencyProperty.Register(
        nameof(CopyCommand),
        typeof(ICommand),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RenameCommandProperty = DependencyProperty.Register(
        nameof(RenameCommand),
        typeof(ICommand),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
        nameof(RemoveCommand),
        typeof(ICommand),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CopyCommandParameterProperty = DependencyProperty.Register(
        nameof(CopyCommandParameter),
        typeof(object),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RenameCommandParameterProperty = DependencyProperty.Register(
        nameof(RenameCommandParameter),
        typeof(object),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RemoveCommandParameterProperty = DependencyProperty.Register(
        nameof(RemoveCommandParameter),
        typeof(object),
        typeof(CrudButtonsControl),
        new PropertyMetadata(null));

    public object RemoveCommandParameter {
        get => (object) GetValue(RemoveCommandParameterProperty);
        set => SetValue(RemoveCommandParameterProperty, value);
    }

    public object RenameCommandParameter {
        get => (object) GetValue(RenameCommandParameterProperty);
        set => SetValue(RenameCommandParameterProperty, value);
    }

    public object CopyCommandParameter {
        get => (object) GetValue(CopyCommandParameterProperty);
        set => SetValue(CopyCommandParameterProperty, value);
    }

    public ICommand RemoveCommand {
        get => (ICommand) GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public ICommand RenameCommand {
        get => (ICommand) GetValue(RenameCommandProperty);
        set => SetValue(RenameCommandProperty, value);
    }

    public ICommand CopyCommand {
        get => (ICommand) GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    public ICommand AddCommand {
        get => (ICommand) GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }
}
