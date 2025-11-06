using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.ViewModels.Links;
internal class LinksSelectorViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly IDocTypesHandler _bimModelPartsHandler;
    private readonly IDocTypesProvider _docTypesProvider;
    private readonly ILocalizationService _localization;
    private string _errorText;
    private bool _allLinksSelected;

    public LinksSelectorViewModel(
        RevitRepository revitRepository,
        IDocTypesHandler bimModelPartsHandler,
        IDocTypesProvider docTypesProvider,
        ILocalizationService localization) {

        _revitRepository = revitRepository
            ?? throw new ArgumentNullException(nameof(revitRepository));
        _bimModelPartsHandler = bimModelPartsHandler
            ?? throw new ArgumentNullException(nameof(bimModelPartsHandler));
        _docTypesProvider = docTypesProvider
            ?? throw new ArgumentNullException(nameof(docTypesProvider));
        _localization = localization
            ?? throw new ArgumentNullException(nameof(localization));
        Links = new ObservableCollection<LinkViewModel>(_revitRepository.GetAllRevitLinkTypes()
            .Where(doc => _docTypesProvider.GetDocTypes().Contains(_bimModelPartsHandler.GetDocType(doc)))
            .Select(link => new LinkViewModel(link, _localization))
            .OrderBy(vm => vm.Name));

        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }


    public ICommand AcceptViewCommand { get; }

    public ObservableCollection<LinkViewModel> Links { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    public bool AllLinksSelected {
        get => _allLinksSelected;
        set {
            RaiseAndSetIfChanged(ref _allLinksSelected, value);
            foreach(var link in Links) {
                link.IsSelected = value;
            }
        }
    }


    private void AcceptView() {
        _revitRepository.SetRevitLinkTypesToUse(Links
            .Where(vm => vm.IsSelected)
            .Select(vm => vm.GetLinkType()));
    }

    private bool CanAcceptView() {
        if(Links.All(link => !link.IsSelected)) {
            ErrorText = _localization.GetLocalizedString("LinksSelectorWindow.Validation.SelectAnyLink");
            return false;
        }

        ErrorText = null;
        return true;
    }
}
