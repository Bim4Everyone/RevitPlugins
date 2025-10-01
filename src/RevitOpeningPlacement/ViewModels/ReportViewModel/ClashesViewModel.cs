using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.OpeningPlacement;

namespace RevitOpeningPlacement.ViewModels.ReportViewModel;
internal class ClashesViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    public ClashesViewModel(RevitRepository revitRepository, IEnumerable<UnplacedClashModel> clashes) {
        _revitRepository = revitRepository;
        Clashes = new ObservableCollection<ClashViewModel>(clashes.Select(item => new ClashViewModel(item)));
        ClashesViewSource = new CollectionViewSource() { Source = Clashes };

        SelectCommand = RelayCommand.Create<ClashViewModel>(Select, CanSelect);
    }

    public ObservableCollection<ClashViewModel> Clashes { get; }
    public CollectionViewSource ClashesViewSource { get; }
    public ICommand SelectCommand { get; }


    private void Select(ClashViewModel clash) {
        var elements = new[]{
            clash.Clash.MainElement,
            clash.Clash.OtherElement
        };
        _revitRepository.SelectAndShowElement(elements);
    }

    private bool CanSelect(ClashViewModel p) {
        return p != null;
    }
}
