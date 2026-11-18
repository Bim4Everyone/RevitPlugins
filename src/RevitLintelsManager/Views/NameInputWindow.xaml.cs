using System.Windows;
using System.Windows.Threading;

using dosymep.SimpleServices;

using RevitLintelsManager.ViewModels;

namespace RevitLintelsManager.Views;

internal partial class NameInputWindow {
    public NameInputWindow(ILoggerService loggerService, ISerializationService serializationService,
        ILanguageService languageService, ILocalizationService localizationService,
        IUIThemeService uiThemeService, IUIThemeUpdaterService themeUpdaterService)
        : base(loggerService, serializationService, languageService, localizationService,
            uiThemeService, themeUpdaterService) {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }

    public override string PluginName => nameof(RevitLintelsManager);
    public override string ProjectConfigName => nameof(NameInputWindow);

    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e) {
        if(e.OldValue is NameInputViewModel oldViewModel) {
            oldViewModel.AcceptRequested -= Accept;
        }
        if(e.NewValue is NameInputViewModel viewModel) {
            viewModel.AcceptRequested += Accept;
        }
    }

    private void Accept() {
        DialogResult = true;
    }

    private void OnLoaded(object sender, RoutedEventArgs e) {
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () => {
            if(IsLoaded && IsVisible) {
                SizeToContent = SizeToContent.Height;
                _nameTextBox.Focus();
                _nameTextBox.SelectAll();
            }
        });
    }
}
