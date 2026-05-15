using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;

    private bool _isModuleCheck;
    private string _moduleName;
    private string _moduleComment;
    private string _moduleCode;
    private string _moduleErrors;

    private string _sheetName;
    private string _sheetSize;
    private string _sheetCoefficient;
    private Family _titleBlockFamily;
    private FamilySymbol _titleBlockType;
    private SheetPropsVM _properties;
    private ObservableCollection<SheetComponentVM> _sheetComponents = [];
    private List<FamilySymbol> _titleBlockTypes;

    public SheetVM(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;

        SelectTitleBlockFamilyCommand = RelayCommand.Create(SelectTitleBlockFamily);
        CreateSheetCommand = RelayCommand.Create(CreateComponent, ValidateModule);
    }

    public ICommand SelectTitleBlockFamilyCommand { get; }
    public ICommand CreateSheetCommand { get; }

    public bool IsModuleCheck {
        get => _isModuleCheck;
        set => RaiseAndSetIfChanged(ref _isModuleCheck, value);
    }

    public string ModuleName {
        get => _moduleName;
        set => RaiseAndSetIfChanged(ref _moduleName, value);
    }

    public string ModuleComment {
        get => _moduleComment;
        set => RaiseAndSetIfChanged(ref _moduleComment, value);
    }

    public string ModuleCode {
        get => _moduleCode;
        set => RaiseAndSetIfChanged(ref _moduleCode, value);
    }

    public string ModuleErrors {
        get => _moduleErrors;
        set => RaiseAndSetIfChanged(ref _moduleErrors, value);
    }


    public string SheetName {
        get => _sheetName;
        set => RaiseAndSetIfChanged(ref _sheetName, value);
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


    public void CreateComponent() { }

    public bool ValidateModule() {
        if(string.IsNullOrEmpty(SheetName)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetNameIsEmpty");
            return false;
        }
        if(!double.TryParse(SheetSize, out double sheetSizeAsDouble) || sheetSizeAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetSizeIsNotCorrect");
            return false;
        }
        if(!double.TryParse(SheetCoefficient, out double sheetCoefficientAsDouble) || sheetCoefficientAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetCoefficientIsNotCorrect");
            return false;
        }
        if(TitleBlockFamily is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TitleBlockFamilyIsNull");
            return false;
        }
        if(TitleBlockType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TitleBlockTypeIsNull");
            return false;
        }
        if(SheetComponents.FirstOrDefault(c => c.ModuleErrors != string.Empty) != null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ErrorInSheetComponents");
            return false;
        }

        ModuleErrors = string.Empty;
        return true;
    }
}
