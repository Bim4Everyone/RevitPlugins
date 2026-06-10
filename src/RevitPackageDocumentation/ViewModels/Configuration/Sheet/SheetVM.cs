using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;
internal class SheetVM : BaseParamContainerVM {
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly StringParamSetService _stringParamSetService;
    private readonly string _sheetCoefficientParamName = "А";
    private readonly string _sheetSizeParamName = "х";

    private bool _isModuleCheck;
    private string _moduleName;
    private string _moduleComment;
    private string _moduleCode;
    private string _moduleErrors;

    private SheetSetVM _sheetSet;
    private string _sheetNameFormula = string.Empty;
    private string _sheetName;
    private string _sheetSize;
    private string _sheetCoefficient;
    private Family _titleBlockFamily;
    private FamilySymbol _titleBlockType;
    private ObservableCollection<SheetComponentVM> _sheetComponents = [];
    private List<FamilySymbol> _titleBlockTypes;
    private bool _hasErrors;
    private ViewSheet _sheetInstance;

    public SheetVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetSetVM sheetSetVM,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory) : base(repository, stringParamSetService, sheetSetParams) {

        SheetSet = sheetSetVM;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        _stringParamSetService = stringParamSetService;

        SelectTitleBlockFamilyCommand = RelayCommand.Create(SelectTitleBlockFamily);
        CreateSheetCommand = RelayCommand.Create(CreateComponent, ValidateModule);

        AddComponentCommand = RelayCommand.Create<ComponentTypeItem>(AddComponent);
        RemoveComponentCommand = RelayCommand.Create<SheetComponentVM>(RemoveComponent);
    }

    public ICommand SelectTitleBlockFamilyCommand { get; }
    public ICommand CreateSheetCommand { get; }

    public ICommand AddComponentCommand { get; }
    public ICommand RemoveComponentCommand { get; }


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

    public bool HasErrors {
        get => _hasErrors;
        set => RaiseAndSetIfChanged(ref _hasErrors, value);
    }


    public SheetSetVM SheetSet {
        get => _sheetSet;
        set => RaiseAndSetIfChanged(ref _sheetSet, value);
    }

    public string SheetNameFormula {
        get => _sheetNameFormula;
        set => RaiseAndSetIfChanged(ref _sheetNameFormula, value);
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

    public ObservableCollection<SheetComponentVM> SheetComponents {
        get => _sheetComponents;
        set => RaiseAndSetIfChanged(ref _sheetComponents, value);
    }

    public ViewSheet SheetInstance {
        get => _sheetInstance;
        set => RaiseAndSetIfChanged(ref _sheetInstance, value);
    }

    private void SelectTitleBlockFamily() {
        TitleBlockType = null;
        SetTitleBlockTypes(TitleBlockFamily);
    }

    public void SetTitleBlockTypes(Family titleBlockFamily) {
        TitleBlockTypes = titleBlockFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => Repository.Document.GetElement(id) as FamilySymbol)
            ?.ToList();
    }

    internal void RemoveComponent(SheetComponentVM sheetComponent) {
        if(sheetComponent != null && SheetComponents.Contains(sheetComponent)) {
            SheetComponents.Remove(sheetComponent);
        }
    }

    private void AddComponent(ComponentTypeItem selectedComponentType) {
        if(selectedComponentType?.ComponentType == null)
            return;

        try {
            var componentData = _sheetSetDataFactory.CreateComponentData(selectedComponentType.ComponentType);
            if(componentData == null)
                return;

            var component = _sheetSetVMFactory.CreateComponentVM(SheetSet, this, componentData);
            component.IsModuleCheck = true;
            SheetComponents.Add(component);
        } catch(Exception) {
            _messageBoxService.Show("An error occurred while adding the component!", "Error");
        }
    }

    public void CreateComponent() { }

    public bool ValidateModule() {
        if(string.IsNullOrEmpty(SheetNameFormula)) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetNameIsEmpty");
            HasErrors = true;
            return false;
        }
        if(!double.TryParse(SheetSize, out double sheetSizeAsDouble) || sheetSizeAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetSizeIsNotCorrect");
            HasErrors = true;
            return false;
        }
        if(!double.TryParse(SheetCoefficient, out double sheetCoefficientAsDouble) || sheetCoefficientAsDouble < 1) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.SheetCoefficientIsNotCorrect");
            HasErrors = true;
            return false;
        }
        if(TitleBlockFamily is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TitleBlockFamilyIsNull");
            HasErrors = true;
            return false;
        }
        if(TitleBlockType is null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.TitleBlockTypeIsNull");
            HasErrors = true;
            return false;
        }
        if(SheetComponents.FirstOrDefault(c => c.ModuleErrors != string.Empty) != null) {
            ModuleErrors = _localizationService.GetLocalizedString("MainWindow.ErrorInSheetComponents");
            HasErrors = true;
            return false;
        }

        HasErrors = false;
        ModuleErrors = string.Empty;
        return true;
    }

    public void Process() {
        SheetInstance = null;
        SheetInstance = Repository.GetSheetByName(SheetName);

        if(SheetInstance is null) {
            try {
                SheetInstance = ViewSheet.Create(Repository.Document, TitleBlockType.Id);
                SheetInstance.Name = SheetName;

                var titleBlock = Repository.GetTitleBlocks(SheetInstance);

                double.TryParse(SheetSize, out double sheetSize);
                titleBlock.LookupParameter(_sheetSizeParamName).Set(sheetSize);

                double.TryParse(SheetCoefficient, out double sheetCoefficient);
                titleBlock.LookupParameter(_sheetCoefficientParamName).Set(sheetCoefficient);

                SetCustomParams(SheetInstance);

                Repository.Document.Regenerate();
            } catch(Exception) { }
        }

        foreach(var component in SheetComponents.Where(c => c.IsModuleCheck).ToList()) {
            component.Process();
        }
    }
}
