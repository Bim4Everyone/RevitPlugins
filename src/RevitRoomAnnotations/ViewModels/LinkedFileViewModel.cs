using System;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitRoomAnnotations.Models;

namespace RevitRoomAnnotations.ViewModels;

internal class LinkedFileViewModel : BaseViewModel {
    private bool _isSelected;
    public LinkedFileViewModel(LinkedFile model) => Model = model ?? throw new ArgumentNullException(nameof(model));
    public LinkedFile Model { get; }
    public string Name => Model.Name;
    public bool IsLoaded => Model.IsLoaded;
    public RevitLinkInstance LinkInstance => Model.LinkInstance;

    public bool IsSelected {
        get => _isSelected;
        set => RaiseAndSetIfChanged(ref _isSelected, value);
    }
}
