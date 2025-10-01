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
internal class StructureCategoryFilterViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly Filter _elementsFilter;
    private readonly SearchSetViewModel _straightSearchSetElements;
    private readonly SearchSetViewModel _invertedSearchSetElements;
    private SearchSetViewModel _searchSetElements;


    /// <summary>
    /// Конструктор модели представления для проверки настроек фильтрации элементов конструкций из связанных файлов,
    /// для которых будут создаваться задания на отверстия.
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа</param>
    /// <param name="structureElementsFilter">Фильтр для элементов конструкций</param>
    public StructureCategoryFilterViewModel(RevitRepository revitRepository, Filter structureElementsFilter) {
        _revitRepository = revitRepository;
        MessageBoxService = GetPlatformService<IMessageBoxService>();

        _elementsFilter = structureElementsFilter;
        _straightSearchSetElements = new StructureLinksSearchSetViewModel(
            _revitRepository,
            _elementsFilter,
            new StraightRevitFilterGenerator());
        _invertedSearchSetElements = new StructureLinksSearchSetViewModel(
            _revitRepository,
            _elementsFilter,
            new InvertedRevitFilterGenerator());
        ElementsSearchSet = _straightSearchSetElements;

        InversionChangedCommand = RelayCommand.Create(InversionChanged);
        CloseCommand = RelayCommand.Create(Close);
        ShowSetCommand = RelayCommand.Create(ShowSet);
    }


    public bool Inverted { get; set; }

    public ICommand InversionChangedCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ShowSetCommand { get; }
    public IMessageBoxService MessageBoxService { get; }

    public SearchSetViewModel ElementsSearchSet {
        get => _searchSetElements;
        set => RaiseAndSetIfChanged(ref _searchSetElements, value);
    }


    private void InversionChanged() {
        ElementsSearchSet = Inverted ? _invertedSearchSetElements : _straightSearchSetElements;
        ShowSet();
    }

    private void ShowSet() {
        var invertedSet = Inverted ? _straightSearchSetElements : _invertedSearchSetElements;
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
