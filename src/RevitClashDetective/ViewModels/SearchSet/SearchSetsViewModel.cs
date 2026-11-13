using System;
using System.Windows.Input;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;

namespace RevitClashDetective.ViewModels.SearchSet;
internal class SearchSetsViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localization;
    private readonly SearchSetViewModel _straightSearchSet;
    private readonly SearchSetViewModel _invertedSearchSet;
    private SearchSetViewModel _searchSet;

    public SearchSetsViewModel(RevitRepository revitRepository,
        ILocalizationService localization,
        string name,
        ILogicalFilterContext filterContext,
        IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _localization = localization ?? throw new ArgumentNullException(nameof(localization));
        if(filterContext is null) {
            throw new ArgumentNullException(nameof(filterContext));
        }
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

        _straightSearchSet = new SearchSetViewModel(_revitRepository, filterContext, false);
        _invertedSearchSet = new SearchSetViewModel(_revitRepository, filterContext, true);

        SearchSet = _straightSearchSet;
        Name = name;

        InversionChangedCommand = RelayCommand.Create(InversionChanged);
        ShowSetCommand = RelayCommand.Create(ShowSet);
        CloseCommand = RelayCommand.Create(Close);
    }


    public IMessageBoxService MessageBoxService { get; }

    public string Name { get; }

    public bool Inverted { get; set; }

    public ICommand InversionChangedCommand { get; }
    public ICommand CloseCommand { get; }
    public ICommand ShowSetCommand { get; }

    public SearchSetViewModel SearchSet {
        get => _searchSet;
        set => RaiseAndSetIfChanged(ref _searchSet, value);
    }

    private void InversionChanged() {
        SearchSet = Inverted ? _invertedSearchSet : _straightSearchSet;
        ShowSet();
    }

    private void Close() {
        _revitRepository.DoAction(ExecuteCreateFiltersCommand);
    }

    private void ExecuteCreateFiltersCommand() {
        var command = new CreateFiltersCommand();
        command.ExecuteCommand(_revitRepository.UiApplication, Name);
    }

    /// <summary>
    /// Показывает текущий набор на виде коллизий, скрывая элементы противоположного набора.
    /// </summary>
    private void ShowSet() {
        var setToHide = Inverted ? _straightSearchSet : _invertedSearchSet;
        try {
            _revitRepository.ShowElements(setToHide.GetRevitFilter(), SearchSet.GetCategories());
        } catch(InvalidOperationException ex) {
            MessageBoxService.Show(
                ex.Message,
                _localization.GetLocalizedString("BIM"),
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }
}
