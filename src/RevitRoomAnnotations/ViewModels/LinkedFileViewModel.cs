using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitRoomAnnotations.ViewModels;

internal class LinkedFileViewModel : BaseViewModel {
    private bool _isLoaded;

    public LinkedFileViewModel(RevitLinkInstance linkInstance) {
        LinkInstance = linkInstance ?? throw new ArgumentNullException(nameof(linkInstance));
        IsLoaded = LinkInstance.GetLinkDocument() != null;
        IsSelected = false;
        Name = LinkInstance.Name;
    }

    public RevitLinkInstance LinkInstance { get; }

    public bool IsSelected { get; set; }
    public bool IsLoaded {
        get => _isLoaded;
        set => RaiseAndSetIfChanged(ref _isLoaded, value);
    }
    public string Name { get; set; }

}
