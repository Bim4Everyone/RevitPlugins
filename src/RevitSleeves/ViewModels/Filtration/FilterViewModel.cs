using System;
using System.Linq;
using System.Windows.Input;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.FilterModel;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal abstract class FilterViewModel : BaseViewModel {
    protected readonly RevitRepository _revitRepository;
    protected readonly Filter _filter;
    protected readonly SearchSetViewModel _straightSearchSet;
    protected readonly SearchSetViewModel _invertedSearchSet;
    private SearchSetViewModel _activeSearchSet;
    private bool _inverted;

    protected FilterViewModel(RevitRepository revitRepository, Filter filter, IMessageBoxService messageBoxService) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _filter = filter ?? throw new ArgumentNullException(nameof(filter));
        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));

        _straightSearchSet = GetStraightSearchSet();
        _invertedSearchSet = GetInvertedSearchSet();
        ActiveSearchSet = _straightSearchSet;

        InversionChangedCommand = RelayCommand.Create(InversionChanged);
        CloseCommand = RelayCommand.Create(Close);
        ShowSetCommand = RelayCommand.Create(ShowSet);
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

    protected abstract SearchSetViewModel GetStraightSearchSet();

    protected abstract SearchSetViewModel GetInvertedSearchSet();

    private void ShowSet() {
        SearchSetViewModel invertedSet;
        if(Inverted) {
            invertedSet = _straightSearchSet;
        } else {
            invertedSet = _invertedSearchSet;
        }
        HideSet(invertedSet);
    }

    private void HideSet(SearchSetViewModel setToHide) {
        try {
            _revitRepository.GetClashRevitRepository().ShowElements(
                setToHide.Filter.GetRevitFilter(_revitRepository.Document, setToHide.FilterGenerator),
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

    private void InversionChanged() {
        if(Inverted) {
            ActiveSearchSet = _invertedSearchSet;
        } else {
            ActiveSearchSet = _straightSearchSet;
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
