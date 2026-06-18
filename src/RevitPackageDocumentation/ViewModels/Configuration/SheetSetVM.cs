using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration;
internal class SheetSetVM : BaseViewModel {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly StringParamSetService _stringParamSetService;

    private string _name;
    private ObservableCollection<SheetVM> _sheetList = [];
    private SheetVM _selectedSheet;
    private SheetSetParametersListVM _sheetSetParams;

    public SheetSetVM(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory,
        StringParamSetService stringParamSetService) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        _stringParamSetService = stringParamSetService;

        AddSheetCommand = RelayCommand.Create(AddSheet);
        RemoveSheetCommand = RelayCommand.Create<SheetVM>(RemoveSheet);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand RemoveSheetCommand { get; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public ObservableCollection<SheetVM> SheetList {
        get => _sheetList;
        set => RaiseAndSetIfChanged(ref _sheetList, value);
    }

    public SheetVM SelectedSheet {
        get => _selectedSheet;
        set => RaiseAndSetIfChanged(ref _selectedSheet, value);
    }

    public SheetSetParametersListVM SheetSetParams {
        get => _sheetSetParams;
        set => RaiseAndSetIfChanged(ref _sheetSetParams, value);
    }

    public void ValidateAllSheets() {
        foreach(var sheet in SheetList) {
            foreach(var component in sheet.SheetComponents) {
                component.ValidateModule();
            }
            sheet.ValidateModule();
        }
    }

    internal void AddSheet() {
        SheetList.Add(
            new SheetVM(
                _revitRepository,
                _stringParamSetService,
                SheetSetParams.Params,
                this,
                _localizationService,
                _messageBoxService,
                _sheetSetVMFactory,
                _sheetSetDataFactory) {
                IsModuleCheck = true,
                ModuleName = "Новый лист",
                SheetSize = "1",
                SheetCoefficient = "1",
            });
    }

    internal void RemoveSheet(SheetVM sheet) {
        if(sheet != null && SheetList.Contains(sheet)) {
            if(SelectedSheet == sheet) {
                SelectedSheet = null;
            }
            SheetList.Remove(sheet);
        }
    }


    /// <summary>
    /// В случае изменения имени параметра нужно обойти все листы и их компоненты, и обновить привязки
    /// </summary>
    public void UpdateDueParamNameChange(PluginParamVM pluginParam) {
        if(pluginParam is not StringParamVM stringParam) {
            return;
        }
        foreach(var sheet in SheetList) {
            sheet.UpdateDueParamNameChange();
            foreach(var component in sheet.SheetComponents) {
                component.UpdateDueParamNameChange();
            }
        }
    }

    public void UpdateDueParamValueChange(PluginParamVM pluginParam) {
        if(pluginParam is not StringParamVM stringParam) {
            foreach(var sheet in SheetList) {
                sheet.ValidateModule();
                foreach(var component in sheet.SheetComponents) {
                    component.ValidateModule();
                }
            }
            return;
        }
        foreach(var sheet in SheetList) {
            sheet.UpdateDueParamValueChange(stringParam);
            foreach(var component in sheet.SheetComponents) {
                component.UpdateDueParamValueChange(stringParam);
            }
        }
    }
}
