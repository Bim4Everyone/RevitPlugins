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

/// <summary>
/// Комплект листов: список листов с их модулями и параметры комплекта, на которые ссылаются формулы
/// </summary>
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

    /// <summary>
    /// Команда добавления нового листа в комплект
    /// </summary>
    public ICommand AddSheetCommand { get; }

    /// <summary>
    /// Команда удаления листа из комплекта
    /// </summary>
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

    /// <summary>
    /// Имя комплекта (конфигурации)
    /// </summary>
    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Листы комплекта
    /// </summary>
    public ObservableCollection<SheetVM> SheetList {
        get => _sheetList;
        set => RaiseAndSetIfChanged(ref _sheetList, value);
    }

    /// <summary>
    /// Лист, открытый для редактирования в окне
    /// </summary>
    public SheetVM SelectedSheet {
        get => _selectedSheet;
        set => RaiseAndSetIfChanged(ref _selectedSheet, value);
    }

    /// <summary>
    /// Параметры комплекта, значения которых подставляются в формулы листов и модулей
    /// </summary>
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

    /// <summary>
    /// Повторно проверяет все листы и их модули
    /// </summary>
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
                ModuleName = _localizationService.GetLocalizedString("MainViewModel.NewSheetName"),
                SheetSize = "1",
                SheetCoefficient = "1",
                TitleBlockFamilyFilter = new FiltrationComboBoxFilterListVM(SheetSetParams.Params, _stringParamSetService),
                TitleBlockTypeFilter = new FiltrationComboBoxFilterListVM(SheetSetParams.Params, _stringParamSetService),
            });
    }

    /// <summary>
    /// Перемещает лист в списке комплекта на offset позиций. Порядок листов - порядок их создания
    /// </summary>
    public void MoveSheet(SheetVM sheet, int offset) {
        if(!CanMoveSheet(sheet, offset)) {
            return;
        }
        int index = SheetList.IndexOf(sheet);
        SheetList.Move(index, index + offset);
    }

    /// <summary>
    /// Проверяет, можно ли переместить лист на offset позиций, не выходя за границы списка
    /// </summary>
    public bool CanMoveSheet(SheetVM sheet, int offset) {
        int index = SheetList.IndexOf(sheet);
        return index >= 0 && index + offset >= 0 && index + offset < SheetList.Count;
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
    /// Пересчитывает значения всех формул листов и модулей после загрузки конфигурации
    /// </summary>
    public void UpdateOnInitialization() {
        RecalculateAllFormulas();
    }

    /// <summary>
    /// В случае изменения имени параметра нужно обойти все листы и их компоненты, и обновить привязки
    /// </summary>
    public void UpdateDueParamNameChange(PluginParamVM pluginParam) {
        if(pluginParam is not StringParamVM) {
            return;
        }
        RecalculateAllFormulas();
    }

    /// <summary>
    /// Обновляет листы и модули после изменения значения параметра комплекта:
    /// выбор элемента - повторная проверка и обновление родительских видов фрагментов,
    /// текстовый параметр - пересчет формул, которые на него ссылаются
    /// </summary>
    public void UpdateDueParamValueChange(PluginParamVM pluginParam) {
        if(pluginParam is SelectElemParamVM) {
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

    /// <summary>
    /// Пересчитывает значения всех формул листов и их модулей по текущим параметрам комплекта
    /// </summary>
    private void RecalculateAllFormulas() {
        foreach(var sheet in SheetList) {
            sheet.UpdateDueParamNameChange();
            foreach(var component in sheet.SheetComponents) {
                component.UpdateDueParamNameChange();
            }
        }
    }
}
