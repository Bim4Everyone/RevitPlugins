using System.Collections;
using System.Windows;
using System.Windows.Input;

using dosymep.SimpleServices;

namespace RevitParamsChecker.Views.Controls;

internal partial class EditableListControl {
    public EditableListControl()
        : base() {
        InitializeComponent();
    }

    public EditableListControl(
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

    public static readonly DependencyProperty ListLabelProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(string),
        typeof(EditableListControl),
        new PropertyMetadata(default(string)));

    public static readonly DependencyProperty AddCommandProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RenameCommandProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CopyCommandProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RemoveCommandProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ListItemsSourceProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(IEnumerable),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ListSelectedItemProperty = DependencyProperty.Register(
        nameof(ListLabel),
        typeof(object),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public string ListLabel {
        get => (string) GetValue(ListLabelProperty);
        set => SetValue(ListLabelProperty, value);
    }

    public ICommand AddCommand {
        get => (ICommand) GetValue(AddCommandProperty);
        set => SetValue(AddCommandProperty, value);
    }

    public ICommand RenameCommand {
        get => (ICommand) GetValue(RenameCommandProperty);
        set => SetValue(RenameCommandProperty, value);
    }

    public ICommand CopyCommand {
        get => (ICommand) GetValue(CopyCommandProperty);
        set => SetValue(CopyCommandProperty, value);
    }

    public ICommand RemoveCommand {
        get => (ICommand) GetValue(RemoveCommandProperty);
        set => SetValue(RemoveCommandProperty, value);
    }

    public IEnumerable ListItemsSource {
        get => (IEnumerable) GetValue(ListItemsSourceProperty);
        set => SetValue(ListItemsSourceProperty, value);
    }

    public object ListSelectedItem {
        get => (object) GetValue(ListSelectedItemProperty);
        set => SetValue(ListSelectedItemProperty, value);
    }
}
