using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;

namespace RevitPunchingRebar.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IPunchingRebarPlacementService _placementService;
    private readonly ILocalizationService _localizationService;

    private List<ElementId> _pylonsFromModel = [];
    private List<LinkedPylonReference> _pylonsFromLink = [];
    private Slab _slab;

    private string _errorText;

    private int _pylonsFromModelCount;
    private int _pylonsFromLinkCount;
    private int _selectedPylonsCount;
    private bool _isSlabSelected;

    private int _selectedSlabRebarDiameter;
    private bool _isRebarCoverFromSlab;
    private bool _isRebarCoverFromUser;
    private int _rebarCoverTop;
    private int _rebarCoverBottom;

    private IList<string> _familyNames;
    private string _selectedFamilyName;
    private IList<string> _familyTypes;
    private string _selectedFamilyType;
    private int _selectedStirrupDiameter;
    private string _selectedRebarClass;
    private int _stirrupStep;
    private int _frameWidth;

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="placementService">Сервис расстановки каркасов.</param>
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        IPunchingRebarPlacementService placementService,
        ILocalizationService localizationService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _placementService = placementService;
        _localizationService = localizationService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectPylonsFromModelCommand = RelayCommand.Create<Window>(SelectPylonsFromModel);
        SelectPylonsFromLinkCommand = RelayCommand.Create<Window>(SelectPylonsFromLink);
        SelectSlabCommand = RelayCommand.Create<Window>(SelectSlab);
        LoadFamilyTypesCommand = RelayCommand.Create(LoadFamilyTypes);
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    public ICommand AcceptViewCommand { get; }

    /// <summary>
    /// Команда выбора пилонов в текущем проекте.
    /// </summary>
    public ICommand SelectPylonsFromModelCommand { get; }

    /// <summary>
    /// Команда выбора пилонов в связанных файлах.
    /// </summary>
    public ICommand SelectPylonsFromLinkCommand { get; }

    /// <summary>
    /// Команда выбора плиты.
    /// </summary>
    public ICommand SelectSlabCommand { get; }

    /// <summary>
    /// Команда загрузки типов выбранного семейства каркаса.
    /// </summary>
    public ICommand LoadFamilyTypesCommand { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Доступные диаметры арматуры, мм.
    /// </summary>
    public List<int> RebarDiameters { get; } = [6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 32, 36, 40];

    /// <summary>
    /// Доступные классы арматуры хомутов.
    /// </summary>
    public List<string> RebarClasses { get; } = ["А500С", "А240"];

    /// <summary>
    /// Количество пилонов, выбранных в текущем проекте.
    /// </summary>
    public int PylonsFromModelCount {
        get => _pylonsFromModelCount;
        private set => RaiseAndSetIfChanged(ref _pylonsFromModelCount, value);
    }

    /// <summary>
    /// Количество пилонов, выбранных в связанных файлах.
    /// </summary>
    public int PylonsFromLinkCount {
        get => _pylonsFromLinkCount;
        private set => RaiseAndSetIfChanged(ref _pylonsFromLinkCount, value);
    }

    /// <summary>
    /// Общее количество выбранных пилонов.
    /// </summary>
    public int SelectedPylonsCount {
        get => _selectedPylonsCount;
        private set => RaiseAndSetIfChanged(ref _selectedPylonsCount, value);
    }

    /// <summary>
    /// Выбрана ли плита.
    /// </summary>
    public bool IsSlabSelected {
        get => _isSlabSelected;
        private set => RaiseAndSetIfChanged(ref _isSlabSelected, value);
    }

    /// <summary>
    /// Диаметр фоновой арматуры плиты, мм.
    /// </summary>
    public int SelectedSlabRebarDiameter {
        get => _selectedSlabRebarDiameter;
        set => RaiseAndSetIfChanged(ref _selectedSlabRebarDiameter, value);
    }

    /// <summary>
    /// Брать защитный слой из типов защитного слоя плиты.
    /// </summary>
    public bool IsRebarCoverFromSlab {
        get => _isRebarCoverFromSlab;
        set => RaiseAndSetIfChanged(ref _isRebarCoverFromSlab, value);
    }

    /// <summary>
    /// Задать защитный слой вручную.
    /// </summary>
    public bool IsRebarCoverFromUser {
        get => _isRebarCoverFromUser;
        set => RaiseAndSetIfChanged(ref _isRebarCoverFromUser, value);
    }

    /// <summary>
    /// Защитный слой сверху, мм.
    /// </summary>
    public int RebarCoverTop {
        get => _rebarCoverTop;
        set => RaiseAndSetIfChanged(ref _rebarCoverTop, value);
    }

    /// <summary>
    /// Защитный слой снизу, мм.
    /// </summary>
    public int RebarCoverBottom {
        get => _rebarCoverBottom;
        set => RaiseAndSetIfChanged(ref _rebarCoverBottom, value);
    }

    /// <summary>
    /// Имена семейств, загруженных в проект.
    /// </summary>
    public IList<string> FamilyNames {
        get => _familyNames;
        private set => RaiseAndSetIfChanged(ref _familyNames, value);
    }

    /// <summary>
    /// Выбранное семейство каркаса.
    /// </summary>
    public string SelectedFamilyName {
        get => _selectedFamilyName;
        set => RaiseAndSetIfChanged(ref _selectedFamilyName, value);
    }

    /// <summary>
    /// Типы выбранного семейства каркаса.
    /// </summary>
    public IList<string> FamilyTypes {
        get => _familyTypes;
        private set => RaiseAndSetIfChanged(ref _familyTypes, value);
    }

    /// <summary>
    /// Выбранный тип каркаса.
    /// </summary>
    public string SelectedFamilyType {
        get => _selectedFamilyType;
        set => RaiseAndSetIfChanged(ref _selectedFamilyType, value);
    }

    /// <summary>
    /// Диаметр хомутов, мм.
    /// </summary>
    public int SelectedStirrupDiameter {
        get => _selectedStirrupDiameter;
        set => RaiseAndSetIfChanged(ref _selectedStirrupDiameter, value);
    }

    /// <summary>
    /// Класс арматуры хомутов.
    /// </summary>
    public string SelectedRebarClass {
        get => _selectedRebarClass;
        set => RaiseAndSetIfChanged(ref _selectedRebarClass, value);
    }

    /// <summary>
    /// Шаг хомутов, мм.
    /// </summary>
    public int StirrupStep {
        get => _stirrupStep;
        set => RaiseAndSetIfChanged(ref _stirrupStep, value);
    }

    /// <summary>
    /// Ширина каркаса, мм.
    /// </summary>
    public int FrameWidth {
        get => _frameWidth;
        set => RaiseAndSetIfChanged(ref _frameWidth, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        FamilyNames = _revitRepository.GetFamilyNames();
        LoadConfig();
    }

    /// <summary>
    /// Метод применения настроек главного окна. (выполнение плагина)
    /// </summary>
    /// <remarks>
    /// В данном методе должны браться настройки пользователя и сохраняться в конфиг, а так же быть основной код плагина.
    /// </remarks>
    private void AcceptView() {
        RevitSettings settings = SaveConfig();
        PlaceFrames(settings);
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
        if(SelectedPylonsCount == 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectPylons");
            return false;
        }

        if(_slab == null) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectSlab");
            return false;
        }

        double rebarCoverTop = UnitUtilsHelper.FromMm(RebarCoverTop);
        double rebarCoverBottom = UnitUtilsHelper.FromMm(RebarCoverBottom);

        if(rebarCoverTop <= 0 || rebarCoverTop > _slab.Thickness * 0.5) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorRebarCoverTop");
            return false;
        }

        if(rebarCoverBottom <= 0 || rebarCoverBottom > _slab.Thickness * 0.5) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorRebarCoverBottom");
            return false;
        }

        if(string.IsNullOrEmpty(SelectedFamilyName)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectFamily");
            return false;
        }

        if(string.IsNullOrEmpty(SelectedFamilyType)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectFamilyType");
            return false;
        }

        if(string.IsNullOrEmpty(SelectedRebarClass)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSelectRebarClass");
            return false;
        }

        if(StirrupStep <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorStirrupStep");
            return false;
        }

        if(FrameWidth <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorFrameWidth");
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Скрывает окно и дает выбрать пилоны в текущем проекте.
    /// Выбор заменяет ранее выбранные пилоны из текущего проекта и не затрагивает пилоны из связей.
    /// </summary>
    private void SelectPylonsFromModel(Window window) {
        SelectInRevit(window, () => {
            _pylonsFromModel = _revitRepository.PickPylonsFromModel();
            UpdatePylonsCount();
        });
    }

    /// <summary>
    /// Скрывает окно и дает выбрать пилоны в связанных файлах.
    /// Выбор заменяет ранее выбранные пилоны из связей и не затрагивает пилоны из текущего проекта.
    /// </summary>
    private void SelectPylonsFromLink(Window window) {
        SelectInRevit(window, () => {
            _pylonsFromLink = _revitRepository.PickPylonsFromLink();
            UpdatePylonsCount();
        });
    }

    /// <summary>
    /// Скрывает окно и дает выбрать плиту.
    /// </summary>
    private void SelectSlab(Window window) {
        SelectInRevit(window, () => {
            _slab = _revitRepository.GetSlab(_revitRepository.PickSlab());
            IsSlabSelected = true;
        });
    }

    /// <summary>
    /// Скрывает окно на время выбора элементов в Revit и показывает его снова после выбора или отмены.
    /// </summary>
    /// <remarks>
    /// При ошибке окно повторно не показывается: исключение уходит в команду и обрабатывается платформой.
    /// </remarks>
    private void SelectInRevit(Window window, Action selectAction) {
        window.Hide();
        try {
            selectAction();
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // Пользователь отменил выбор - оставляем предыдущий выбор
        }

        _ = window.ShowDialog();
    }

    private void UpdatePylonsCount() {
        PylonsFromModelCount = _pylonsFromModel.Count;
        PylonsFromLinkCount = _pylonsFromLink.Count;
        SelectedPylonsCount = PylonsFromModelCount + PylonsFromLinkCount;
    }

    /// <summary>
    /// Загружает типы выбранного семейства каркаса.
    /// </summary>
    private void LoadFamilyTypes() {
        FamilyTypes = _revitRepository.GetFamilyTypes(SelectedFamilyName);
    }

    /// <summary>
    /// Расставляет каркасы у выбранных пилонов по сохраненным настройкам.
    /// </summary>
    private void PlaceFrames(RevitSettings settings) {
        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using Transaction transaction = _revitRepository.Document.StartTransaction(transactionName);

        List<IPylon> pylons = _revitRepository.GetPylons(_pylonsFromModel, _pylonsFromLink);

        FrameParams frameParams = new FrameParams();

        frameParams.FrameFamilyName = settings.FamilyName;
        frameParams.FrameFamilyTypeName = settings.FamilyType;
        frameParams.HostSlab = _slab;
        frameParams.SlabThickness = _slab.Thickness;
        frameParams.RebarCoverTop = UnitUtilsHelper.FromMm(settings.RebarCoverTop);
        frameParams.RebarCoverBottom = UnitUtilsHelper.FromMm(settings.RebarCoverBottom);
        frameParams.PlateRebarDiameter = UnitUtilsHelper.FromMm(settings.SlabRebarDiameter);
        frameParams.StirrupRebarClass = settings.RebarClass == "А500С" ? RebarClass.A500C : RebarClass.A240;
        frameParams.StirrupRebarDiameter = UnitUtilsHelper.FromMm(settings.StirrupDiameter);
        frameParams.StirrupStep = UnitUtilsHelper.FromMm(settings.StirrupStep);
        frameParams.FrameWidth = UnitUtilsHelper.FromMm(settings.FrameWidth);

        _placementService.Run(pylons, frameParams, _revitRepository);

        transaction.Commit();
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SelectedFamilyName = setting?.FamilyName ?? string.Empty;
        LoadFamilyTypes();
        SelectedFamilyType = setting?.FamilyType ?? string.Empty;

        SelectedSlabRebarDiameter = setting?.SlabRebarDiameter ?? 10;
        IsRebarCoverFromUser = setting?.IsRebarCoverFromUser ?? true;
        IsRebarCoverFromSlab = !IsRebarCoverFromUser;
        RebarCoverTop = setting?.RebarCoverTop ?? 20;
        RebarCoverBottom = setting?.RebarCoverBottom ?? 40;

        SelectedStirrupDiameter = setting?.StirrupDiameter ?? 10;
        SelectedRebarClass = setting?.RebarClass ?? "А500С";
        StirrupStep = setting?.StirrupStep ?? 200;
        FrameWidth = setting?.FrameWidth ?? 200;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    /// <returns>Сохраненные настройки текущего проекта.</returns>
    private RevitSettings SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.SlabRebarDiameter = SelectedSlabRebarDiameter;
        setting.IsRebarCoverFromUser = IsRebarCoverFromUser;

        if(IsRebarCoverFromSlab) {
            setting.RebarCoverTop = Convert.ToInt32(UnitUtilsHelper.ToMm(_slab.RebarCoverTop));
            setting.RebarCoverBottom = Convert.ToInt32(UnitUtilsHelper.ToMm(_slab.RebarCoverBottom));
        } else {
            setting.RebarCoverTop = RebarCoverTop;
            setting.RebarCoverBottom = RebarCoverBottom;
        }

        setting.FamilyName = SelectedFamilyName;
        setting.FamilyType = SelectedFamilyType;
        setting.StirrupDiameter = SelectedStirrupDiameter;
        setting.RebarClass = SelectedRebarClass;
        setting.StirrupStep = StirrupStep;
        setting.FrameWidth = FrameWidth;

        _pluginConfig.SaveProjectConfig();
        return setting;
    }
}
