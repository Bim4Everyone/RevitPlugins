using System;
using System.Windows.Input;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterGenerators;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig {
    /// <summary>
    /// Модель представления для проверки настроек фильтрации инженерных элементов, для которых будут создаваться задания на отверстия.
    /// </summary>
    internal class MepCategoryFilterViewModel : BaseViewModel {
        private readonly RevitRepository _revitRepository;

        private Filter _linearElementsFilter;
        private SearchSetViewModel _searchSetLinearElements;
        private SearchSetViewModel _straightSearchSetLinearElements;
        private SearchSetViewModel _invertedSearchSetLinearElements;

        private Filter _nonLinearElementsFilter;
        private SearchSetViewModel _searchSetNonLinearElements;
        private SearchSetViewModel _straightSearchSetNonLinearElements;
        private SearchSetViewModel _invertedSearchSetNonLinearElements;


        /// <summary>
        /// Конструктор модели представления для проверки настроек фильтрации инженерных элементов, для которых будут создаваться задания на отверстия.
        /// </summary>
        /// <param name="revitRepository">Репозиторий активного документа, в котором находятся элементы инженерных систем</param>
        /// <param name="linearElementsFilter">Фильтр для линейных элементов инженерных систем (воздуховоды, трубы и т.п.)</param>
        /// <param name="nonLinearElementsFilter">Фильтр для нелинейных элементов инженерных систем (соединительные детали воздуховодов, соединительные детали трубопроводов и т.п.)</param>
        public MepCategoryFilterViewModel(RevitRepository revitRepository, Filter linearElementsFilter, Filter nonLinearElementsFilter) {
            _revitRepository = revitRepository;

            _linearElementsFilter = linearElementsFilter;
            _straightSearchSetLinearElements = new SearchSetViewModel(_revitRepository, _linearElementsFilter, new StraightRevitFilterGenerator());
            _invertedSearchSetLinearElements = new SearchSetViewModel(_revitRepository, _linearElementsFilter, new InvertedRevitFilterGenerator());
            LinearElementsSearchSet = _straightSearchSetLinearElements;

            _nonLinearElementsFilter = nonLinearElementsFilter;
            _straightSearchSetNonLinearElements = new SearchSetViewModel(_revitRepository, _nonLinearElementsFilter, new StraightRevitFilterGenerator());
            _invertedSearchSetNonLinearElements = new SearchSetViewModel(_revitRepository, _nonLinearElementsFilter, new InvertedRevitFilterGenerator());
            NonLinearElementsSearchSet = _straightSearchSetNonLinearElements;

            InversionChangedCommand = new RelayCommand(InversionChanged);
            CloseCommand = new RelayCommand(Close);
        }


        public bool Inverted { get; set; }

        public ICommand InversionChangedCommand { get; }
        public ICommand CloseCommand { get; }

        public SearchSetViewModel LinearElementsSearchSet {
            get => _searchSetLinearElements;
            set => RaiseAndSetIfChanged(ref _searchSetLinearElements, value);
        }

        public SearchSetViewModel NonLinearElementsSearchSet {
            get => _searchSetNonLinearElements;
            set => RaiseAndSetIfChanged(ref _searchSetNonLinearElements, value);
        }


        private void InversionChanged(object p) {
            if(Inverted) {
                LinearElementsSearchSet = _invertedSearchSetLinearElements;
                NonLinearElementsSearchSet = _invertedSearchSetNonLinearElements;
            } else {
                LinearElementsSearchSet = _straightSearchSetLinearElements;
                NonLinearElementsSearchSet = _straightSearchSetNonLinearElements;
            }
        }

        private void Close(object p) {
            Action action = () => {
                var command = new SetOpeningTasksPlacementConfigCmd();
                command.ExecuteCommand(_revitRepository.UIApplication);
            };
            _revitRepository.DoAction(action);
        }
    }
}
