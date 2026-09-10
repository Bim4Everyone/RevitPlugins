using System;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Filtration;

namespace RevitOpeningPlacement.ViewModels.OpeningConfig;
internal class StructureCategoryFilterViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly SearchSetViewModel _straightSearchSetElements;
    private readonly SearchSetViewModel _invertedSearchSetElements;
    private SearchSetViewModel _searchSetElements;


    /// <summary>
    /// Конструктор модели представления для проверки настроек фильтрации элементов конструкций из связанных файлов,
    /// для которых будут создаваться задания на отверстия.
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа</param>
    /// <param name="structureElementsFilter">Фильтр для элементов конструкций</param>
    /// <param name="messageBoxService">Сервис вывода сообщений</param>
    public StructureCategoryFilterViewModel(
        RevitRepository revitRepository,
        CategoryFilter structureElementsFilter,
        IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        if(structureElementsFilter is null) {
            throw new ArgumentNullException(nameof(structureElementsFilter));
        }

        _straightSearchSetElements = new StructureLinksSearchSetViewModel(
            _revitRepository,
            structureElementsFilter,
            inverted: false);
        _invertedSearchSetElements = new StructureLinksSearchSetViewModel(
            _revitRepository,
            structureElementsFilter,
            inverted: true);
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
                setToHide.GetRevitFilter(),
                setToHide.GetCategories());
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
