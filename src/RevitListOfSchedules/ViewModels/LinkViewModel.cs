using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;
internal class LinkViewModel : BaseViewModel {

    private readonly LinkTypeElement _linkElement;
    private readonly ILocalizationService _localizationService;
    private bool _isChecked;
    private bool _isLoaded;
    private string _statusString;

    public LinkViewModel(LinkTypeElement linkElement, ILocalizationService localizationService) {
        _linkElement = linkElement;
        _localizationService = localizationService;
        SetStatus();
        _statusString = GetStringStatus();
    }

    public ElementId Id => _linkElement.Id;
    public string Name => _linkElement.Name;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value, nameof(IsChecked));
    }
    public bool IsLoaded {
        get => _isLoaded;
        set => RaiseAndSetIfChanged(ref _isLoaded, value, nameof(IsLoaded));
    }
    public string StatusString {
        get => _statusString;
        set => RaiseAndSetIfChanged(ref _statusString, value, nameof(StatusString));
    }

    public void ReloadLinkType() {
        _linkElement.Reload();
        SetStatus();
    }

    public bool CanReloadLinkType() {
        return _isChecked && _linkElement.RevitLink.GetLinkedFileStatus() != LinkedFileStatus.InClosedWorkset;
    }

    private void SetStatus() {
        IsLoaded = _linkElement.RevitLink.GetLinkedFileStatus() is LinkedFileStatus.Loaded;
        StatusString = GetStringStatus();
    }

    private string GetStringStatus() {
        return _isLoaded ? _localizationService.GetLocalizedString("MainWindow.StatusTextLoaded")
            : _localizationService.GetLocalizedString("MainWindow.StatusTextUnloaded");
    }
}
