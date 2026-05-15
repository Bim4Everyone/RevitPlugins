using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM : BaseViewModel {
    private readonly RevitRepository _revitRepository;

    private string _name;
    private string _sheetSize;
    private string _sheetCoefficient;
    private Family _titleBlockFamily;
    private FamilySymbol _titleBlockType;
    private SheetPropsVM _properties;
    private ObservableCollection<SheetComponentVM> _sheetComponents = [];
    private List<FamilySymbol> _titleBlockTypes;

    public SheetVM(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
        SelectTitleBlockFamilyCommand = RelayCommand.Create(SelectTitleBlockFamily);
    }

    public ICommand SelectTitleBlockFamilyCommand { get; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public string SheetSize {
        get => _sheetSize;
        set => RaiseAndSetIfChanged(ref _sheetSize, value);
    }

    public string SheetCoefficient {
        get => _sheetCoefficient;
        set => RaiseAndSetIfChanged(ref _sheetCoefficient, value);
    }


    public List<FamilySymbol> TitleBlockTypes {
        get => _titleBlockTypes;
        set => RaiseAndSetIfChanged(ref _titleBlockTypes, value);
    }

    public Family TitleBlockFamily {
        get => _titleBlockFamily;
        set => RaiseAndSetIfChanged(ref _titleBlockFamily, value);
    }

    public FamilySymbol TitleBlockType {
        get => _titleBlockType;
        set => RaiseAndSetIfChanged(ref _titleBlockType, value);
    }

    public SheetPropsVM Properties {
        get => _properties;
        set => RaiseAndSetIfChanged(ref _properties, value);
    }

    public ObservableCollection<SheetComponentVM> SheetComponents {
        get => _sheetComponents;
        set => RaiseAndSetIfChanged(ref _sheetComponents, value);
    }


    private void SelectTitleBlockFamily() {
        TitleBlockType = null;
        SetTitleBlockTypes(TitleBlockFamily);
    }

    public void SetTitleBlockTypes(Family titleBlockFamily) {
        TitleBlockTypes = titleBlockFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => _revitRepository.Document.GetElement(id) as FamilySymbol)
            ?.ToList();
    }


    internal void RemoveComponent(SheetComponentVM sheetComponent) {
        if(sheetComponent != null && SheetComponents.Contains(sheetComponent)) {
            SheetComponents.Remove(sheetComponent);
        }
    }
}
