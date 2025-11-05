using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using RevitSetCoordParams.Models;

namespace RevitSetCoordParams.ViewModels;
internal class WarningGroupViewModel {

    public IReadOnlyCollection<WarningElement> WarningElements { get; set; }
    public ICommand ShowElementCommand { get; set; }
    public ObservableCollection<WarningElementViewModel> Warnings => new(GetWarningElementViewModel());
    public string Caption { get; set; }
    public string Description { get; set; }
    public string WarningQuantity { get; set; }

    // Метод получения списка WarningElementViewModel
    private IEnumerable<WarningElementViewModel> GetWarningElementViewModel() {
        return WarningElements
            .Select(warningElement => new WarningElementViewModel(warningElement.RevitElement) {
                ShowElementCommand = ShowElementCommand
            });
    }
}
