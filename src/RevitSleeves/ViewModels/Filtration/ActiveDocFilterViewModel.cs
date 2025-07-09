using dosymep.SimpleServices;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal class ActiveDocFilterViewModel : FilterViewModel {
    public ActiveDocFilterViewModel(
        RevitRepository revitRepository,
        Filter filter,
        IMessageBoxService messageBoxService)
        : base(revitRepository, filter, messageBoxService) {
    }

    protected override SearchSetViewModel GetInvertedSearchSet() {
        return new ActiveDocSearchSetViewModel(_revitRepository, _filter, new InvertedRevitFilterGenerator());
    }

    protected override SearchSetViewModel GetStraightSearchSet() {
        return new ActiveDocSearchSetViewModel(_revitRepository, _filter, new StraightRevitFilterGenerator());
    }
}
