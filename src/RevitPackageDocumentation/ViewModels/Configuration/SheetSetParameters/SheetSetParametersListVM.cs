using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;
internal class SheetSetParametersListVM : BaseViewModel {
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly ILocalizationService _localizationService;

    private ObservableCollection<PluginParamVM> _params = [];
    private ObservableCollection<SelectElemParamVM> _selectElemParams = [];
    private string _sheetSetParamsErrors = string.Empty;

    public SheetSetParametersListVM(
        SheetSetVM sheetSet,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory,
        ILocalizationService localizationService) {
        SheetSet = sheetSet;
        _messageBoxService = messageBoxService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        _localizationService = localizationService;

        AddSheetSetParamCommand = RelayCommand.Create<ComponentTypeItem>(AddSheetSetParam);
        RemoveSheetSetParamCommand = RelayCommand.Create<PluginParamVM>(RemoveSheetSetParam);
    }

    public ICommand AddSheetSetParamCommand { get; }
    public ICommand RemoveSheetSetParamCommand { get; }

    public SheetSetVM SheetSet { get; }

    public ObservableCollection<PluginParamVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    public ObservableCollection<SelectElemParamVM> SelectElemParams {
        get => _selectElemParams;
        set => RaiseAndSetIfChanged(ref _selectElemParams, value);
    }

    public string SheetSetParamsErrors {
        get => _sheetSetParamsErrors;
        set => RaiseAndSetIfChanged(ref _sheetSetParamsErrors, value);
    }


    public bool ValidateParams() {
        if(Params?.Any(p => p.ErrorInParamName) == true) {
            SheetSetParamsErrors = _localizationService.GetLocalizedString("MainWindow.ErrorInSheetSetParamNames");
            return false;
        }
        if(Params?.Any(p => p.ErrorInParamValue) == true) {
            SheetSetParamsErrors = _localizationService.GetLocalizedString("MainWindow.ErrorInSheetSetParamValues");
            return false;
        }

        SheetSetParamsErrors = string.Empty;
        return true;
    }

    private void AddSheetSetParam(ComponentTypeItem selectedSheetSetParamType) {
        if(selectedSheetSetParamType?.ComponentType == null)
            return;

        try {
            var paramData = _sheetSetDataFactory.CreatePluginParamData(selectedSheetSetParamType.ComponentType);
            if(paramData == null)
                return;

            AddSheetSetParam(paramData);
        } catch(System.Exception) {
            _messageBoxService.Show("An error occurred while adding the parameter!", "Error");
        }
    }


    public void AddSheetSetParam(PluginParamData paramData) {
        var parameter = _sheetSetVMFactory.CreateParamVM(this, paramData);
        Params.Add(parameter);
        if(parameter is SelectElemParamVM selectParam) {
            SelectElemParams.Add(selectParam);
        }
        ValidateParams();
    }


    private void RemoveSheetSetParam(PluginParamVM pluginParam) {
        // Удаляем из списка параметров выбора
        if(pluginParam != null && SelectElemParams.Contains(pluginParam) && pluginParam is SelectElemParamVM selectParam) {
            // Проходимся по каждому компоненту, и если в них есть параметр SelectedSelectElemParam
            // и его значение pluginParam, то ставим null (у не выбранных листов он сам не сбрасывает)
            SheetSet.SheetList
                .SelectMany(s => s.SheetComponents)
                .Where(component => {
                    var prop = component.GetType().GetProperty("SelectedSelectElemParam");
                    return prop != null && prop.GetValue(component) == selectParam;
                })
                .ToList()
                .ForEach(component => {
                    var prop = component.GetType().GetProperty("SelectedSelectElemParam");
                    prop?.SetValue(component, null);
                });

            SelectElemParams.Remove(selectParam);
        }
        // Удаляем из общего списка параметров
        if(pluginParam != null && Params.Contains(pluginParam)) {
            Params.Remove(pluginParam);
        }
        ValidateParams();
        SheetSet.ValidateAllSheets();
    }
}
