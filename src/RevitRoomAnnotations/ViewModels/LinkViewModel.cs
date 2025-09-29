using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitRoomAnnotations.Models;


namespace RevitRoomAnnotations.ViewModels;
internal class LinkViewModel : BaseViewModel {
    private readonly ILocalizationService _localizationService;
    private bool _isChecked;

    public LinkViewModel(LinkInstanceElement linkInstanceElement, ILocalizationService localizationService) {
        LinkInstanceElement = linkInstanceElement;
        _localizationService = localizationService;
    }

    public LinkInstanceElement LinkInstanceElement { get; }
    public string Name => LinkInstanceElement.Name;

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value, nameof(IsChecked));
    }
    public bool IsLoaded => GetStatus();
    public string StatusString => GetStringStatus();

    // Метод установки статуса
    private bool GetStatus() {
        return LinkInstanceElement.LinkedFileStatus is LinkedFileStatus.Loaded;
    }

    // Метод установки текстового статуса 
    private string GetStringStatus() {
        return IsLoaded
            ? _localizationService.GetLocalizedString("MainWindow.StatusTextLoaded")
            : _localizationService.GetLocalizedString("MainWindow.StatusTextUnloaded");
    }
}
