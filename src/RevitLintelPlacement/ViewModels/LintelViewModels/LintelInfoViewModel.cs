using System.ComponentModel.DataAnnotations;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitLintelPlacement.Models;

namespace RevitLintelPlacement.ViewModels;

internal class LintelInfoViewModel : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private ElementId _elementInWallId;
    private ElementInWallKind _elementInWallKind;
    private string _elementInWallName;
    private string _level;
    private ElementId _lintelId;
    private string _wallTypeName;

    public LintelInfoViewModel() {
        RotateLintelCommand = new RelayCommand(RotateLintel);
    }

    public LintelInfoViewModel(
        RevitRepository revitRepository,
        FamilyInstance lintel,
        FamilyInstance elementInWall) {
        _revitRepository = revitRepository;
        Level = _revitRepository.GetElementById(lintel.LevelId)?.Name;
        if(elementInWall != null) {
            ElementInWall = elementInWall;
            ElementInWallId = elementInWall.Id;
            ElementInWallName = $"{elementInWall.Symbol.Family.Name}: {elementInWall.Name}";
            WallTypeName = elementInWall.Host.Name;
            Level = _revitRepository.GetElementById(elementInWall.LevelId)?.Name;
            if(elementInWall.Symbol.Family.Name.ToLower()
               .Contains(_revitRepository.LintelsCommonConfig.HolesFilter.ToLower())) {
                ElementInWallKind = ElementInWallKind.Opening;
            } else if(elementInWall.Category.Id == _revitRepository.GetCategory(BuiltInCategory.OST_Doors).Id) {
                ElementInWallKind = ElementInWallKind.Door;
            } else if(elementInWall.Category.Id == new ElementId(BuiltInCategory.OST_Windows)) {
                ElementInWallKind = ElementInWallKind.Window;
            } else {
                ElementInWallKind = ElementInWallKind.None;
            }
        } else {
            ElementInWallKind = ElementInWallKind.None;
        }

        Lintel = lintel;
        LintelId = lintel.Id;
        RotateLintelCommand = new RelayCommand(RotateLintel, CanRotateLintel);
    }

    public string ElementInWallName {
        get => _elementInWallName;
        set => RaiseAndSetIfChanged(ref _elementInWallName, value);
    }

    public string WallTypeName {
        get => _wallTypeName;
        set => RaiseAndSetIfChanged(ref _wallTypeName, value);
    }

    public string Level {
        get => _level;
        set => RaiseAndSetIfChanged(ref _level, value);
    }

    public ElementInWallKind ElementInWallKind {
        get => _elementInWallKind;
        set => RaiseAndSetIfChanged(ref _elementInWallKind, value);
    }

    public ICommand RotateLintelCommand { get; set; }

    public ElementId LintelId {
        get => _lintelId;
        set => RaiseAndSetIfChanged(ref _lintelId, value);
    }

    public ElementId ElementInWallId {
        get => _elementInWallId;
        set => RaiseAndSetIfChanged(ref _elementInWallId, value);
    }

    public FamilyInstance Lintel { get; set; }
    public FamilyInstance ElementInWall { get; set; }

    private async void RotateLintel(object p) {
        await _revitRepository.MirrorLintel(Lintel, ElementInWall);
    }

    private bool CanRotateLintel(object p) {
        return ElementInWall != null && Lintel != null;
    }
}

public enum ElementInWallKind {
    [Display(Name = "Все категории")]
    All,

    [Display(Name = "Двери")]
    Door,

    [Display(Name = "Окна")]
    Window,

    [Display(Name = "Отверстия")]
    Opening,

    [Display(Name = "Без категории")]
    None
}
