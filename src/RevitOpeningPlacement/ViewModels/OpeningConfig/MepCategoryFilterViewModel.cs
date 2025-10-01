using System;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterGenerators;

using RevitClashDetective.Models.FilterModel;

using RevitOpeningPlacement.Models;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
/// <summary>
/// Модель представления для проверки настроек фильтрации инженерных элементов, для которых будут создаваться задания на отверстия.
/// </summary>
internal class MepCategoryFilterViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private readonly Filter _linearElementsFilter;
    private readonly SearchSetViewModel _straightSearchSetLinearElements;
    private readonly SearchSetViewModel _invertedSearchSetLinearElements;
    private SearchSetViewModel _searchSetLinearElements;

    private readonly Filter _nonLinearElementsFilter;
    private readonly SearchSetViewModel _straightSearchSetNonLinearElements;
    private readonly SearchSetViewModel _invertedSearchSetNonLinearElements;
    private SearchSetViewModel _searchSetNonLinearElements;


    /// <summary>
    /// Конструктор модели представления для проверки настроек фильтрации инженерных элементов, для которых будут создаваться задания на отверстия.
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа, в котором находятся элементы инженерных систем</param>
    /// <param name="linearElementsFilter">Фильтр для линейных элементов инженерных систем (воздуховоды, трубы и т.п.)</param>
    /// <param name="nonLinearElementsFilter">Фильтр для нелинейных элементов инженерных систем (соединительные детали воздуховодов, соединительные детали трубопроводов и т.п.)</param>
    public MepCategoryFilterViewModel(RevitRepository revitRepository, Filter linearElementsFilter, Filter nonLinearElementsFilter) {
        _revitRepository = revitRepository;
        MessageBoxService = GetPlatformService<IMessageBoxService>();

        _linearElementsFilter = linearElementsFilter;
        _straightSearchSetLinearElements = new ActiveDocSearchSetViewModel(_revitRepository, _linearElementsFilter, new StraightRevitFilterGenerator());
        _invertedSearchSetLinearElements = new ActiveDocSearchSetViewModel(_revitRepository, _linearElementsFilter, new InvertedRevitFilterGenerator());
        LinearElementsSearchSet = _straightSearchSetLinearElements;

        _nonLinearElementsFilter = nonLinearElementsFilter;
        _straightSearchSetNonLinearElements = new ActiveDocSearchSetViewModel(_revitRepository, _nonLinearElementsFilter, new StraightRevitFilterGenerator());
        _invertedSearchSetNonLinearElements = new ActiveDocSearchSetViewModel(_revitRepository, _nonLinearElementsFilter, new InvertedRevitFilterGenerator());
        NonLinearElementsSearchSet = _straightSearchSetNonLinearElements;

        InversionChangedCommand = RelayCommand.Create(InversionChanged);
        CloseCommand = RelayCommand.Create(Close);
        ShowLinearSetCommand = RelayCommand.Create(ShowLinearSet);
        ShowNonLinearSetCommand = RelayCommand.Create(ShowNonLinearSet);
    }


    public bool Inverted { get; set; }

    public ICommand InversionChangedCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ShowLinearSetCommand { get; }
    public ICommand ShowNonLinearSetCommand { get; }
    public IMessageBoxService MessageBoxService { get; }

    public SearchSetViewModel LinearElementsSearchSet {
        get => _searchSetLinearElements;
        set => RaiseAndSetIfChanged(ref _searchSetLinearElements, value);
    }

    public SearchSetViewModel NonLinearElementsSearchSet {
        get => _searchSetNonLinearElements;
        set => RaiseAndSetIfChanged(ref _searchSetNonLinearElements, value);
    }


    private void InversionChanged() {
        if(Inverted) {
            LinearElementsSearchSet = _invertedSearchSetLinearElements;
            NonLinearElementsSearchSet = _invertedSearchSetNonLinearElements;
        } else {
            LinearElementsSearchSet = _straightSearchSetLinearElements;
            NonLinearElementsSearchSet = _straightSearchSetNonLinearElements;
        }
        ShowLinearSet();
    }

    private void ShowLinearSet() {
        var invertedSet = Inverted ? _straightSearchSetLinearElements : _invertedSearchSetLinearElements;
        HideSet(invertedSet);
    }

    private void ShowNonLinearSet() {
        var invertedSet = Inverted ? _straightSearchSetNonLinearElements : _invertedSearchSetNonLinearElements;
        HideSet(invertedSet);
    }

    private void HideSet(SearchSetViewModel setToHide) {
        try {
            _revitRepository.GetClashRevitRepository().ShowElements(
                setToHide.Filter.GetRevitFilter(_revitRepository.Doc, setToHide.FilterGenerator),
                setToHide.Filter
                    .CategoryIds
                    .Select(c => c.AsBuiltInCategory())
                    .ToHashSet());
        } catch(InvalidOperationException ex) {
            MessageBoxService.Show(
                ex.Message,
                $"BIM",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }

    private void Close() {
        void action() {
            var command = new SetOpeningTasksPlacementConfigCmd();
            command.ExecuteCommand(_revitRepository.UIApplication);
        }
        _revitRepository.DoAction(action);
    }
}
