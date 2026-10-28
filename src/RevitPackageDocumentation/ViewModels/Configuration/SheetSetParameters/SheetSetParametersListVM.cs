using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.Validation;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters;

/// <summary>
/// Список параметров комплекта. Параметры выбора элемента дополнительно хранятся в отдельном списке
/// для выпадающих списков привязки к объекту в модулях видов
/// </summary>
internal class SheetSetParametersListVM : ValidatableVM {
    // Имя свойства модуля вида, в котором хранится выбранный параметр привязки к объекту
    private const string _selectedSelectElemParamPropertyName = "SelectedSelectElemParam";

    private readonly IMessageBoxService _messageBoxService;
    private readonly ILocalizationService _localizationService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;

    private ObservableCollection<PluginParamVM> _params = [];
    private ObservableCollection<SelectElemParamVM> _selectElemParams = [];
    private string _moduleComment;

    public SheetSetParametersListVM(
        SheetSetVM sheetSet,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory,
        ILocalizationService localizationService)
        : base(localizationService) {
        SheetSet = sheetSet;
        _messageBoxService = messageBoxService;
        _localizationService = localizationService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        ValidateAllProperties();

        AddSheetSetParamCommand = RelayCommand.Create<ComponentTypeItem>(AddSheetSetParam);
        RemoveSheetSetParamCommand = RelayCommand.Create<PluginParamVM>(RemoveSheetSetParam);
    }

    /// <summary>
    /// Команда добавления параметра выбранного типа
    /// </summary>
    public ICommand AddSheetSetParamCommand { get; }

    /// <summary>
    /// Команда удаления параметра
    /// </summary>
    public ICommand RemoveSheetSetParamCommand { get; }

    /// <summary>
    /// Комплект, которому принадлежат параметры
    /// </summary>
    public SheetSetVM SheetSet { get; }

    /// <summary>
    /// Все параметры комплекта
    /// </summary>
    [ChildHasErrors(ErrorMessage = "Validation.ErrorInSheetSetParams")]
    public ObservableCollection<PluginParamVM> Params {
        get => _params;
        set => RaiseAndSetIfChanged(ref _params, value);
    }

    /// <summary>
    /// Параметры выбора элемента (подмножество Params)
    /// </summary>
    public ObservableCollection<SelectElemParamVM> SelectElemParams {
        get => _selectElemParams;
        set => RaiseAndSetIfChanged(ref _selectElemParams, value);
    }

    /// <summary>
    /// Комментарий к параметрам комплекта
    /// </summary>
    public string ModuleComment {
        get => _moduleComment;
        set => RaiseAndSetIfChanged(ref _moduleComment, value);
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
            _messageBoxService.Show(
                _localizationService.GetLocalizedString("MainViewModel.AddParameterError"),
                _localizationService.GetLocalizedString("MainViewModel.Error"));
        }
    }

    /// <summary>
    /// Создает параметр по данным конфигурации и добавляет его в списки
    /// </summary>
    public void AddSheetSetParam(PluginParamData paramData) {
        var parameter = _sheetSetVMFactory.CreateParamVM(this, paramData);
        Params.Add(parameter);
        if(parameter is SelectElemParamVM selectParam) {
            SelectElemParams.Add(selectParam);
        }
    }

    private void RemoveSheetSetParam(PluginParamVM pluginParam) {
        // Удаляем из списка параметров выбора
        if(pluginParam is SelectElemParamVM selectParam && SelectElemParams.Contains(selectParam)) {
            // Проходимся по каждому компоненту, и если в них есть параметр SelectedSelectElemParam
            // и его значение pluginParam, то ставим null (у не выбранных листов он сам не сбрасывает)
            foreach(var component in SheetSet.SheetList.SelectMany(s => s.SheetComponents)) {
                var prop = component.GetType().GetProperty(_selectedSelectElemParamPropertyName);
                if(prop != null && prop.GetValue(component) == selectParam) {
                    prop.SetValue(component, null);
                }
            }

            SelectElemParams.Remove(selectParam);
        }
        // Удаляем из общего списка параметров
        if(pluginParam != null) {
            Params.Remove(pluginParam);
        }
        SheetSet.ValidateAllSheets();
    }
}
