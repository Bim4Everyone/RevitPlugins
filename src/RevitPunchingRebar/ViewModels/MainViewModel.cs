using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.Revit;
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

    private string _errorText;

    public List<int> RebarDiameters { get; private set; } = [6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 32, 36, 40];
    public List<string> RebarClasses { get; private set; } = ["А500С", "А240"];

    //Общие настройки
    public ICommand SelectPylonsFromModelCommand { get; }
    public ICommand SelectPylonsFromLinkCommand { get; }
    public ICommand SelectSlabCommand { get; }

    private List<ElementId> _pylonsFromModel = [];
    private List<LinkedPylonReference> _pylonsFromLink = [];
    private ElementId _slabId;

    private int _pylonsFromModelCount;
    public int PylonsFromModelCount {
        get => _pylonsFromModelCount;
        private set => RaiseAndSetIfChanged(ref _pylonsFromModelCount, value);
    }

    private int _pylonsFromLinkCount;
    public int PylonsFromLinkCount {
        get => _pylonsFromLinkCount;
        private set => RaiseAndSetIfChanged(ref _pylonsFromLinkCount, value);
    }

    private int _selectedPylonsCount;
    public int SelectedPylonsCount {
        get => _selectedPylonsCount;
        private set => RaiseAndSetIfChanged(ref _selectedPylonsCount, value);
    }

    private string _isSlabSelected = "Нет";
    public string IsSlabSelected {
        get => _isSlabSelected;
        private set => RaiseAndSetIfChanged(ref _isSlabSelected, value);
    }

    //Арматура плиты
    private int _selectedSlabRebarDiameter;
    public int SelectedSlabRebarDiameter {
        get { return _selectedSlabRebarDiameter; }
        set {
            _selectedSlabRebarDiameter = value;
            OnPropertyChanged(nameof(SelectedSlabRebarDiameter));
        }
    }

    private bool _isRebarCoverFromSlab;
    public bool IsRebarCoverFromSlab {
        get { return _isRebarCoverFromSlab; }
        set {
            _isRebarCoverFromSlab = value;
            IsControlEnabled = !value;
            OnPropertyChanged();
        }
    }

    private bool _isRebarCoverFromUser;
    public bool IsRebarCoverFromUser {
        get { return _isRebarCoverFromUser; }
        set {
            _isRebarCoverFromUser = value;
            OnPropertyChanged();
        }
    }

    private bool _isControlEnabled;
    public bool IsControlEnabled {
        get { return _isControlEnabled; }
        set {
            _isControlEnabled = value;
            OnPropertyChanged(nameof(IsControlEnabled));
        }
    }

    private int _rebarCoverTop;
    public int RebarCoverTop {
        get { return _rebarCoverTop; }
        set {
            _rebarCoverTop = value;
            OnPropertyChanged(nameof(RebarCoverTop));
        }
    }

    private int _rebarCoverBottom;
    public int RebarCoverBottom {
        get { return _rebarCoverBottom; }
        set {
            _rebarCoverBottom = value;
            OnPropertyChanged(nameof(RebarCoverBottom));
        }
    }

    //Арматура каркасов
    public IList<string> FamilyNames { get; set; }

    private string _selectedFamilyName;
    public string SelectedFamilyName {
        get { return _selectedFamilyName; }
        set {
            _selectedFamilyName = value;
            FamilyTypes = _revitRepository.GetFamilyTypes(value);
            OnPropertyChanged(nameof(SelectedFamilyName));
        }
    }

    private IList<string> _familyTypes;
    public IList<string> FamilyTypes {
        get { return _familyTypes; }
        set {
            _familyTypes = value;
            OnPropertyChanged(nameof(FamilyTypes));
        }
    }

    private string _selectedFamilyType;
    public string SelectedFamilyType {
        get { return _selectedFamilyType; }
        set {
            _selectedFamilyType = value;
            OnPropertyChanged(nameof(SelectedFamilyType));
        }
    }

    private int _selectedStirrupDiameter;
    public int SelectedStirrupDiameter {
        get { return _selectedStirrupDiameter; }
        set {
            _selectedStirrupDiameter = value;
            OnPropertyChanged(nameof(SelectedStirrupDiameter));
        }
    }

    private string _selectedRebarClass;
    public string SelectedRebarClass {
        get { return _selectedRebarClass; }
        set {
            _selectedRebarClass = value;
            OnPropertyChanged(nameof(SelectedRebarClass));
        }
    }

    private int _stirrupStep;
    public int StirrupStep {
        get { return _stirrupStep; }
        set {
            _stirrupStep = value;
            OnPropertyChanged(nameof(StirrupStep));
        }
    }

    private int _frameWidth;
    public int FrameWidth {
        get { return _frameWidth; }
        set {
            _frameWidth = value;
            OnPropertyChanged(nameof(FrameWidth));
        }
    }

    /// <summary>
    /// Создает экземпляр основной ViewModel главного окна.
    /// </summary>
    /// <param name="pluginConfig">Настройки плагина.</param>
    /// <param name="revitRepository">Класс доступа к интерфейсу Revit.</param>
    /// <param name="placementService">Сервис расстановки каркасов.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository,
        IPunchingRebarPlacementService placementService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _placementService = placementService;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectPylonsFromModelCommand = RelayCommand.Create<Window>(SelectPylonsFromModel);
        SelectPylonsFromLinkCommand = RelayCommand.Create<Window>(SelectPylonsFromLink);
        SelectSlabCommand = RelayCommand.Create<Window>(SelectSlab);

        FamilyNames = _revitRepository.GetFamilyNames();
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
            _slabId = _revitRepository.PickSlab();
            IsSlabSelected = "Да";
        });
    }

    private void UpdatePylonsCount() {
        PylonsFromModelCount = _pylonsFromModel.Count;
        PylonsFromLinkCount = _pylonsFromLink.Count;
        SelectedPylonsCount = PylonsFromModelCount + PylonsFromLinkCount;
    }

    /// <summary>
    /// Скрывает окно на время выбора элементов в Revit и показывает его снова после выбора или отмены.
    /// </summary>
    private void SelectInRevit(Window window, Action selectAction) {
        window.Hide();
        try {
            selectAction();
        } catch(Autodesk.Revit.Exceptions.OperationCanceledException) {
            // Пользователь отменил выбор - оставляем предыдущий выбор
        } finally {
            _ = window.ShowDialog();
        }
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
    /// Текст ошибки, который отображается при неверном вводе пользователя.
    /// </summary>
    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }

    /// <summary>
    /// Метод загрузки главного окна.
    /// </summary>
    /// <remarks>В данном методе должна происходить загрузка настроек окна, а так же инициализация полей окна.</remarks>
    private void LoadView() {
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
    /// Расставляет каркасы у выбранных пилонов по сохраненным настройкам.
    /// </summary>
    private void PlaceFrames(RevitSettings settings) {
        using Transaction transaction = _revitRepository.Document.StartTransaction("Расстановка каркасов");

        List<IPylon> pylons = _revitRepository.GetPylons(_pylonsFromModel, _pylonsFromLink);

        FrameParams frameParams = new FrameParams();

        frameParams.FrameFamilyName = settings.FamilyName;
        frameParams.FrameFamilyTypeName = settings.FamilyType;
        frameParams.HostSlab = _revitRepository.GetSlab(_slabId);
        frameParams.SlabThickness = frameParams.HostSlab.Thickness;
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
    /// Метод проверки возможности выполнения команды применения настроек.
    /// </summary>
    /// <returns>В случае когда true - команда может выполниться, в случае false - нет.</returns>
    /// <remarks>
    /// В данном методе происходит валидация ввода пользователя и уведомление его о неверных значениях.
    /// В методе проверяемые свойства окна должны быть отсортированы в таком же порядке как в окне (сверху-вниз)
    /// </remarks>
    private bool CanAcceptView() {
        if(SelectedPylonsCount == 0) {
            ErrorText = "Выберите пилоны";
            return false;
        }

        if(_slabId == null) {
            ErrorText = "Выберите плиту";
            return false;
        }

        double rebarCoverTopFt = UnitUtilsHelper.FromMm(RebarCoverTop);
        double rebarCoverBottomFt = UnitUtilsHelper.FromMm(RebarCoverBottom);
        Slab slab = _revitRepository.GetSlab(_slabId);

        if(rebarCoverTopFt <= 0 || rebarCoverTopFt > slab.Thickness * 0.5) {
            ErrorText = "Введите корректный верхний защитный слой";
            return false;
        }

        if(rebarCoverBottomFt <= 0 || rebarCoverBottomFt > slab.Thickness * 0.5) {
            ErrorText = "Введите корректный нижний защитный слой";
            return false;
        }

        if(string.IsNullOrEmpty(SelectedFamilyName)) {
            ErrorText = "Выберите семейство арматурных каркасов";
            return false;
        }

        if(string.IsNullOrEmpty(SelectedFamilyType)) {
            ErrorText = "Выберите тип арматурных каркасов";
            return false;
        }

        if(string.IsNullOrEmpty(SelectedRebarClass)) {
            ErrorText = "Выберите класс арматурных стержней";
            return false;
        }

        if(StirrupStep <= 0) {
            ErrorText = "Введите корректный шаг хомутов";
            return false;
        }

        if(FrameWidth <= 0) {
            ErrorText = "Введите корректную ширину каркаса";
            return false;
        }

        ErrorText = null;
        return true;
    }

    /// <summary>
    /// Загрузка настроек плагина.
    /// </summary>
    private void LoadConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document);

        SelectedFamilyName = setting?.FamilyName ?? string.Empty;
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
            Slab slab = _revitRepository.GetSlab(_slabId);

            setting.RebarCoverTop = Convert.ToInt32(UnitUtilsHelper.ToMm(slab.RebarCoverTop));
            setting.RebarCoverBottom = Convert.ToInt32(UnitUtilsHelper.ToMm(slab.RebarCoverBottom));
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
