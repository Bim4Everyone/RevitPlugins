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
            return new StructureLinksSearchSetViewModel(_revitRepository, _filter, new InvertedRevitFilterGenerator());
        }

        protected override SearchSetViewModel GetStraightSearchSet() {
            return new StructureLinksSearchSetViewModel(_revitRepository, _filter, new StraightRevitFilterGenerator());
        }
    }
}
