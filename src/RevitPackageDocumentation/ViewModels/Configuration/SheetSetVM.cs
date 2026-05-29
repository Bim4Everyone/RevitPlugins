using System.Collections.ObjectModel;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Parameters;

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
    private ObservableCollection<PluginParamVM> _params = [];
    private SheetVM _selectedSheet;

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

        AddSheetSetParamCommand = RelayCommand.Create<ComponentTypeItem>(AddSheetSetParam);
        RemoveSheetSetParamCommand = RelayCommand.Create<PluginParamVM>(RemoveSheetSetParam);

        //UpdateSheetSetParamCommand = RelayCommand.Create<PluginParamVM>(UpdateSheetSetParam);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand RemoveSheetCommand { get; }
    public ICommand AddSheetSetParamCommand { get; }
    public ICommand RemoveSheetSetParamCommand { get; }
    //public ICommand UpdateSheetSetParamCommand { get; }

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

    public ObservableCollection<PluginParamVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }


    internal void AddSheet() {
        SheetList.Add(
            new SheetVM(
                this,
                _revitRepository,
                _localizationService,
                _messageBoxService,
                _sheetSetVMFactory,
                _sheetSetDataFactory,
                _stringParamSetService) { ModuleName = "Новый лист" });
    }

    internal void RemoveSheet(SheetVM sheet) {
        if(sheet != null && SheetList.Contains(sheet)) {
            if(SelectedSheet == sheet) {
                SelectedSheet = null;
            }
            SheetList.Remove(sheet);
        }
    }

    private void AddSheetSetParam(ComponentTypeItem selectedSheetSetParamType) {
        if(selectedSheetSetParamType?.ComponentType == null)
            return;

        try {
            var paramData = _sheetSetDataFactory.CreatePluginParamData(selectedSheetSetParamType.ComponentType);
            if(paramData == null)
                return;

            var parameter = _sheetSetVMFactory.CreateParamVM(this, paramData);
            Params.Add(parameter);
        } catch(System.Exception) {
            _messageBoxService.Show("An error occurred while adding the parameter!", "Error");
        }
    }

    private void RemoveSheetSetParam(PluginParamVM pluginParam) {
        if(pluginParam != null && Params.Contains(pluginParam)) {
            Params.Remove(pluginParam);
        }
    }

    public void UpdateSheetSetParam(PluginParamVM pluginParam) {
        foreach(var sheet in SheetList) {
            sheet.UpdateSheetSetParam(pluginParam);
        }
    }
}
