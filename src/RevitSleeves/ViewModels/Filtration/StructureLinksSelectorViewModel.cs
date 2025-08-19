using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitSleeves.Models;

namespace RevitSleeves.ViewModels.Filtration;
internal class StructureLinksSelectorViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private string _errorText;


    public StructureLinksSelectorViewModel(
        RevitRepository revitRepository,
        ILocalizationService localizationService) {

        _revitRepository = revitRepository
            ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _localizationService = localizationService
            ?? throw new System.ArgumentNullException(nameof(localizationService));
        Links = [];

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(() => { }, CanAcceptView);
    }


    public ICommand AcceptViewCommand { get; }

    public ICommand LoadViewCommand { get; }

    public ObservableCollection<LinkViewModel> Links { get; }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    private void LoadView() {
        var linkTypes = _revitRepository.GetStructureLinkTypes();
        foreach(var linkType in linkTypes) {
            Links.Add(new LinkViewModel(_localizationService, linkType));
        }
    }

    private bool CanAcceptView() {
        if(Links.All(link => !link.IsSelected)) {
            ErrorText = _localizationService.GetLocalizedString("LinksSelectorWindow.Validation.SelectAnyLink");
            return false;
        }

        ErrorText = null;
        return true;
    }
}
