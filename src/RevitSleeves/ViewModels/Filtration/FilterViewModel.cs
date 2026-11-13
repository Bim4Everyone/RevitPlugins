using System;
using System.Collections.Generic;
using System.Windows.Input;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;
using RevitSleeves.Services.Core;

using DocInfo = RevitClashDetective.Models.DocInfo;

namespace RevitSleeves.ViewModels.Filtration;

internal class FilterViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILogicalFilterContext _filterContext;
    private readonly ICollection<DocInfo> _searchTargets;
    private SearchSetViewModel _activeSearchSet;
    private bool _inverted;

    public FilterViewModel(
        RevitRepository revitRepository,
        ILogicalFilterContext filterContext,
        ICollection<DocInfo> searchTargets,
        IMessageBoxService messageBoxService) {

        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filterContext = filterContext ?? throw new ArgumentNullException(nameof(filterContext));
        _searchTargets = searchTargets ?? throw new ArgumentNullException(nameof(searchTargets));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

        InversionChangedCommand = RelayCommand.Create(InversionChanged);
        CloseCommand = RelayCommand.Create(Close);
        ShowSetCommand = RelayCommand.Create(ShowSet);

        Initialize();
    }


    public ICommand InversionChangedCommand { get; }

    public ICommand CloseCommand { get; }

    public ICommand ShowSetCommand { get; }

    public IMessageBoxService MessageBoxService { get; }

    public bool Inverted {
        get => _inverted;
        set => RaiseAndSetIfChanged(ref _inverted, value);
    }

    public SearchSetViewModel ActiveSearchSet {
        get => _activeSearchSet;
        set => RaiseAndSetIfChanged(ref _activeSearchSet, value);
    }

    private SearchSetViewModel StraightSearchSet { get; set; }

    private SearchSetViewModel InvertedSearchSet { get; set; }


    private void Initialize() {
        StraightSearchSet = new SearchSetViewModel(_revitRepository, _filterContext, _searchTargets, inverted: false);
        InvertedSearchSet = new SearchSetViewModel(_revitRepository, _filterContext, _searchTargets, inverted: true);
        ActiveSearchSet = StraightSearchSet;
    }

    private void ShowSet() {
        SearchSetViewModel setToHide;
        if(Inverted) {
            setToHide = StraightSearchSet;
        } else {
            setToHide = InvertedSearchSet;
        }
        HideSet(setToHide);
    }

    private void HideSet(SearchSetViewModel setToHide) {
        try {
            _revitRepository.GetClashRevitRepository().ShowElements(
                setToHide.GetRevitFilter(),
                setToHide.GetCategories());
        } catch(InvalidOperationException ex) {
            MessageBoxService.Show(
                ex.Message,
                NamesProvider.BIM,
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error,
                System.Windows.MessageBoxResult.OK);
        }
    }

    private void InversionChanged() {
        if(Inverted) {
            ActiveSearchSet = InvertedSearchSet;
        } else {
            ActiveSearchSet = StraightSearchSet;
        }
        ShowSet();
    }

    private void Close() {
        Action action = () => {
            var command = new SleevesSettingsCommand();
            command.ExecuteCommand(_revitRepository.UIApplication);
        };
        _revitRepository.GetClashRevitRepository().DoAction(action);
    }
}
