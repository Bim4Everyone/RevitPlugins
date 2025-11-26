using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitRooms.ViewModels;

namespace RevitRooms.Views;
/// <summary>
/// Interaction logic for NumberingOrderSelectWindow.xaml
/// </summary>
public partial class NumberingOrderSelectWindow {
    public NumberingOrderSelectWindow(
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

    public override string PluginName => nameof(RevitRooms);
    public override string ProjectConfigName => nameof(NumberingOrderSelectWindow);

    private void ButtonOK_Click(object sender, RoutedEventArgs e) {
        DialogResult = true;
    }

    private void ButtonCancel_Click(object sender, RoutedEventArgs e) {
        DialogResult = false;
    }
}

internal class NumberingOrderSelectViewModel : BaseViewModel {
    private ObservableCollection<NumberingOrderViewModel> _numberingOrders = [];

    public NumberingOrderSelectViewModel() {
        SelectCommand = new RelayCommand(Select, CanSelect);
    }

    public ICommand SelectCommand { get; set; }
    public ObservableCollection<NumberingOrderViewModel> NumberingOrders {
        get => _numberingOrders;
        set => RaiseAndSetIfChanged(ref _numberingOrders, value);
    }

    private void Select(object parameter) {

    }

    private bool CanSelect(object parameter) {
        return NumberingOrders.Any(item => item.IsSelected);
    }
}
