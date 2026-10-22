using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;

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
        SelectAllSheetsCommand = RelayCommand.Create(SelectAllSheets);
        DeselectAllSheetsCommand = RelayCommand.Create(DeselectAllSheets);
        SelectSheetsWithoutErrorsCommand = RelayCommand.Create(SelectSheetsWithoutErrors);
    }

    public ICommand AddSheetCommand { get; }
    public ICommand RemoveSheetCommand { get; }

    /// <summary>
    /// Команда включения в генерацию всех листов (галочки модулей листов не меняются)
    /// </summary>
    public ICommand SelectAllSheetsCommand { get; }

    /// <summary>
    /// Команда исключения из генерации всех листов (галочки модулей листов не меняются)
    /// </summary>
    public ICommand DeselectAllSheetsCommand { get; }

    /// <summary>
    /// Команда включения в генерацию всех листов и модулей без ошибок и исключения остальных
    /// </summary>
    public ICommand SelectSheetsWithoutErrorsCommand { get; }

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

    private void SelectAllSheets() {
        foreach(var sheet in SheetList) {
            sheet.IsModuleCheck = true;
        }
    }

    private void DeselectAllSheets() {
        foreach(var sheet in SheetList) {
            sheet.IsModuleCheck = false;
        }
    }

    /// <summary>
    /// Отмечает все, что можно сгенерировать: модули без ошибок, затем листы без ошибок.
    /// Лист, у которого есть модули, но ни один из них не остался отмеченным, не отмечается
    /// </summary>
    private void SelectSheetsWithoutErrors() {
        foreach(var sheet in SheetList) {
            sheet.SelectComponentsWithoutErrors();

            // Ошибки неотмеченных модулей на ошибки листа не влияют
            bool hasCheckedComponents = sheet.SheetComponents.Count == 0
                || sheet.SheetComponents.Any(c => c.IsModuleCheck);
            sheet.IsModuleCheck = !sheet.HasErrors && hasCheckedComponents;
        }
    }

    public void ValidateAllSheets() {
        foreach(var sheet in SheetList) {
            foreach(var component in sheet.SheetComponents) {
                component.CanCreateComponent();
            }
            sheet.CanCreateComponent();
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
                TitleBlockFamilyFilter = new FiltrationComboBoxFilterListVM(SheetSetParams.Params, _stringParamSetService),
                TitleBlockTypeFilter = new FiltrationComboBoxFilterListVM(SheetSetParams.Params, _stringParamSetService),
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

    public void UpdateOnInitialization() {
        foreach(var sheet in SheetList) {
            sheet.UpdateDueParamNameChange();
            foreach(var component in sheet.SheetComponents) {
                component.UpdateDueParamNameChange();
            }
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
        if(pluginParam is SelectElemParamVM selectElemParam) {
            foreach(var sheet in SheetList) {
                sheet.ValidateAllProperties();
                foreach(var component in sheet.SheetComponents) {
                    component.ValidateAllProperties();
                }
                // От выбранного элемента зависит уровень, по которому фрагменты ищут родительский вид
                sheet.UpdateCalloutParents();
            }
            return;
        }

        if(pluginParam is not StringParamVM stringParam) {
            foreach(var sheet in SheetList) {
                sheet.CanCreateComponent();
                foreach(var component in sheet.SheetComponents) {
                    component.CanCreateComponent();
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
