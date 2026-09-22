using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPunchingRebar.Models;
using RevitPunchingRebar.Models.Interfaces;
using RevitPunchingRebar.Models.SelectionFilters;

namespace RevitPunchingRebar.ViewModels;

/// <summary>
/// Основная ViewModel главного окна плагина.
/// </summary>
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string _errorText;

    public SelectHandler SelectHandler { get; set; }
    public ExternalEvent SelectExternalEvent { get; set; }

    public PlacementHandler PlacementHandler { get; set; }
    public ExternalEvent PlacementExternalEvent { get; set; }

    public List<int> RebarDiameters { get; private set; } = [6, 8, 10, 12, 14, 16, 18, 20, 22, 25, 28, 32, 36, 40];
    public List<string> RebarClasses { get; private set; } = ["А500С", "А240"];

    public event Action RequestHideWindow;
    public event Action RequestShowWindow;
    public event Action RequestCloseWindow;

    //Общие настройки
    public ICommand SelectPylonsFromModelCommand => new RelayCommand(_ => StartSelection(SelectionMode.MultiFromModel));
    public ICommand SelectPylonsFromLinkCommand => new RelayCommand(_ => StartSelection(SelectionMode.MultiFromLink));
    public ICommand SelectSlabCommand => new RelayCommand(_ => StartSelection(SelectionMode.Single));

    private string _selectedSlabId;
    public string SelectedSlabId {
        get { return _selectedSlabId; }
        set { 
            _selectedSlabId = value;
            SelectedSlabeChanged();
        }
    }

    private List<string> _selectedPylonsFromModel;
    public List<string> SelectedPylonsFromModel {
        get { return _selectedPylonsFromModel; }
        set {
            _selectedPylonsFromModel = value;

            if (value.Count != 0) {
                SelectedPylonsFromLink.Clear();
                SelectedPylonsCount = _selectedPylonsFromModel.Count;
            }
        }
    }

    private Dictionary<string, string> _selectedPylonsFromLink;
    public Dictionary<string, string> SelectedPylonsFromLink {
        get { return _selectedPylonsFromLink; }
        set {
            _selectedPylonsFromLink = value;

            if(value.Count != 0) {
                SelectedPylonsFromModel.Clear();
                SelectedPylonsCount = _selectedPylonsFromLink.Count;
            }
        }
    }

    private int _selectedPylonsCount;
    public int SelectedPylonsCount {
        get { return _selectedPylonsCount; }
        set { 
            _selectedPylonsCount = value;
            OnPropertyChanged(nameof(SelectedPylonsCount));
        }
    }

    private string _isSlabSelected;

    public string IsSlabSelected {
        get { return _isSlabSelected; }
        set { 
            _isSlabSelected = value;
            OnPropertyChanged(nameof(IsSlabSelected));
        }
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
    /// <param name="localizationService">Интерфейс доступа к сервису локализации.</param>
    public MainViewModel(
        PluginConfig pluginConfig,
        RevitRepository revitRepository) {

        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
        CancelViewCommand = RelayCommand.Create(CancelView);

        FamilyNames = _revitRepository.GetFamilyNames();
        SelectedPylonsFromLink = new Dictionary<string, string>();
    }

    private void StartSelection(SelectionMode selectionMode) {
        SelectHandler.SelectionMode = selectionMode;
        RequestHideWindow?.Invoke();
        SelectExternalEvent.Raise();
    }

    public void OnSelected() {
        RequestShowWindow?.Invoke();
    }

    private void SelectedSlabeChanged() {
        if(SelectedSlabId != null) {
            IsSlabSelected = "Да";
        } else {
            IsSlabSelected = "Нет";
        }
    }

    /// <summary>
    /// Команда загрузки главного окна.
    /// </summary>
    public ICommand LoadViewCommand { get; }
    
    /// <summary>
    /// Команда применения настроек главного окна. (запуск плагина)
    /// </summary>
    /// <remarks>В случаях, когда используется немодальное окно, требуется данную команду удалять.</remarks>
    public ICommand AcceptViewCommand { get; }
    public ICommand CancelViewCommand { get; }

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
        SaveConfig();

        RequestCloseWindow?.Invoke();
        PlacementExternalEvent.Raise();
    }

    private void CancelView() {
        RequestCloseWindow?.Invoke();
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

        if(SelectedSlabId == null) {
            ErrorText = "Выберите плиту";
            return false;
        }

        double rebarCoverTopFt = _revitRepository.FromMmToFt(RebarCoverTop);
        double rebarCoverBottomFt = _revitRepository.FromMmToFt(RebarCoverBottom);
        Slab slab = _revitRepository.GetSlab(SelectedSlabId);

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
        SelectedSlabId = setting?.SlabId ?? null;
        SelectedPylonsFromModel = setting?.PylonsFromModel ?? [];
        SelectedPylonsFromLink = setting?.PylonsFromLink ?? [];

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
    private void SaveConfig() {
        RevitSettings setting = _pluginConfig.GetSettings(_revitRepository.Document)
                                ?? _pluginConfig.AddSettings(_revitRepository.Document);

        setting.PylonsFromModel = SelectedPylonsFromModel;
        setting.PylonsFromLink = SelectedPylonsFromLink;
        setting.SlabId = SelectedSlabId;
        setting.SlabRebarDiameter = SelectedSlabRebarDiameter;
        setting.IsRebarCoverFromUser = IsRebarCoverFromUser;

        if(IsRebarCoverFromSlab) {
            Slab slab = _revitRepository.GetSlab(SelectedSlabId);

            setting.RebarCoverTop = Convert.ToInt32(_revitRepository.FromFtToMm(slab.RebarCoverTop));
            setting.RebarCoverBottom = Convert.ToInt32(_revitRepository.FromFtToMm(slab.RebarCoverBottom));

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
        PlacementHandler.Settings = setting;
    }
}
