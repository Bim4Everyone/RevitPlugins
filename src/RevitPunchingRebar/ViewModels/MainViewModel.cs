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
using RevitPunchingRebar.Services;

using Visibility = System.Windows.Visibility;

namespace RevitPunchingRebar.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private bool _isInitialized;
    private bool _isParameterSettingsOpen;
    private ParameterSettingsViewModel _parameterEditor;
    private FrameParameterSettings _parameterSettings = FrameParameterSettings.CopyWithDefaults(null);
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IPunchingRebarPlacementService _placementService;
    private readonly ILocalizationService _localizationService;

    private List<ElementId> _pylonsFromModel = [];
    private List<LinkedPylonReference> _pylonsFromLink = [];
    private Slab _slab;

    private string _errorText;

    private string _pylonsFromModelLabel;
    private string _pylonsFromLinkLabel;
    private string _slabLabel;
    private bool _isPylonsSelected;
    private int _selectedPylonsCount;
    private bool _isSlabSelected;

    private int _selectedSlabRebarDiameter;
    private bool _isRebarCoverFromSlab;
    private bool _isRebarCoverFromUser;
    private string _rebarCoverTop;
    private string _rebarCoverBottom;

    private IList<string> _familyNames;
    private string _selectedFamilyName;
    private IList<string> _familyTypes;
    private string _selectedFamilyType;
    private int _selectedStirrupDiameter;
    private RebarClass _selectedRebarClass;
    private string _stirrupStep;
    private string _frameWidth;
    private int _selectedLongRebarDiameter;
    private string _longRebarAnchorageLength;
    private string _stirrupBendLength;

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
        ParameterSettingsCommand = RelayCommand.Create(ToggleParameterSettings);
        ApplyParameterSettingsCommand = RelayCommand.Create(ApplyParameterSettings);
        CancelParameterSettingsCommand = RelayCommand.Create(CancelParameterSettings);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectPylonsFromModelCommand = RelayCommand.Create<Window>(SelectPylonsFromModel);
        SelectPylonsFromLinkCommand = RelayCommand.Create<Window>(SelectPylonsFromLink);
        SelectSlabCommand = RelayCommand.Create<Window>(SelectSlab);
        LoadFamilyTypesCommand = RelayCommand.Create(LoadFamilyTypes);
        ResetToDefaultCommand = RelayCommand.Create<string>(ResetToDefault);

        RebarClasses = [
            new RebarClassVM(RebarClass.A500C, _localizationService.GetLocalizedString("RebarClass.A500C")),
            new RebarClassVM(RebarClass.A240, _localizationService.GetLocalizedString("RebarClass.A240"))
        ];

        DefaultValueHints = CreateDefaultValueHints();

        UpdatePylonsCount();
        UpdateSlabLabel();
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }

    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    public ICommand AcceptViewCommand { get; }
    public ICommand ParameterSettingsCommand { get; }
    public ICommand ApplyParameterSettingsCommand { get; }
    public ICommand CancelParameterSettingsCommand { get; }

    public ParameterSettingsViewModel ParameterEditor {
        get => _parameterEditor;
        private set => RaiseAndSetIfChanged(ref _parameterEditor, value);
    }

    public bool IsParameterSettingsOpen {
        get => _isParameterSettingsOpen;
        private set => RaiseAndSetIfChanged(ref _isParameterSettingsOpen, value);
    }

    public Visibility MainContentVisibility => IsParameterSettingsOpen ? Visibility.Collapsed : Visibility.Visible;
    public Visibility ParameterSettingsVisibility => IsParameterSettingsOpen ? Visibility.Visible : Visibility.Collapsed;

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
    /// Команда сброса поля к значению по умолчанию. Параметр — имя свойства ViewModel.
    /// </summary>
    public ICommand ResetToDefaultCommand { get; }

    /// <summary>
    /// Подсказки кнопок сброса со значением по умолчанию. Ключ — имя свойства ViewModel.
    /// </summary>
    public Dictionary<string, string> DefaultValueHints { get; }

    /// <summary>
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Итог расстановки каркасов. null, если расстановка не выполнялась.
    /// </summary>
    public PlacementResult PlacementResult { get; private set; }

    /// <summary>
    /// Доступные диаметры арматуры, мм.
    /// </summary>
    public List<int> RebarDiameters { get; } = [6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 32, 36, 40];

    /// <summary>
    /// Доступные классы арматуры хомутов.
    /// </summary>
    public IReadOnlyList<RebarClassVM> RebarClasses { get; }

    /// <summary>
    /// Подпись строки выбора пилонов из текущего проекта с количеством выбранных.
    /// </summary>
    public string PylonsFromModelLabel {
        get => _pylonsFromModelLabel;
        private set => RaiseAndSetIfChanged(ref _pylonsFromModelLabel, value);
    }

    /// <summary>
    /// Подпись строки выбора пилонов из связей с количеством выбранных.
    /// </summary>
    public string PylonsFromLinkLabel {
        get => _pylonsFromLinkLabel;
        private set => RaiseAndSetIfChanged(ref _pylonsFromLinkLabel, value);
    }

    /// <summary>
    /// Подпись строки выбора плиты со статусом выбора.
    /// </summary>
    public string SlabLabel {
        get => _slabLabel;
        private set => RaiseAndSetIfChanged(ref _slabLabel, value);
    }

    /// <summary>
    /// Выбран ли хотя бы один пилон (из модели или из связи).
    /// </summary>
    public bool IsPylonsSelected {
        get => _isPylonsSelected;
        private set => RaiseAndSetIfChanged(ref _isPylonsSelected, value);
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
    public string RebarCoverTop {
        get => _rebarCoverTop;
        set => RaiseAndSetIfChanged(ref _rebarCoverTop, value);
    }

    /// <summary>
    /// Защитный слой снизу, мм.
    /// </summary>
    public string RebarCoverBottom {
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
    public RebarClass SelectedRebarClass {
        get => _selectedRebarClass;
        set => RaiseAndSetIfChanged(ref _selectedRebarClass, value);
    }

    /// <summary>
    /// Шаг хомутов, мм.
    /// </summary>
    public string StirrupStep {
        get => _stirrupStep;
        set => RaiseAndSetIfChanged(ref _stirrupStep, value);
    }

    /// <summary>
    /// Ширина каркаса, мм.
    /// </summary>
    public string FrameWidth {
        get => _frameWidth;
        set => RaiseAndSetIfChanged(ref _frameWidth, value);
    }

    /// <summary>
    /// Длина отгибов хомутов, мм.
    /// </summary>
    public string StirrupBendLength {
        get => _stirrupBendLength;
        set => RaiseAndSetIfChanged(ref _stirrupBendLength, value);
    }

    /// <summary>
    /// Диаметр продольных стержней каркаса, мм.
    /// </summary>
    public int SelectedLongRebarDiameter {
        get => _selectedLongRebarDiameter;
        set => RaiseAndSetIfChanged(ref _selectedLongRebarDiameter, value);
    }

    /// <summary>
    /// Длина выпуска продольных стержней за крайние хомуты, мм.
    /// </summary>
    public string LongRebarAnchorageLength {
        get => _longRebarAnchorageLength;
        set => RaiseAndSetIfChanged(ref _longRebarAnchorageLength, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
        if(_isInitialized) {
            return;
        }

        _isInitialized = true;
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

        if(!_slab.TopElevation.HasValue) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSlabTopFace");
            return false;
        }

        if(IsRebarCoverFromSlab && !_slab.HasRebarCover) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorSlabRebarCoverMissing");
            return false;
        }

        if(!IsRebarCoverValid(RebarCoverTop, _slab.RebarCoverTop)) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorRebarCoverTop");
            return false;
        }

        if(!IsRebarCoverValid(RebarCoverBottom, _slab.RebarCoverBottom)) {
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

        if(!TryParseMm(FrameWidth, out int frameWidth) || frameWidth <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorFrameWidth");
            return false;
        }

        if(frameWidth <= SelectedStirrupDiameter + SelectedLongRebarDiameter) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorFrameWidthTooSmall");
            return false;
        }

        if(!TryParseMm(StirrupStep, out int stirrupStep) || stirrupStep <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorStirrupStep");
            return false;
        }

        if(!TryParseMm(StirrupBendLength, out int stirrupBendLength) || stirrupBendLength <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorStirrupBendLength");
            return false;
        }

        if(!TryParseMm(LongRebarAnchorageLength, out int anchorageLength) || anchorageLength < 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorLongRebarAnchorageLength");
            return false;
        }

        // Используем те же настройки и расчет высоты, что и при расстановке,
        // но не сохраняем конфигурацию во время проверки ввода.
        RevitSettings settings = new RevitSettings();
        UpdateSettings(settings);
        IFrameParams frameParams = new FrameParams(settings, _slab);
        if(frameParams.GetFrameHeight() <= 0) {
            ErrorText = _localizationService.GetLocalizedString("MainWindow.ErrorFrameHeight");
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Проверяет защитный слой: в режиме «из плиты» — значение из плиты,
    /// в режиме «вручную» — введенное целое число мм. Слой должен быть больше нуля
    /// и не больше половины толщины плиты.
    /// </summary>
    private bool IsRebarCoverValid(string userValueMm, double? slabValue) {
        double rebarCover;
        if(IsRebarCoverFromSlab) {
            rebarCover = slabValue.Value;
        } else if(TryParseMm(userValueMm, out int rebarCoverMm)) {
            rebarCover = UnitUtilsHelper.FromMm(rebarCoverMm);
        } else {
            return false;
        }

        return rebarCover > 0 && rebarCover <= _slab.Thickness * 0.5;
    }

    /// <summary>
    /// Разбирает введенное пользователем целое число миллиметров.
    /// </summary>
    private static bool TryParseMm(string text, out int valueMm) {
        return int.TryParse(text?.Trim(), out valueMm);
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
            UpdateSlabLabel();
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
        SelectedPylonsCount = _pylonsFromModel.Count + _pylonsFromLink.Count;
        IsPylonsSelected = SelectedPylonsCount > 0;

        PylonsFromModelLabel = string.Format(
            _localizationService.GetLocalizedString("MainWindow.PylonsFromModel"), _pylonsFromModel.Count);
        PylonsFromLinkLabel = string.Format(
            _localizationService.GetLocalizedString("MainWindow.PylonsFromLink"), _pylonsFromLink.Count);
    }

    private void UpdateSlabLabel() {
        SlabLabel = _localizationService.GetLocalizedString(
            IsSlabSelected ? "MainWindow.SlabSelected" : "MainWindow.SlabNotSelected");
    }

    /// <summary>
    /// Загружает типы выбранного семейства каркаса.
    /// </summary>
    private void LoadFamilyTypes() {
        FamilyTypes = _revitRepository.GetFamilyTypes(SelectedFamilyName);
    }

    private void ToggleParameterSettings() {
        if(IsParameterSettingsOpen) {
            CancelParameterSettings();
            return;
        }

        SetParameterEditor(new ParameterSettingsViewModel(
            FrameParameterSettings.CopyWithDefaults(_parameterSettings), _localizationService));
    }

    private void ApplyParameterSettings() {
        if(ParameterEditor != null) {
            _parameterSettings = ParameterEditor.GetSettings();
            SetParameterEditor(null);
        }
    }

    private void CancelParameterSettings() {
        SetParameterEditor(null);
    }

    private void SetParameterEditor(ParameterSettingsViewModel editor) {
        ParameterEditor = editor;
        IsParameterSettingsOpen = editor != null;
        OnPropertyChanged(nameof(MainContentVisibility));
        OnPropertyChanged(nameof(ParameterSettingsVisibility));
    }

    /// <summary>
    /// Расставляет каркасы у выбранных пилонов по сохраненным настройкам.
    /// </summary>
    private void PlaceFrames(RevitSettings settings) {
        string transactionName = _localizationService.GetLocalizedString("MainWindow.TransactionName");
        using Transaction transaction = _revitRepository.Document.StartTransaction(transactionName);

        List<IPylon> pylons = _revitRepository.GetPylons(_pylonsFromModel, _pylonsFromLink);

        IFrameParams frameParams = new FrameParams(settings, _slab);

        PlacementResult = _placementService.Run(pylons, frameParams);

        transaction.Commit();
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);
        _parameterSettings = FrameParameterSettings.CopyWithDefaults(setting?.ParameterSettings);

        SelectedFamilyName = GetAvailableValue(FamilyNames, setting?.FamilyName, DefaultSettings.FrameFamilyName);
        LoadFamilyTypes();
        SelectedFamilyType = GetAvailableValue(FamilyTypes, setting?.FamilyType, DefaultSettings.FrameFamilyTypeName);

        SelectedSlabRebarDiameter = setting?.SlabRebarDiameter ?? DefaultSettings.SlabRebarDiameterMm;
        IsRebarCoverFromUser = setting?.IsRebarCoverFromUser ?? true;
        IsRebarCoverFromSlab = !IsRebarCoverFromUser;
        RebarCoverTop = (setting?.RebarCoverTop ?? DefaultSettings.RebarCoverTopMm).ToString();
        RebarCoverBottom = (setting?.RebarCoverBottom ?? DefaultSettings.RebarCoverBottomMm).ToString();

        SelectedStirrupDiameter = setting?.StirrupDiameter ?? DefaultSettings.StirrupDiameterMm;
        SelectedRebarClass = setting?.StirrupRebarClass ?? DefaultSettings.StirrupRebarClass;
        StirrupStep = (setting?.StirrupStep ?? DefaultSettings.StirrupStepMm).ToString();
        FrameWidth = (setting?.FrameWidth ?? DefaultSettings.FrameWidthMm).ToString();
        StirrupBendLength = (setting?.StirrupBendLength ?? DefaultSettings.StirrupBendLengthMm).ToString();
        SelectedLongRebarDiameter = setting?.LongRebarDiameter ?? DefaultSettings.LongRebarDiameterMm;
        LongRebarAnchorageLength = (setting?.LongRebarAnchorageLength
                                    ?? DefaultSettings.LongRebarAnchorageLengthMm).ToString();
    }

    /// <summary>
    /// Возвращает значение свойства ViewModel к значению по умолчанию.
    /// </summary>
    /// <param name="propertyName">Имя свойства.</param>
    private void ResetToDefault(string propertyName) {
        switch(propertyName) {
            case nameof(SelectedSlabRebarDiameter):
                SelectedSlabRebarDiameter = DefaultSettings.SlabRebarDiameterMm;
                break;
            case nameof(RebarCoverTop):
                RebarCoverTop = DefaultSettings.RebarCoverTopMm.ToString();
                break;
            case nameof(RebarCoverBottom):
                RebarCoverBottom = DefaultSettings.RebarCoverBottomMm.ToString();
                break;
            case nameof(SelectedFamilyName):
                ResetFrameFamily();
                break;
            case nameof(SelectedFamilyType):
                ResetFrameFamilyType();
                break;
            case nameof(SelectedStirrupDiameter):
                SelectedStirrupDiameter = DefaultSettings.StirrupDiameterMm;
                break;
            case nameof(SelectedRebarClass):
                SelectedRebarClass = DefaultSettings.StirrupRebarClass;
                break;
            case nameof(StirrupStep):
                StirrupStep = DefaultSettings.StirrupStepMm.ToString();
                break;
            case nameof(FrameWidth):
                FrameWidth = DefaultSettings.FrameWidthMm.ToString();
                break;
            case nameof(StirrupBendLength):
                StirrupBendLength = DefaultSettings.StirrupBendLengthMm.ToString();
                break;
            case nameof(SelectedLongRebarDiameter):
                SelectedLongRebarDiameter = DefaultSettings.LongRebarDiameterMm;
                break;
            case nameof(LongRebarAnchorageLength):
                LongRebarAnchorageLength = DefaultSettings.LongRebarAnchorageLengthMm.ToString();
                break;
        }
    }

    /// <summary>
    /// Выбирает семейство и тип каркаса по умолчанию, если семейство загружено в проект.
    /// </summary>
    private void ResetFrameFamily() {
        if(!FamilyNames.Contains(DefaultSettings.FrameFamilyName)) {
            return;
        }

        SelectedFamilyName = DefaultSettings.FrameFamilyName;
        LoadFamilyTypes();
        ResetFrameFamilyType();
    }

    /// <summary>
    /// Выбирает тип каркаса по умолчанию, если он есть у выбранного семейства.
    /// </summary>
    private void ResetFrameFamilyType() {
        if(FamilyTypes != null && FamilyTypes.Contains(DefaultSettings.FrameFamilyTypeName)) {
            SelectedFamilyType = DefaultSettings.FrameFamilyTypeName;
        }
    }

    /// <summary>
    /// Создает подсказки кнопок сброса: «Сбросить к значению по умолчанию: …».
    /// </summary>
    private Dictionary<string, string> CreateDefaultValueHints() {
        string format = _localizationService.GetLocalizedString("MainWindow.ResetToDefault");
        string defaultRebarClass = _localizationService.GetLocalizedString(
            $"RebarClass.{DefaultSettings.StirrupRebarClass}");

        return new Dictionary<string, string> {
            [nameof(SelectedSlabRebarDiameter)] = string.Format(format, DefaultSettings.SlabRebarDiameterMm),
            [nameof(RebarCoverTop)] = string.Format(format, DefaultSettings.RebarCoverTopMm),
            [nameof(RebarCoverBottom)] = string.Format(format, DefaultSettings.RebarCoverBottomMm),
            [nameof(SelectedFamilyName)] = string.Format(format, DefaultSettings.FrameFamilyName),
            [nameof(SelectedFamilyType)] = string.Format(format, DefaultSettings.FrameFamilyTypeName),
            [nameof(SelectedStirrupDiameter)] = string.Format(format, DefaultSettings.StirrupDiameterMm),
            [nameof(SelectedRebarClass)] = string.Format(format, defaultRebarClass),
            [nameof(StirrupStep)] = string.Format(format, DefaultSettings.StirrupStepMm),
            [nameof(FrameWidth)] = string.Format(format, DefaultSettings.FrameWidthMm),
            [nameof(StirrupBendLength)] = string.Format(format, DefaultSettings.StirrupBendLengthMm),
            [nameof(SelectedLongRebarDiameter)] = string.Format(format, DefaultSettings.LongRebarDiameterMm),
            [nameof(LongRebarAnchorageLength)] = string.Format(format, DefaultSettings.LongRebarAnchorageLengthMm)
        };
    }

    /// <summary>
    /// Возвращает сохраненное значение, если оно есть в списке, иначе значение по умолчанию, если есть оно.
    /// </summary>
    private static string GetAvailableValue(IList<string> values, string savedValue, string defaultValue) {
        if(savedValue != null && values.Contains(savedValue)) {
            return savedValue;
        }

        return values.Contains(defaultValue) ? defaultValue : null;
    }

    /// <summary>
    /// Сохранение настроек плагина.
    /// </summary>
    /// <returns>Сохраненные настройки текущего проекта.</returns>
    private RevitSettings SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        UpdateSettings(setting);
        _pluginConfig.SaveProjectConfig();
        return setting;
    }

    /// <summary>
    /// Заполняет настройки текущими значениями окна без записи конфигурации.
    /// </summary>
    /// <param name="setting">Настройки для проверки или сохранения.</param>
    private void UpdateSettings(RevitSettings setting) {
        setting.ParameterSettings = FrameParameterSettings.CopyWithDefaults(_parameterSettings);
        setting.SlabRebarDiameter = SelectedSlabRebarDiameter;
        setting.IsRebarCoverFromUser = IsRebarCoverFromUser;

        // Сохраняются введенные вручную значения; в режиме "из плиты" слой берется из плиты при расстановке
        if(TryParseMm(RebarCoverTop, out int rebarCoverTop)) {
            setting.RebarCoverTop = rebarCoverTop;
        }

        if(TryParseMm(RebarCoverBottom, out int rebarCoverBottom)) {
            setting.RebarCoverBottom = rebarCoverBottom;
        }

        setting.FamilyName = SelectedFamilyName;
        setting.FamilyType = SelectedFamilyType;
        setting.StirrupDiameter = SelectedStirrupDiameter;
        setting.StirrupRebarClass = SelectedRebarClass;
        setting.StirrupStep = int.Parse(StirrupStep.Trim());
        setting.FrameWidth = int.Parse(FrameWidth.Trim());
        setting.StirrupBendLength = int.Parse(StirrupBendLength.Trim());
        setting.LongRebarDiameter = SelectedLongRebarDiameter;
        setting.LongRebarAnchorageLength = int.Parse(LongRebarAnchorageLength.Trim());
    }
}
