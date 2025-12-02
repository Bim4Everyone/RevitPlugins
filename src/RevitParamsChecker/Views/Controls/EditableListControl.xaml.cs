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

    public static readonly DependencyProperty AddItemCommandProperty = DependencyProperty.Register(
        nameof(AddItemCommand),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RenameItemCommandProperty = DependencyProperty.Register(
        nameof(RenameItemCommand),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty CopyItemCommandProperty = DependencyProperty.Register(
        nameof(CopyItemCommand),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty RemoveItemsCommandProperty = DependencyProperty.Register(
        nameof(RemoveItemsCommand),
        typeof(ICommand),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ListItemsSourceProperty = DependencyProperty.Register(
        nameof(ListItemsSource),
        typeof(IEnumerable),
        typeof(EditableListControl),
        new PropertyMetadata(null));

    public static readonly DependencyProperty ListSelectedItemProperty = DependencyProperty.Register(
        nameof(ListSelectedItem),
        typeof(object),
        typeof(EditableListControl),
        new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string ListLabel {
        get => (string) GetValue(ListLabelProperty);
        set => SetValue(ListLabelProperty, value);
    }

    public ICommand AddItemCommand {
        get => (ICommand) GetValue(AddItemCommandProperty);
        set => SetValue(AddItemCommandProperty, value);
    }

    public ICommand RenameItemCommand {
        get => (ICommand) GetValue(RenameItemCommandProperty);
        set => SetValue(RenameItemCommandProperty, value);
    }

    public ICommand CopyItemCommand {
        get => (ICommand) GetValue(CopyItemCommandProperty);
        set => SetValue(CopyItemCommandProperty, value);
    }

    public ICommand RemoveItemsCommand {
        get => (ICommand) GetValue(RemoveItemsCommandProperty);
        set => SetValue(RemoveItemsCommandProperty, value);
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
