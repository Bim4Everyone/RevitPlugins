using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject;
using Ninject.Syntax;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.Services;
using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.ViewModels.UserSettings;
using RevitPylonDocumentation.Views;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly IResolutionRoot _resolutionRoot;

    private string _errorText;
    private bool _pylonSelectedManually = false;

    private List<PylonSheetInfoVM> _selectedHostsInfoVM = [];

    private bool _settingsEdited = false;

    private string _hostsInfoFilter;
    private ICollectionView _hostsInfoView;

    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository,
                         ILocalizationService localizationService, IResolutionRoot resolutionRoot) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _localizationService = localizationService;
        _resolutionRoot = resolutionRoot;

        SelectionSettings = new UserSelectionSettingsVM();
        VerticalViewSettings = new UserVerticalViewSettingsPageVM(this, _localizationService);
        TransverseViewSettings = new UserTransverseViewSettingsPageVM(this, _localizationService);
        SchedulesSettings = new UserSchedulesSettingsPageVM(this);
        ScheduleFiltersSettings = new UserScheduleFiltersSettingsPageVM(this, _localizationService);
        LegendsAndAnnotationsSettings = new UserLegendsAndAnnotationsSettingsVM(this, _localizationService);
        PylonSettings = new UserPylonSettingsVM(this, _revitRepository, _localizationService);
        ProjectSettings = new UserProjectSettingsVM(this, _revitRepository, _localizationService);
        SheetSettings = new UserSheetSettingsVM(this, _revitRepository, _localizationService);

        ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
        TitleBlocks = _revitRepository.TitleBlocksInProject;
        Legends = _revitRepository.LegendsInProject;
        ViewTemplatesInPj = _revitRepository.AllViewTemplates;
        SchedulesInPj = _revitRepository.AllScheduleViews;

        DimensionTypes = _revitRepository.DimensionTypes;
        SpotDimensionTypes = _revitRepository.SpotDimensionTypes;
        RebarTagTypes = _revitRepository.RebarTagTypes;
        DetailComponentsTypes = _revitRepository.DetailComponentTypes;
        TypicalAnnotationsTypes = _revitRepository.TypicalAnnotationsTypes;

        SetHostsInfoFilters();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectPylonCommand = RelayCommand.Create(SelectPylon);

        ApplySettingsCommand = RelayCommand.Create(ApplySettings, CanApplySettings);
        CheckSettingsCommand = RelayCommand.Create(CheckSettings);
        GetHostMarksInGUICommand = RelayCommand.Create(GetHostMarksInGUI);

        AddScheduleFilterParamCommand = RelayCommand.Create(ScheduleFiltersSettings.AddScheduleFilterParam);
        DeleteScheduleFilterParamCommand = RelayCommand.Create(ScheduleFiltersSettings.DeleteScheduleFilterParam,
                                                               ScheduleFiltersSettings.CanChangeScheduleFilterParam);
        SettingsChangedCommand = RelayCommand.Create(SettingsChanged);

        ClearHostsInfoFilterInGUICommand = RelayCommand.Create(ClearHostsInfoFilterInGUI);
        SelectAllHostsInfoInGUICommand = RelayCommand.Create(SelectAllHostsInfoInGUI);
        UnselectAllHostsInfoInGUICommand = RelayCommand.Create(UnselectAllHostsInfoInGUI);

        SelectAllFuncInGUICommand = RelayCommand.Create(SelectAllFuncInGUI);
        UnselectAllFuncInGUICommand = RelayCommand.Create(UnselectAllFuncInGUI);
        InvertAllFuncInGUICommand = RelayCommand.Create(InvertAllFuncInGUI);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }
    public ICommand ApplySettingsCommand { get; }
    public ICommand CheckSettingsCommand { get; }
    public ICommand GetHostMarksInGUICommand { get; }
    public ICommand AddScheduleFilterParamCommand { get; }
    public ICommand DeleteScheduleFilterParamCommand { get; }
    public ICommand SettingsChangedCommand { get; }
    public ICommand SelectPylonCommand { get; }
    public ICommand ClearHostsInfoFilterInGUICommand { get; }
    public ICommand SelectAllHostsInfoInGUICommand { get; }
    public ICommand UnselectAllHostsInfoInGUICommand { get; }
    public ICommand SelectAllFuncInGUICommand { get; }
    public ICommand UnselectAllFuncInGUICommand { get; }
    public ICommand InvertAllFuncInGUICommand { get; }

    /// <summary>
    /// Настройки выбора пользователя (с какими компонентами должен работать плагин) с предыдущего сеанса
    /// </summary>
    public UserSelectionSettingsVM SelectionSettings { get; set; }

    /// <summary>
    /// Настройки параметров проекта с предыдущего сеанса
    /// </summary>
    public UserProjectSettingsVM ProjectSettings { get; set; }

    /// <summary>
    /// Настройки параметров листов с предыдущего сеанса
    /// </summary>
    public UserSheetSettingsVM SheetSettings { get; set; }

    /// <summary>
    /// Настройки параметров пилонов с предыдущего сеанса
    /// </summary>
    public UserPylonSettingsVM PylonSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания вертикальных разрезов с предыдущего сеанса
    /// </summary>
    public UserVerticalViewSettingsPageVM VerticalViewSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания горизонтальных разрезов с предыдущего сеанса
    /// </summary>
    public UserTransverseViewSettingsPageVM TransverseViewSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания спек с предыдущего сеанса
    /// </summary>
    public UserSchedulesSettingsPageVM SchedulesSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания фильтров спек с предыдущего сеанса
    /// </summary>
    public UserScheduleFiltersSettingsPageVM ScheduleFiltersSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания легенд и типовых аннотаций с предыдущего сеанса
    /// </summary>
    public UserLegendsAndAnnotationsSettingsVM LegendsAndAnnotationsSettings { get; set; }

    /// <summary>
    /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
    /// </summary>
    public ObservableCollection<string> ProjectSections { get; set; } = [];


    /// <summary>
    /// Список всех найденных пилонов для работы в проекте (оболочек)
    /// </summary>
    public ObservableCollection<PylonSheetInfoVM> HostsInfoVM { get; set; } = [];

    /// <summary>
    /// Список пилонов (оболочек) для работы из выбранного пользователем комплекта документации
    /// </summary>
    public List<PylonSheetInfoVM> SelectedHostsInfoVM {
        get => _selectedHostsInfoVM;
        set => RaiseAndSetIfChanged(ref _selectedHostsInfoVM, value);
    }

    /// <summary>
    /// Рамки листов, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> TitleBlocks { get; set; } = [];

    /// <summary>
    /// Легенды, имеющиеся в проекте
    /// </summary>
    public List<View> Legends { get; set; } = [];

    /// <summary>
    /// Типоразмеры видов, имеющиеся в проекте
    /// </summary>
    public List<ViewFamilyType> ViewFamilyTypes { get; set; } = [];

    /// <summary>
    /// Типоразмеры размеров, имеющиеся в проекте
    /// </summary>
    public List<DimensionType> DimensionTypes { get; set; } = [];

    /// <summary>
    /// Типоразмеры марок арматурных стержней, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> RebarTagTypes { get; set; } = [];

    /// <summary>
    /// Типоразмеры элементов узла, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> DetailComponentsTypes { get; set; } = [];

    /// <summary>
    /// Типоразмеры типовых аннотаций, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> TypicalAnnotationsTypes { get; set; } = [];

    /// <summary>
    /// Типоразмеры высотных отметок, имеющиеся в проекте
    /// </summary>
    public List<SpotDimensionType> SpotDimensionTypes { get; set; } = [];

    /// <summary>
    /// Перечень шаблонов видов в проекте
    /// </summary>
    public List<ViewSection> ViewTemplatesInPj { get; set; } = [];

    /// <summary>
    /// Перечень видов спецификаций в проекте
    /// </summary>
    public List<ViewSchedule> SchedulesInPj { get; set; } = [];

    // Инфо по пилонам
    /// <summary>
    /// Список всех марок пилонов (напр., "12.30.25-20⌀32")
    /// </summary>
    public ObservableCollection<string> HostMarks { get; set; } = [];

    /// <summary>
    /// Список меток основ, которые выбрал пользователь
    /// </summary>
    public System.Collections.IList SelectedHostMarks { get; set; }


    /// <summary>
    /// Фильтр списка марок пилонов
    /// </summary>
    public string HostsInfoFilter {
        get => _hostsInfoFilter;
        set {
            if(value != _hostsInfoFilter) {
                _hostsInfoFilter = value;
                _hostsInfoView.Refresh();
                OnPropertyChanged(nameof(HostsInfoFilter));
            }
        }
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }



    /// <summary>
    /// Метод, отрабатывающий при загрузке окна
    /// </summary>
    private void LoadView() {
        LoadConfig();

        ApplySettings();
        CheckSettings();
    }

    /// <summary>
    /// Метод, отрабатывающий при нажатии кнопки "Ок"
    /// </summary>
    private void AcceptView() {
        SaveConfig();
        CreateSheetsNViews();
    }

    /// <summary>
    /// Определяет можно ли запустить работу плагина
    /// </summary>
    private bool CanAcceptView() {
        return ErrorText == string.Empty;
    }

    /// <summary>
    /// Подгружает параметры плагина с предыдущего запуска
    /// </summary>
    private void LoadConfig() {
        var settings = _pluginConfig.GetSettings(_revitRepository.Document);

        if(settings != null) {
            _pluginConfig.GetConfigProps(settings, this);
        }
    }

    /// <summary>
    /// Сохраняет параметры плагина для следующего запуска
    /// </summary>
    private void SaveConfig() {
        var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                       ?? _pluginConfig.AddSettings(_revitRepository.Document);

        _pluginConfig.SetConfigProps(settings, this);
        _pluginConfig.SaveProjectConfig();
    }


    /// <summary>
    /// Получает названия комплектов документации по пилонам
    /// </summary>
    private void GetRebarProjectSections() {
        // Пользователь может перезадать параметр раздела, поэтому сначала чистим
        ProjectSections.Clear();

        using(var transaction =
              _revitRepository.Document.StartTransaction("Search for possible documentation sets")) {
            _revitRepository.GetHostData(this);

            transaction.RollBack();
        }

        HostsInfoVM = new ObservableCollection<PylonSheetInfoVM>(_revitRepository.HostsInfo);
        ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
        OnPropertyChanged(nameof(HostsInfoVM));
        OnPropertyChanged(nameof(ProjectSections));
    }


    /// <summary>
    /// Обновляет список пилонов в соответствии с выбранным комплектом документации. 
    /// Отрабатывает в момент выбора комплекта документации в ComboBox
    /// </summary>
    private void GetHostMarksInGUI() {
        SelectedHostsInfoVM = [.. HostsInfoVM
            .Where(item => item.ProjectSection.Equals(SelectionSettings.SelectedProjectSection))
            .ToList()];

        SetHostsInfoFilters();
    }

    /// <summary>
    /// Дает возможность пользователю выбрать вручную нужный для работы пилон
    /// </summary>
    private void SelectPylon() {
        var elementid = _revitRepository.ActiveUIDocument.Selection
            .PickObject(ObjectType.Element, _localizationService.GetLocalizedString("VM.SelectPylon")).ElementId;
        var element = _revitRepository.Document.GetElement(elementid);

        if(element != null) {
            HostsInfoVM.Clear();
            SelectedHostsInfoVM.Clear();
            SelectionSettings.SelectedProjectSection = string.Empty;
            _pylonSelectedManually = true;

            _revitRepository.GetHostData(this, [element]);

            HostsInfoVM = new ObservableCollection<PylonSheetInfoVM>(_revitRepository.HostsInfo);
            ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
            OnPropertyChanged(nameof(HostsInfoVM));
            OnPropertyChanged(nameof(ProjectSections));


            if(HostsInfoVM.Count > 0) {
                SelectedHostsInfoVM.Add(HostsInfoVM.FirstOrDefault());
                HostsInfoVM.FirstOrDefault().IsCheck = true;
                SelectionSettings.SelectedProjectSection = ProjectSections.FirstOrDefault();
            }
        }

        SelectionSettings.NeedWorkWithGeneralView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularView = false;
        SelectionSettings.NeedWorkWithTransverseViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseViewSecond = false;
        SelectionSettings.NeedWorkWithTransverseViewThird = false;
        SelectionSettings.NeedWorkWithMaterialSchedule = false;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = false;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = false;
        SelectionSettings.NeedWorkWithLegend = false;
        SelectionSettings.NeedWorkWithGeneralRebarView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = false;
        SelectionSettings.NeedWorkWithSkeletonSchedule = false;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = false;

        _resolutionRoot.Get<MainWindow>().ShowDialog();
    }


    /// <summary>
    /// Применяет изменения настроек плагина (передает данные из временных переменных в постоянные, по которым работает плагин)
    /// </summary>
    private void ApplySettings() {
        ErrorText = string.Empty;

        VerticalViewSettings.ApplySettings();
        TransverseViewSettings.ApplySettings();
        SchedulesSettings.ApplySettings();
        ScheduleFiltersSettings.ApplySettings();
        LegendsAndAnnotationsSettings.ApplySettings();
        PylonSettings.ApplySettings();
        ProjectSettings.ApplySettings();
        SheetSettings.ApplySettings();

        // Получаем заново список заполненных разделов проекта
        if(!_pylonSelectedManually) {
            GetRebarProjectSections();
        }

        VerticalViewSettings.FindGeneralViewTemplate();
        VerticalViewSettings.FindGeneralRebarViewTemplate();
        VerticalViewSettings.FindVerticalViewFamilyType();
        TransverseViewSettings.FindTransverseViewTemplate();
        TransverseViewSettings.FindTransverseRebarViewTemplate();
        TransverseViewSettings.FindTransverseViewFamilyType();

        SchedulesSettings.FindSkeletonSchedule();
        SchedulesSettings.FindSkeletonByElemsSchedule();
        SchedulesSettings.FindMaterialSchedule();
        SchedulesSettings.FindSystemPartsSchedule();
        SchedulesSettings.FindIfcPartsSchedule();

        ProjectSettings.FindDimensionType();
        ProjectSettings.FindSpotDimensionType();
        ProjectSettings.FindSkeletonTagType();
        ProjectSettings.FindRebarTagTypeWithSerif();
        ProjectSettings.FindRebarTagTypeWithStep();
        ProjectSettings.FindRebarTagTypeWithComment();
        ProjectSettings.FindUniversalTagType();
        ProjectSettings.FindBreakLineType();
        ProjectSettings.FindConcretingJointType();
        LegendsAndAnnotationsSettings.FindLegend();
        SheetSettings.FindTitleBlock();

        _settingsEdited = false;
        SaveConfig();
    }

    /// <summary>
    /// Определяет можно ли применить изменения настроек плагина (передать данные из временные переменных в постоянные, 
    /// по которым работает плагин). Доступно при изменении одного из параметров настроек
    /// </summary>
    private bool CanApplySettings() {
        return _settingsEdited;
    }

    private void CheckSettings() {
        if(!VerticalViewSettings.CheckSettings())
            return;
        if(!TransverseViewSettings.CheckSettings())
            return;
        if(!LegendsAndAnnotationsSettings.CheckSettings())
            return;
        if(!PylonSettings.CheckSettings())
            return;
        if(!ProjectSettings.CheckSettings())
            return;
        if(!SheetSettings.CheckSettings())
            return;
    }

    /// <summary>
    /// Анализирует выбранные пользователем элементы вида, создает лист, виды, спецификации, и размещает их на листе
    /// </summary>
    private void CreateSheetsNViews() {
        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("MainWindow.Title"));

        var settings = new CreationSettings(
            SelectionSettings.GetSettings<UserSelectionSettings>(),
            VerticalViewSettings.GetSettings<UserVerticalViewSettings>(),
            TransverseViewSettings.GetSettings<UserTransverseViewSettings>(),
            SchedulesSettings.GetSettings<UserSchedulesSettings>(),
            ScheduleFiltersSettings.GetSettings<UserScheduleFiltersSettings>(),
            LegendsAndAnnotationsSettings.GetSettings<UserLegendsAndAnnotationsSettings>(),
            PylonSettings.GetSettings<UserPylonSettings>(),
            ProjectSettings.GetSettings<UserProjectSettings>(),
            SheetSettings.GetSettings<UserSheetSettings>());

        var paramValService = new ParamValueService(_revitRepository);
        var rebarFinder = new RebarFinderService(settings, _revitRepository, paramValService);

        foreach(var hostsInfoVM in SelectedHostsInfoVM) {
            if(!hostsInfoVM.IsCheck) { continue; }

            var hostsInfo = hostsInfoVM.GetPylonSheetInfo(settings, _revitRepository, paramValService, rebarFinder);
            hostsInfo.InitializeComponents();
            hostsInfo.Manager.WorkWithCreation();
        }
        transaction.Commit();
    }

    /// <summary>
    /// Ставит флаг, который показывает,что свойства изменились
    /// </summary>
    public void SettingsChanged() {
        _settingsEdited = true;
        ErrorText = _localizationService.GetLocalizedString("VM.NeedSaveSettings");
    }

    /// <summary>
    /// Задает фильтрацию списка марок пилонов
    /// </summary>
    private void SetHostsInfoFilters() {
        _hostsInfoView = CollectionViewSource.GetDefaultView(SelectedHostsInfoVM);
        _hostsInfoView.Filter = item =>
                String.IsNullOrEmpty(HostsInfoFilter)
                    ? true
                    : ((PylonSheetInfoVM) item).PylonKeyName.IndexOf(HostsInfoFilter,
                    StringComparison.OrdinalIgnoreCase) >= 0;
    }


    /// <summary>
    /// Обнуление строки фильтра привязанного к тексту, через который фильтруется список марок пилонов в GUI
    /// Работает при нажатии "x" в правой части области поиска
    /// </summary>
    private void ClearHostsInfoFilterInGUI() {
        HostsInfoFilter = string.Empty;
    }

    /// <summary>
    /// Ставит галку выбора всем маркам пилонов, видимым в GUI.
    /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка марок пилонов в GUI
    /// </summary>
    private void SelectAllHostsInfoInGUI() {
        foreach(PylonSheetInfoVM pylonSheetInfoVM in SelectedHostsInfoVM) {
            if(String.IsNullOrEmpty(HostsInfoFilter)
                   ? true
                   : (pylonSheetInfoVM.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >=
                  0)) {
                pylonSheetInfoVM.IsCheck = true;
            }
        }

        // Иначе не работало:
        _hostsInfoView.Refresh();
    }


    /// <summary>
    /// Снимает галку выбора у всех марок пилонов, видимых в GUI.
    /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка марок пилонов в GUI
    /// </summary>
    private void UnselectAllHostsInfoInGUI() {
        foreach(PylonSheetInfoVM pylonSheetInfoVM in SelectedHostsInfoVM) {
            if(String.IsNullOrEmpty(HostsInfoFilter)
                   ? true
                   : (pylonSheetInfoVM.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >=
                  0)) {
                pylonSheetInfoVM.IsCheck = false;
            }
        }

        // Иначе не работало:
        _hostsInfoView.Refresh();
    }


    /// <summary>
    /// Ставит галку выбора всем элементам, доступным для создания, видимым в GUI.
    /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка тумблеров в GUI
    /// </summary>
    private void SelectAllFuncInGUI() {
        SelectionSettings.NeedWorkWithGeneralView = true;
        SelectionSettings.NeedWorkWithGeneralPerpendicularView = true;
        SelectionSettings.NeedWorkWithTransverseViewFirst = true;
        SelectionSettings.NeedWorkWithTransverseViewSecond = true;
        SelectionSettings.NeedWorkWithTransverseViewThird = true;
        SelectionSettings.NeedWorkWithGeneralRebarView = true;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = true;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = true;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = true;
        SelectionSettings.NeedWorkWithTransverseRebarViewThird = true;

        SelectionSettings.NeedWorkWithSkeletonSchedule = true;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = true;
        SelectionSettings.NeedWorkWithMaterialSchedule = true;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = true;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = true;
        SelectionSettings.NeedWorkWithLegend = true;
    }


    /// <summary>
    /// Снимает галку выбора у всех, доступных для создания, видимых в GUI.
    /// Отрабатывает при нажатии на кнопку "Выбрать все" возле списка тумблеров в GUI
    /// </summary>
    private void UnselectAllFuncInGUI() {
        SelectionSettings.NeedWorkWithGeneralView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularView = false;
        SelectionSettings.NeedWorkWithTransverseViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseViewSecond = false;
        SelectionSettings.NeedWorkWithTransverseViewThird = false;
        SelectionSettings.NeedWorkWithGeneralRebarView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewThird = false;

        SelectionSettings.NeedWorkWithMaterialSchedule = false;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = false;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = false;
        SelectionSettings.NeedWorkWithLegend = false;
        SelectionSettings.NeedWorkWithSkeletonSchedule = false;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = false;
    }


    /// <summary>
    /// Инвертирует галки выбора у всех, доступных для создания, видимых в GUI.
    /// Отрабатывает при нажатии на кнопку "Инвертировать" возле списка тумблеров в GUI
    /// </summary>
    private void InvertAllFuncInGUI() {
        SelectionSettings.NeedWorkWithGeneralView = !SelectionSettings.NeedWorkWithGeneralView;
        SelectionSettings.NeedWorkWithGeneralPerpendicularView = !SelectionSettings.NeedWorkWithGeneralPerpendicularView;
        SelectionSettings.NeedWorkWithTransverseViewFirst = !SelectionSettings.NeedWorkWithTransverseViewFirst;
        SelectionSettings.NeedWorkWithTransverseViewSecond = !SelectionSettings.NeedWorkWithTransverseViewSecond;
        SelectionSettings.NeedWorkWithTransverseViewThird = !SelectionSettings.NeedWorkWithTransverseViewThird;
        SelectionSettings.NeedWorkWithGeneralRebarView = !SelectionSettings.NeedWorkWithGeneralRebarView;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = !SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = !SelectionSettings.NeedWorkWithTransverseRebarViewFirst;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = !SelectionSettings.NeedWorkWithTransverseRebarViewSecond;
        SelectionSettings.NeedWorkWithTransverseRebarViewThird = !SelectionSettings.NeedWorkWithTransverseRebarViewThird;

        SelectionSettings.NeedWorkWithSkeletonSchedule = !SelectionSettings.NeedWorkWithSkeletonSchedule;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = !SelectionSettings.NeedWorkWithSkeletonByElemsSchedule;
        SelectionSettings.NeedWorkWithMaterialSchedule = !SelectionSettings.NeedWorkWithMaterialSchedule;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = !SelectionSettings.NeedWorkWithSystemPartsSchedule;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = !SelectionSettings.NeedWorkWithIfcPartsSchedule;
        SelectionSettings.NeedWorkWithLegend = !SelectionSettings.NeedWorkWithLegend;
    }
}
