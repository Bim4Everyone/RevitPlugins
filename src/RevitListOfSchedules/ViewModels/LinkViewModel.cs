using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitListOfSchedules.Models;

namespace RevitListOfSchedules.ViewModels;
internal class LinkViewModel : BaseViewModel {
    private readonly LinkTypeElement _linkElement;
    private bool _isChecked;
    private bool _isLoaded;

    public LinkViewModel(LinkTypeElement linkElement) {
        _linkElement = linkElement;
        SetStatus();
        ReloadCommand = RelayCommand.Create(ReloadLinkType, CanReloadLinkType);
    }

    public ICommand ReloadCommand { get; set; }
    public ElementId Id => _linkElement.Id;
    public string Name => _linkElement.Name;
    public string FullName => _linkElement.FullName;

    public bool IsLoaded {
        get => _isLoaded;
        set => RaiseAndSetIfChanged(ref _isLoaded, value, nameof(IsLoaded));
    }

    public bool IsChecked {
        get => _isChecked;
        set => RaiseAndSetIfChanged(ref _isChecked, value, nameof(IsChecked));
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
    }
}
