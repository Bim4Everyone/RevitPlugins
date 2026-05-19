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

    private string _name;
    private SheetSetPropsVM _properties;
    private ObservableCollection<SheetVM> _sheetList = [];
    private ObservableCollection<PluginParamVM> _params = [];
    private SheetVM _selectedSheet;

    public SheetSetVM(
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory) {

        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;

        AddSheetCommand = RelayCommand.Create(AddSheet);
        RemoveSheetCommand = RelayCommand.Create<SheetVM>(RemoveSheet);

        AddSheetSetParamCommand = RelayCommand.Create(AddSheetSetParam);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand RemoveSheetCommand { get; }
    public ICommand AddSheetSetParamCommand { get; }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public SheetSetPropsVM Properties {
        get => _properties;
        set => RaiseAndSetIfChanged(ref _properties, value);
    }

    public ObservableCollection<SheetVM> SheetList {
        get => _sheetList;
        set => RaiseAndSetIfChanged(ref _sheetList, value);
    }

    public ObservableCollection<PluginParamVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    public SheetVM SelectedSheet {
        get => _selectedSheet;
        set => RaiseAndSetIfChanged(ref _selectedSheet, value);
    }


    internal void AddSheet() {
        SheetList.Add(
            new SheetVM(
                _revitRepository,
                _localizationService,
                _messageBoxService,
                _sheetSetVMFactory,
                _sheetSetDataFactory) { ModuleName = "Новый лист" });
    }

    internal void RemoveSheet(SheetVM sheet) {
        if(sheet != null && SheetList.Contains(sheet)) {
            if(SelectedSheet == sheet) {
                SelectedSheet = null;
            }
            SheetList.Remove(sheet);
        }
    }

    internal void AddSheetSetParam() {
        Params.Add(new StringParamVM() { ParamName = "Новый параметр" });
    }

    internal void RemoveParam(PluginParamVM pluginParam) {
        if(pluginParam != null && Params.Contains(pluginParam)) {
            Params.Remove(pluginParam);
        }
    }
}
