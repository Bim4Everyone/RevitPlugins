using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;

using RevitPackageDocumentation.Models;
using RevitPackageDocumentation.Models.ConfigSerializer;
using RevitPackageDocumentation.ViewModels.Configuration.Sheet.SheetComponents;
using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;
using RevitPackageDocumentation.ViewModels.FiltrationComboBoxVMs;
using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Configuration.Sheet;

/// <summary>
/// Модуль листа комплекта: имя и номер листа, рамка с форматом и модули, размещаемые на листе
/// </summary>
internal class SheetVM : ModuleVM {
    // Параметры рамки листа: коэффициент кратности формата (кириллическая "А") и номер формата (кириллическая "х")
    private const string _sheetCoefficientParamName = "А";
    private const string _sheetSizeParamName = "х";

    private readonly ILocalizationService _localizationService;
    private readonly IMessageBoxService _messageBoxService;
    private readonly ISheetSetVMFactory _sheetSetVMFactory;
    private readonly ISheetSetDataFactory _sheetSetDataFactory;

    private SheetSetVM _sheetSet;
    private string _sheetNameFormula = string.Empty;
    private string _sheetName;
    private string _sheetNumberFormula = string.Empty;
    private string _sheetNumber;
    private string _sheetSize;
    private string _sheetCoefficient;
    private Family _titleBlockFamily;
    private FamilySymbol _titleBlockType;
    private ObservableCollection<SheetComponentVM> _sheetComponents = [];
    private List<FamilySymbol> _titleBlockTypes;
    private FiltrationComboBoxFilterListVM _titleBlockFamilyFilter;
    private FiltrationComboBoxFilterListVM _titleBlockTypeFilter;
    private ViewSheet _sheetInstance;

    public SheetVM(
        RevitRepository repository,
        StringParamSetService stringParamSetService,
        ObservableCollection<PluginParamVM> sheetSetParams,
        SheetSetVM sheetSetVM,
        ILocalizationService localizationService,
        IMessageBoxService messageBoxService,
        ISheetSetVMFactory sheetSetVMFactory,
        ISheetSetDataFactory sheetSetDataFactory)
        : base(repository, stringParamSetService, sheetSetParams, localizationService) {

        SheetSet = sheetSetVM;
        _localizationService = localizationService;
        _messageBoxService = messageBoxService;
        _sheetSetVMFactory = sheetSetVMFactory;
        _sheetSetDataFactory = sheetSetDataFactory;

        SelectTitleBlockFamilyCommand = RelayCommand.Create(SelectTitleBlockFamily);
        CreateSheetCommand = RelayCommand.Create(CreateComponent, CanCreateComponent);

        AddComponentCommand = RelayCommand.Create<ComponentTypeItem>(AddComponent);
        RemoveComponentCommand = RelayCommand.Create<SheetComponentVM>(RemoveComponent);
        SelectAllComponentsCommand = RelayCommand.Create(SelectAllComponents);
        DeselectAllComponentsCommand = RelayCommand.Create(DeselectAllComponents);
        SelectComponentsWithoutErrorsCommand = RelayCommand.Create(SelectComponentsWithoutErrors);

        // Родительский вид фрагмента пересчитывается при добавлении модуля и смене его элемента привязки/типоразмера
        _sheetComponents.CollectionChanged += OnSheetComponentsChanged;
    }

    /// <summary>
    /// Команда выбора семейства рамки, вызывается выпадающим списком семейств
    /// </summary>
    public ICommand SelectTitleBlockFamilyCommand { get; }

    /// <summary>
    /// Команда создания только этого листа с его модулями
    /// </summary>
    public ICommand CreateSheetCommand { get; }

    /// <summary>
    /// Команда добавления модуля выбранного типа на лист
    /// </summary>
    public ICommand AddComponentCommand { get; }

    /// <summary>
    /// Команда удаления модуля с листа
    /// </summary>
    public ICommand RemoveComponentCommand { get; }

    /// <summary>
    /// Команда включения в генерацию всех модулей листа
    /// </summary>
    public ICommand SelectAllComponentsCommand { get; }

    /// <summary>
    /// Команда исключения из генерации всех модулей листа
    /// </summary>
    public ICommand DeselectAllComponentsCommand { get; }

    /// <summary>
    /// Команда включения в генерацию модулей листа без ошибок и исключения остальных
    /// </summary>
    public ICommand SelectComponentsWithoutErrorsCommand { get; }

    /// <summary>
    /// Комплект, которому принадлежит лист
    /// </summary>
    public SheetSetVM SheetSet {
        get => _sheetSet;
        set => RaiseAndSetIfChanged(ref _sheetSet, value);
    }

    [Required(ErrorMessage = "Validation.SheetNameIsEmpty")]
    [RegularExpression(@"^[^\\\/:*?""<>|\[\];~]+$", ErrorMessage = "Validation.SheetNameIsNotCorrect")]
    public string SheetNameFormula {
        get => _sheetNameFormula;
        set => RaiseAndSetIfChanged(ref _sheetNameFormula, value);
    }

    /// <summary>
    /// Имя листа, вычисленное по формуле
    /// </summary>
    public string SheetName {
        get => _sheetName;
        set => RaiseAndSetIfChanged(ref _sheetName, value);
    }

    /// <summary>
    /// Формула номера листа. Необязательна: если пустая, номер создаваемого листа назначает Revit
    /// </summary>
    public string SheetNumberFormula {
        get => _sheetNumberFormula;
        set => RaiseAndSetIfChanged(ref _sheetNumberFormula, value);
    }

    /// <summary>
    /// Номер листа, вычисленный по формуле
    /// </summary>
    public string SheetNumber {
        get => _sheetNumber;
        set => RaiseAndSetIfChanged(ref _sheetNumber, value);
    }

    /// <summary>
    /// Номер формата листа (параметр рамки "х")
    /// </summary>
    [Required(ErrorMessage = "Validation.SheetSizeIsEmpty")]
    [RegularExpression(@"^-?\d+$", ErrorMessage = "Validation.SheetSizeIsNotCorrect")]
    public string SheetSize {
        get => _sheetSize;
        set => RaiseAndSetIfChanged(ref _sheetSize, value);
    }

    /// <summary>
    /// Коэффициент кратности формата листа (параметр рамки "А")
    /// </summary>
    [Required(ErrorMessage = "Validation.SheetCoefficientIsEmpty")]
    [RegularExpression(@"^-?\d+$", ErrorMessage = "Validation.SheetCoefficientIsNotCorrect")]
    public string SheetCoefficient {
        get => _sheetCoefficient;
        set => RaiseAndSetIfChanged(ref _sheetCoefficient, value);
    }

    /// <summary>
    /// Типоразмеры выбранного семейства рамки
    /// </summary>
    public List<FamilySymbol> TitleBlockTypes {
        get => _titleBlockTypes;
        set => RaiseAndSetIfChanged(ref _titleBlockTypes, value);
    }

    [Required(ErrorMessage = "Validation.TitleBlockFamilyIsNull")]
    public Family TitleBlockFamily {
        get => _titleBlockFamily;
        set => RaiseAndSetIfChanged(ref _titleBlockFamily, value);
    }

    [Required(ErrorMessage = "Validation.TitleBlockTypeIsNull")]
    public FamilySymbol TitleBlockType {
        get => _titleBlockType;
        set => RaiseAndSetIfChanged(ref _titleBlockType, value);
    }

    /// <summary>
    /// Фильтры выпадающего списка семейств рамки
    /// </summary>
    public FiltrationComboBoxFilterListVM TitleBlockFamilyFilter {
        get => _titleBlockFamilyFilter;
        set => RaiseAndSetIfChanged(ref _titleBlockFamilyFilter, value);
    }

    /// <summary>
    /// Фильтры выпадающего списка типоразмеров рамки
    /// </summary>
    public FiltrationComboBoxFilterListVM TitleBlockTypeFilter {
        get => _titleBlockTypeFilter;
        set => RaiseAndSetIfChanged(ref _titleBlockTypeFilter, value);
    }

    // Компоненты, исключенные из генерации, не влияют на ошибки листа
    [ChildHasErrors(IgnoreUncheckedModules = true, ErrorMessage = "Validation.ErrorInSheetComponents")]
    public ObservableCollection<SheetComponentVM> SheetComponents {
        get => _sheetComponents;
        set => RaiseAndSetIfChanged(ref _sheetComponents, value);
    }

    /// <summary>
    /// Лист в модели: найденный существующий или созданный при генерации
    /// </summary>
    public ViewSheet SheetInstance {
        get => _sheetInstance;
        set => RaiseAndSetIfChanged(ref _sheetInstance, value);
    }

    private void SelectTitleBlockFamily() {
        // Команда вызывается при любой смене выбора в списке семейств, в том числе при переключении листов
        // и автовыборе после фильтрации, поэтому типоразмеры сбрасываются, только если семейство действительно сменилось
        var currentFamilyId = TitleBlockTypes?.FirstOrDefault()?.Family?.Id;
        if(TitleBlockFamily != null && currentFamilyId == TitleBlockFamily.Id) {
            return;
        }
        TitleBlockType = null;
        SetTitleBlockTypes(TitleBlockFamily);
    }

    /// <summary>
    /// Заполняет список типоразмеров по семейству рамки
    /// </summary>
    public void SetTitleBlockTypes(Family titleBlockFamily) {
        TitleBlockTypes = titleBlockFamily
            ?.GetFamilySymbolIds()
            ?.Select(id => Repository.Document.GetElement(id) as FamilySymbol)
            ?.ToList();
    }

    /// <summary>
    /// Перемещает модуль листа на offset позиций. Порядок модулей - порядок их создания и размещения на листе
    /// </summary>
    public void MoveComponent(SheetComponentVM component, int offset) {
        if(!CanMoveComponent(component, offset)) {
            return;
        }
        int index = SheetComponents.IndexOf(component);
        SheetComponents.Move(index, index + offset);
    }

    /// <summary>
    /// Проверяет, можно ли переместить модуль листа на offset позиций, не выходя за границы списка
    /// </summary>
    public bool CanMoveComponent(SheetComponentVM component, int offset) {
        int index = SheetComponents.IndexOf(component);
        return index >= 0 && index + offset >= 0 && index + offset < SheetComponents.Count;
    }

    protected override void MoveModule(int offset) {
        SheetSet?.MoveSheet(this, offset);
    }

    protected override bool CanMoveModule(int offset) {
        return SheetSet?.CanMoveSheet(this, offset) ?? false;
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
            _messageBoxService.Show(
                _localizationService.GetLocalizedString("MainViewModel.AddComponentError"),
                _localizationService.GetLocalizedString("MainViewModel.Error"));
        }
    }

    private void OnSheetComponentsChanged(object sender, NotifyCollectionChangedEventArgs e) {
        // При перемещении модуль приходит и в NewItems, и в OldItems: подписки не меняются,
        // но от порядка модулей зависят родительские планы фрагментов
        if(e.Action == NotifyCollectionChangedAction.Move) {
            UpdateCalloutParents();
            return;
        }
        if(e.NewItems != null) {
            foreach(var component in e.NewItems.OfType<SheetComponentVM>()) {
                component.PropertyChanged += OnSheetComponentPropertyChanged;
            }
        }
        if(e.OldItems != null) {
            foreach(var component in e.OldItems.OfType<SheetComponentVM>()) {
                component.PropertyChanged -= OnSheetComponentPropertyChanged;
            }
        }
        UpdateCalloutParents();
    }

    private void OnSheetComponentPropertyChanged(object sender, PropertyChangedEventArgs e) {
        // Родительский вид фрагмента зависит от уровня элемента привязки и семейства типоразмера вида
        if(sender is CalloutViewVM callout
            && e.PropertyName is nameof(CalloutViewVM.SelectedSelectElemParam) or nameof(CalloutViewVM.ViewFamilyType)) {
            callout.UpdateParentPlan();
            return;
        }

        // Модуль плана выше на листе может создать родительский план для фрагментов - его изменения влияют на них
        if(sender is PlanViewVM
            && e.PropertyName is nameof(PlanViewVM.IsModuleCheck)
                or nameof(PlanViewVM.HasErrors)
                or nameof(PlanViewVM.SelectedSelectElemParam)
                or nameof(PlanViewVM.IsDuplicate)
                or nameof(PlanViewVM.ReferenceView)
                or nameof(PlanViewVM.ViewFamilyType)) {
            UpdateCalloutParents();
        }
    }

    /// <summary>
    /// Пересчитывает родительские планы всех фрагментов листа
    /// </summary>
    public void UpdateCalloutParents() {
        foreach(var callout in SheetComponents.OfType<CalloutViewVM>()) {
            callout.UpdateParentPlan();
        }
    }

    private void SelectAllComponents() {
        foreach(var component in SheetComponents) {
            component.IsModuleCheck = true;
        }
    }

    private void DeselectAllComponents() {
        foreach(var component in SheetComponents) {
            component.IsModuleCheck = false;
        }
    }

    /// <summary>
    /// Отмечает модули листа без ошибок и снимает отметку с остальных. Галочка самого листа не меняется
    /// </summary>
    public void SelectComponentsWithoutErrors() {
        // Модули перебираются сверху вниз: при снятии галочки с плана ошибка фрагментов ниже, которые рассчитывали
        // на этот план, пересчитывается сразу, поэтому они снимаются по общему правилу
        foreach(var component in SheetComponents) {
            component.IsModuleCheck = !component.HasErrors;
        }
    }

    /// <summary>
    /// Создает лист и его отмеченные модули в отдельной транзакции
    /// </summary>
    public override void CreateComponent() {
        using var transaction = Repository.Document.StartTransaction(
            _localizationService.GetLocalizedString("MainWindow.Title"));

        Process(true);
        transaction.Commit();
    }

    public override bool CanCreateComponent() {
        return !HasErrors;
    }

    /// <summary>
    /// Назначает номер созданному листу, если он задан. Если номер не задан или Revit его не принял
    /// (например, такой номер уже занят), остается номер, назначенный Revit, а создание листа продолжается
    /// </summary>
    private void SetSheetNumber(ViewSheet sheet) {
        if(string.IsNullOrWhiteSpace(SheetNumber)) {
            return;
        }
        try {
            sheet.SheetNumber = SheetNumber;
        } catch(Exception) { }
    }

    /// <summary>
    /// Находит лист по имени и номеру или создает новый, затем при processDependent = true
    /// размещает на нем отмеченные модули. Должен вызываться внутри открытой транзакции
    /// </summary>
    public override void Process(bool processDependent) {
        SheetInstance = null;
        SheetInstance = Repository.FindSheet(SheetName, SheetNumber);

        if(SheetInstance is null) {
            try {
                SheetInstance = ViewSheet.Create(Repository.Document, TitleBlockType.Id);
                SheetInstance.Name = SheetName;
                SetSheetNumber(SheetInstance);

                var titleBlock = Repository.GetTitleBlock(SheetInstance);

                // Если у рамки нет параметров формата, остальные параметры листа все равно заполняются
                double.TryParse(SheetSize, out double sheetSize);
                titleBlock?.LookupParameter(_sheetSizeParamName)?.Set(sheetSize);

                double.TryParse(SheetCoefficient, out double sheetCoefficient);
                titleBlock?.LookupParameter(_sheetCoefficientParamName)?.Set(sheetCoefficient);

                SetCustomParams(SheetInstance);

                Repository.Document.Regenerate();
            } catch(Exception) { }
        }

        if(processDependent) {
            foreach(var component in SheetComponents.Where(c => c.IsModuleCheck).ToList()) {
                component.Process();
            }
        }
    }
}
