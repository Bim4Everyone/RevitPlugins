using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.Models.ScheduleFilters;
using RevitPackageDocumentation.ViewModels.Configuration;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;
    private readonly IRevitElementPickerService _revitElementPickerService;
    private readonly SheetSetConfig _sheetSetConfig;

    private SheetSetVM _currentSheetSet;

    private string _errorText;
    private string _sheetSetDataPath;

    private List<ComponentTypeItem> _sheetSetParamTypes;
    private ComponentTypeItem _selectedSheetSetParamType;
    private List<ComponentTypeItem> _componentTypes;
    private ComponentTypeItem _selectedComponentType;

    private List<ViewFamilyType> _planViewFamilyTypes;
    private List<ViewFamilyType> _sectionViewFamilyTypes;
    private List<ViewPlan> _planViewTemplates;
    private List<ViewPlan> _planViewsForDuplication;
    private List<ViewSection> _sectionViewTemplates;
    private List<ElementType> _viewportTypes;
    private List<ViewSchedule> _specsInPj;
    private List<TextNoteType> _textNoteTypes;
    private List<View> _legendsInProject;
    private List<Family> _titleBlockFamilies;
    private IList<ScheduleTypeInfo> _filterTypes;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        ILocalizationService localizationService,
        IOpenFileDialogService openFileDialogService,
        ISaveFileDialogService saveFileDialogService,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory,
        IRevitElementPickerService revitElementPickerService,
        SheetSetConfig sheetSetConfig) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;
        _revitElementPickerService = revitElementPickerService;
        _sheetSetConfig = sheetSetConfig;

        MessageBoxService = messageBoxService ?? throw new ArgumentNullException(nameof(messageBoxService));
        OpenFileDialogService = openFileDialogService ?? throw new ArgumentNullException(nameof(openFileDialogService));
        SaveFileDialogService = saveFileDialogService ?? throw new ArgumentNullException(nameof(saveFileDialogService));

        ImportCommand = RelayCommand.Create(ImportSheetSet);
        ExportCommand = RelayCommand.Create(ExportSheetSet);

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectElemForParamCommand = RelayCommand.Create<SelectElemParamVM>(SelectElemForParam);
    }

    /// <summary>
    /// Команда загрузки конфигурации комплекта из файла
    /// </summary>
    public ICommand ImportCommand { get; }

    /// <summary>
    /// Команда сохранения конфигурации комплекта в файл
    /// </summary>
    public ICommand ExportCommand { get; }

    /// <summary>
    /// Команда выбора элемента в модели для параметра комплекта "Выбор элемента"
    /// </summary>
    public ICommand SelectElemForParamCommand { get; }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }

    public IOpenFileDialogService OpenFileDialogService { get; }
    public ISaveFileDialogService SaveFileDialogService { get; }
    public IMessageBoxService MessageBoxService { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Загруженный комплект листов
    /// </summary>
    public SheetSetVM CurrentSheetSet {
        get => _currentSheetSet;
        set => RaiseAndSetIfChanged(ref _currentSheetSet, value);
    }

    /// <summary>
    /// Списки элементов проекта для выпадающих списков модулей (собираются при открытии окна)
    /// </summary>
    public List<ViewFamilyType> PlanViewFamilyTypes {
        get => _planViewFamilyTypes;
        set => RaiseAndSetIfChanged(ref _planViewFamilyTypes, value);
    }

    public List<ViewFamilyType> SectionViewFamilyTypes {
        get => _sectionViewFamilyTypes;
        set => RaiseAndSetIfChanged(ref _sectionViewFamilyTypes, value);
    }

    public List<ViewPlan> PlanViewTemplates {
        get => _planViewTemplates;
        set => RaiseAndSetIfChanged(ref _planViewTemplates, value);
    }

    /// <summary>
    /// Виды в плане проекта, которые можно копировать с детализацией - исходные виды для дублирования
    /// </summary>
    public List<ViewPlan> PlanViewsForDuplication {
        get => _planViewsForDuplication;
        set => RaiseAndSetIfChanged(ref _planViewsForDuplication, value);
    }

    public List<ViewSection> SectionViewTemplates {
        get => _sectionViewTemplates;
        set => RaiseAndSetIfChanged(ref _sectionViewTemplates, value);
    }

    public List<ElementType> ViewportTypes {
        get => _viewportTypes;
        set => RaiseAndSetIfChanged(ref _viewportTypes, value);
    }

    /// <summary>
    /// Спецификации проекта
    /// </summary>
    public List<ViewSchedule> SpecsInPj {
        get => _specsInPj;
        set => RaiseAndSetIfChanged(ref _specsInPj, value);
    }

    public List<TextNoteType> TextNoteTypes {
        get => _textNoteTypes;
        set => RaiseAndSetIfChanged(ref _textNoteTypes, value);
    }

    public List<View> LegendsInProject {
        get => _legendsInProject;
        set => RaiseAndSetIfChanged(ref _legendsInProject, value);
    }

    public List<Family> TitleBlockFamilies {
        get => _titleBlockFamilies;
        set => RaiseAndSetIfChanged(ref _titleBlockFamilies, value);
    }

    /// <summary>
    /// Условия фильтров спецификаций
    /// </summary>
    public IList<ScheduleTypeInfo> FilterTypes {
        get => _filterTypes;
        set => RaiseAndSetIfChanged(ref _filterTypes, value);
    }

    /// <summary>
    /// Типы модулей, которые можно добавить на лист (выпадающий список добавления)
    /// </summary>
    public List<ComponentTypeItem> ComponentTypes {
        get => _componentTypes;
        set => RaiseAndSetIfChanged(ref _componentTypes, value);
    }

    public ComponentTypeItem SelectedComponentType {
        get => _selectedComponentType;
        set => RaiseAndSetIfChanged(ref _selectedComponentType, value);
    }

    /// <summary>
    /// Типы параметров комплекта, которые можно добавить (выпадающий список добавления)
    /// </summary>
    public List<ComponentTypeItem> SheetSetParamTypes {
        get => _sheetSetParamTypes;
        set => RaiseAndSetIfChanged(ref _sheetSetParamTypes, value);
    }

    public ComponentTypeItem SelectedSheetSetParamType {
        get => _selectedSheetSetParamType;
        set => RaiseAndSetIfChanged(ref _selectedSheetSetParamType, value);
    }

    /// <summary>
    /// Загружает списки элементов проекта и последнюю использованную конфигурацию комплекта.
    /// Если файл последней конфигурации не найден, предлагает выбрать файл
    /// </summary>
    private void LoadView() {
        LoadConfig();
        GetSettingsForUI();

        if(string.IsNullOrEmpty(_sheetSetDataPath) || !File.Exists(_sheetSetDataPath)) {
            ImportSheetSet();
        } else {
            ImportSheetSet(_sheetSetDataPath);
        }
    }

    /// <summary>
    /// Заполняет списки типов модулей, типов параметров и элементов проекта для окна
    /// </summary>
    private void GetSettingsForUI() {
        ComponentTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(SheetComponentVM)))
            .Select(t =>
                new ComponentTypeItem(t, _localizationService.GetLocalizedString($"Type.{t.Name}") ?? string.Empty))
            .OrderBy(item => item.ComponentType switch {
                Type t when t == typeof(PlanViewVM) => 1,
                Type t when t == typeof(SectionViewVM) => 2,
                Type t when t == typeof(CalloutViewVM) => 3,
                _ => 4
            })
            .ToList();
        SelectedComponentType = null;

        SheetSetParamTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.IsSubclassOf(typeof(PluginParamVM)))
            .Select(t =>
                new ComponentTypeItem(t, _localizationService.GetLocalizedString($"Type.{t.Name}") ?? string.Empty))
            .ToList();
        SelectedSheetSetParamType = null;

        PlanViewFamilyTypes = _revitRepository.PlanViewTypes;
        SectionViewFamilyTypes = _revitRepository.SectionViewTypes;
        PlanViewTemplates = _revitRepository.PlanViewTemplates;
        PlanViewsForDuplication = _revitRepository.PlanViewsForDuplication;
        SectionViewTemplates = _revitRepository.SectionViewTemplates;
        ViewportTypes = _revitRepository.ViewportTypes;
        SpecsInPj = _revitRepository.Specs;
        TextNoteTypes = _revitRepository.TextNoteTypes;
        LegendsInProject = _revitRepository.LegendsInProject;
        TitleBlockFamilies = _revitRepository.TitleBlockFamilies;

        FilterTypes = _revitRepository.FilterTypes;
    }

    private void ImportSheetSet() {
        if(OpenFileDialogService.ShowDialog()) {
            // Если пользователь выбрал файл при выборе файла
            _sheetSetDataPath = OpenFileDialogService.File.FullName;
            ImportSheetSet(_sheetSetDataPath);
        } else {
            if(CurrentSheetSet is null) {
                // Если пользователь нажал Отмена при выборе файла и текущая конфигурация еще не загружена
                var sheetSetData = _sheetSetDataFactory.CreateSheetSetData();
                ImportSheetSet(sheetSetData);
            } else {
                // Если пользователь нажал Отмена при выборе файла и текущая конфигурация УЖЕ загружена
                return;
            }
        }
    }

    private void ImportSheetSet(string sheetSetDataPath) {
        var sheetSetData = _sheetSetConfig.Import(sheetSetDataPath);
        ImportSheetSet(sheetSetData);
    }

    private void ImportSheetSet(SheetSetData sheetSetData) {
        CurrentSheetSet = _sheetSetVMFactory.CreateSheetSetVM(sheetSetData);
        CurrentSheetSet.SelectedSheet = CurrentSheetSet.SheetList.FirstOrDefault();
        CurrentSheetSet.ValidateAllSheets();
    }

    private void ExportSheetSet() {
        if(SaveFileDialogService.ShowDialog(_sheetSetDataPath, "config.json")) {
            string temp = SaveFileDialogService.File.FullName;

            // Если путь некорректен
            if(string.IsNullOrEmpty(temp)) {
                MessageBoxService.Show(
                    _localizationService.GetLocalizedString("MainViewModel.ExportPathIsNotCorrect"),
                    _localizationService.GetLocalizedString("MainViewModel.Export"));
                return;
            }

            var currentSheetSetData = _sheetSetDataFactory.CreateSheetSetData(CurrentSheetSet);
            _sheetSetConfig.Export(currentSheetSetData, temp);
            // Сохраняем в плагине только после того, как все успешно экспортировалось
            _sheetSetDataPath = temp;

            MessageBoxService.Show(
                _localizationService.GetLocalizedString("MainViewModel.ExportIsSuccessful"),
                _localizationService.GetLocalizedString("MainViewModel.Export"));
        } else {
            // Если пользователь нажал отмена
            MessageBoxService.Show(
                _localizationService.GetLocalizedString("MainViewModel.ExportCanceled"),
                _localizationService.GetLocalizedString("MainViewModel.Export"));
        }
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        _sheetSetDataPath = setting?.SheetSetDataPath;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SheetSetDataPath = _sheetSetDataPath;
        _pluginConfig.SaveProjectConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. Только сохраняет конфигурацию: листы создаются
    /// командой после закрытия окна (см. <see cref="ProcessSheets"/>)
    /// </summary>
    private void AcceptView() {
        SaveConfig();
    }

    /// <summary>
    /// Создает отмеченные листы и их модули в одной транзакции.
    /// </summary>
    /// <remarks>
    /// Вызывается командой после закрытия окна плагина и освобождения его COM-объектов,
    /// а не из AcceptView: так загрузка и обновление семейств выполняются, когда объекты окна
    /// уже освобождены. Подробнее - в RevitPackageDocumentationCommand.ReleaseClosedWindowComObjects
    /// </remarks>
    public void ProcessSheets() {
        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("MainWindow.Title"));

        foreach(var sheet in CurrentSheetSet.SheetList.Where(s => s.IsModuleCheck).ToList()) {
            sheet.Process(true);
        }
        transaction.Commit();
    }

    /// <summary>
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(CurrentSheetSet?.SheetList?
            .Where(s => s.IsModuleCheck)
            .Any(c => c.HasErrors) ?? true) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorInSheets");
            return false;
        }
        if(CurrentSheetSet?.SheetList?.Count == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.SheetSetHasNotSheets");
            return false;
        }
        if(CurrentSheetSet?.SheetList?.All(p => !p.IsModuleCheck) == true) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.NoSheetsSelected");
            return false;
        }

        // Имя вида должно быть уникальным только среди видов одного типа: план, разрез и спецификация
        // могут называться одинаково, поэтому имя сравнивается вместе с типом вида
        var viewNames = new HashSet<(string ViewKind, string Name)>();
        // Проверяются только компоненты листов, включенных в генерацию
        foreach(var sheetComponent in CurrentSheetSet?.SheetList
            ?.Where(s => s.IsModuleCheck)
            .SelectMany(s => s.SheetComponents)
            .Where(c => c.IsModuleCheck)
            .ToList() ?? []) {
            foreach(var viewName in GetCreatedViewNames(sheetComponent)) {
                if(string.IsNullOrEmpty(viewName.Name))
                    continue;

                if(!viewNames.Add(viewName)) {
                    ErrorText = $"{_localizationService.GetLocalizedString("MainWindow.ViewNameAlreadyHas")} - " +
                                $"{sheetComponent.Sheet.ModuleName} - {viewName.Name}";
                    return false;
                }
            }
        }

        var paramNames = new HashSet<string>();
        foreach(var parameter in CurrentSheetSet.SheetSetParams.Params) {
            string name = parameter.ParamName;
            if(string.IsNullOrEmpty(name))
                continue;

            if(!paramNames.Add(name)) {
                ErrorText = $"{_localizationService.GetLocalizedString("MainWindow.ParamNameAlreadyHas")} - {name}";
                return false;
            }
        }

        ErrorText = string.Empty;
        return true;
    }

    /// <summary>
    /// Возвращает имена видов, которые создаст модуль, вместе с типом вида.
    /// </summary>
    /// <remarks>
    /// Revit требует уникальности имени только среди видов одного типа. Тип определяется семейством вида
    /// (план этажа, план потолка, разрез, узел и т.д.), спецификации - отдельный тип.
    /// Фрагмент на плане этажа является планом этажа, поэтому его имя проверяется вместе с именами планов.
    /// Для сечений и фрагментов проверяются фактические имена создаваемых видов: ИмяВида_1..N.
    /// </remarks>
    private static IEnumerable<(string ViewKind, string Name)> GetCreatedViewNames(SheetComponentVM component) {
        switch(component) {
            case ScheduleViewVM schedule:
                yield return (nameof(ViewSchedule), schedule.ViewName);
                break;
            case PlanViewVM plan:
                yield return (plan.GetPlanViewFamily()?.ToString(), plan.ViewName);
                break;
            case SectionViewVM section:
                foreach(int number in GetViewNumbers(section.ViewCount)) {
                    yield return (section.ViewFamilyType?.ViewFamily.ToString(), section.GetNumberedViewName(number));
                }
                break;
            case CalloutViewVM callout:
                foreach(int number in GetViewNumbers(callout.ViewCount)) {
                    yield return (callout.ViewFamilyType?.ViewFamily.ToString(), callout.GetNumberedViewName(number));
                }
                break;
        }
    }

    /// <summary>
    /// Возвращает порядковые номера видов 1..ViewCount. Если количество задано некорректно - пустой список
    /// (об этом сообщает проверка модуля)
    /// </summary>
    private static IEnumerable<int> GetViewNumbers(string viewCount) {
        return int.TryParse(viewCount, out int count) && count > 0
            ? Enumerable.Range(1, count)
            : Enumerable.Empty<int>();
    }

    /// <summary>
    /// Метод команды по выбору элемента для параметра конфигурации
    /// </summary>
    private void SelectElemForParam(SelectElemParamVM vm) {
        _revitElementPickerService.PickElement(onSelected: (element) => {
            vm.SelectedElem = element;
            // Передаем сам параметр, чтобы обновились модули, зависящие от выбранного элемента (например, фрагменты)
            vm.ParamValueChangeCommand.Execute(vm);
        });
    }
}
