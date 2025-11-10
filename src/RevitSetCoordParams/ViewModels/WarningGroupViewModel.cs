using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.ViewModels;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningGroupViewModel : BaseViewModel {

    private ObservableCollection<WarningElementViewModel> _warnings;
    private string _caption;
    private string _description;
    private string _warningQuantity;

    public IReadOnlyCollection<WarningElement> WarningElements { get; set; } = [];
    public ICommand ShowElementCommand { get; set; }

    public ObservableCollection<WarningElementViewModel> Warnings {
        get => _warnings;
        set => RaiseAndSetIfChanged(ref _warnings, value);
    }
    public string Caption {
        get => _caption;
        set => RaiseAndSetIfChanged(ref _caption, value);
    }
    public string Description {
        get => _description;
        set => RaiseAndSetIfChanged(ref _description, value);
    }
    public string WarningQuantity {
        get => _warningQuantity;
        set => RaiseAndSetIfChanged(ref _warningQuantity, value);
    }

    /// <summary>
    /// Метод загрузки окна
    /// </summary>    
    public void LoadView() {
        Warnings = new(GetWarningElementViewModel());
    }

    // Метод получения списка WarningElementViewModel
    private IEnumerable<WarningElementViewModel> GetWarningElementViewModel() {
        return WarningElements
            .Select(warningElement => new WarningElementViewModel(warningElement.RevitElement) {
                ShowElementCommand = ShowElementCommand
            });
    }
}
