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
        var vm = new ActiveDocSearchSetViewModel(_revitRepository, _filter, new InvertedRevitFilterGenerator());
        vm.Initialize();
        return vm;
    }

    protected override SearchSetViewModel GetStraightSearchSet() {
        var vm = new ActiveDocSearchSetViewModel(_revitRepository, _filter, new StraightRevitFilterGenerator());
        vm.Initialize();
        return vm;
    }
}
