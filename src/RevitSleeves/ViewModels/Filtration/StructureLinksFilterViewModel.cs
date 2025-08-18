using dosymep.SimpleServices;

using RevitClashDetective.Models.FilterGenerators;
using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration {
    internal class StructureLinksFilterViewModel : FilterViewModel {
        public StructureLinksFilterViewModel(
            RevitRepository revitRepository,
            Filter filter,
            IMessageBoxService messageBoxService)
            : base(revitRepository, filter, messageBoxService) {
        }

        protected override SearchSetViewModel GetInvertedSearchSet() {
            var vm = new StructureLinksSearchSetViewModel(
                _revitRepository, _filter, new InvertedRevitFilterGenerator());
            vm.Initialize();
            return vm;
        }

        protected override SearchSetViewModel GetStraightSearchSet() {
            var vm = new StructureLinksSearchSetViewModel(
                _revitRepository, _filter, new StraightRevitFilterGenerator());
            vm.Initialize();
            return vm;
        }
    }
}
