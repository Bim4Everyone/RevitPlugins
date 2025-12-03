using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

namespace RevitParamsChecker.ViewModels.Results;

internal class CheckResultViewModel : BaseViewModel {
    private string _elementsFilter;

    public CheckResultViewModel() {
        // TODO
        Name = "Проверка 1";
        ElementResults = [new ElementResultViewModel()];
        SelectElementsCommand = RelayCommand.Create<IList>(SelectElements, CanSelectElements);
    }

    public ICommand SelectElementsCommand { get; }

    public string Name { get; }

    public ObservableCollection<RuleResultViewModel> RuleResults { get; }

    public ObservableCollection<ElementResultViewModel> ElementResults { get; }

    public string ElementsFilter {
        get => _elementsFilter;
        set => RaiseAndSetIfChanged(ref _elementsFilter, value);
    }

    private void SelectElements(IList items) {
        // TODO
    }

    private bool CanSelectElements(IList items) {
        return items != null && items.OfType<ElementResultViewModel>().Count() != 0;
    }
}
