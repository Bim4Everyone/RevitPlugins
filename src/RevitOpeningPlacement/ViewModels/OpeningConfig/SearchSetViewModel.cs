using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal abstract class SearchSetViewModel : BaseViewModel {
    private protected readonly RevitRepository _revitRepository;

    protected SearchSetViewModel(RevitRepository revitRepository, Filter filter, RevitFilterGenerator generator) {
        _revitRepository = revitRepository;
        FilterGenerator = generator;
        Filter = filter;
        InitializeGrid();
    }

    public RevitFilterGenerator FilterGenerator { get; }
    public Filter Filter { get; }
    public GridControlViewModel Grid { get; private protected set; }

    private protected abstract void InitializeGrid();
}
