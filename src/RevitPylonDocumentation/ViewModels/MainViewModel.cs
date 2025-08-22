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
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.Views;

using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.ViewModels;
internal class MainViewModel : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;

    private string _errorText;
    private bool _pylonSelectedManually = false;
    private string _selectedProjectSection = string.Empty;
    private ViewFamilyType _selectedViewFamilyType;
    private DimensionType _selectedDimensionType;
    private FamilySymbol _selectedSkeletonTagType;
    private FamilySymbol _selectedRebarTagTypeWithSerif;
    private FamilySymbol _selectedRebarTagTypeWithStep;
    private FamilySymbol _selectedRebarTagTypeWithComment;

    private SpotDimensionType _selectedSpotDimensionType;
    private View _selectedGeneralViewTemplate;
    private View _selectedGeneralRebarViewTemplate;
    private View _selectedTransverseViewTemplate;
    private View _selectedTransverseRebarViewTemplate;
    private View _selectedLegend;
    private View _selectedRebarNode;
    private FamilySymbol _selectedTitleBlock;
    private List<PylonSheetInfo> _selectedHostsInfo = [];

    private bool _settingsEdited = false;

    private string _hostsInfoFilter;
    private ICollectionView _hostsInfoView;

    public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;

        SelectionSettings = new UserSelectionSettings();
        ProjectSettings = new UserProjectSettings(this, _revitRepository);
        ViewSectionSettings = new UserViewSectionSettings(this);
        SchedulesSettings = new UserSchedulesSettings(this);

        ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
        TitleBlocks = _revitRepository.TitleBlocksInProject;
        Legends = _revitRepository.LegendsInProject;
        ViewTemplatesInPj = _revitRepository.AllViewTemplates;

        DimensionTypes = _revitRepository.DimensionTypes;
        SpotDimensionTypes = _revitRepository.SpotDimensionTypes;
        RebarTagTypes = _revitRepository.RebarTagTypes;

        SetHostsInfoFilters();

        ParamValService = new ParamValueService(revitRepository);
        RebarFinder = new RebarFinderService(this, _revitRepository);
        ComparerOfElements = new ElementComparer();

        LoadViewCommand = RelayCommand.Create(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

        SelectPylonCommand = RelayCommand.Create(SelectPylon);

        ApplySettingsCommand = RelayCommand.Create(ApplySettings, CanApplySettings);
        CheckSettingsCommand = RelayCommand.Create(CheckSettings);
        GetHostMarksInGUICommand = RelayCommand.Create(GetHostMarksInGUI);

        AddScheduleFilterParamCommand = RelayCommand.Create(AddScheduleFilterParam);
        DeleteScheduleFilterParamCommand = RelayCommand.Create(DeleteScheduleFilterParam, CanChangeScheduleFilterParam);
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
    public UserSelectionSettings SelectionSettings { get; set; }

    /// <summary>
    /// Настройки параметров проекта с предыдущего сеанса
    /// </summary>
    public UserProjectSettings ProjectSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания разрезов с предыдущего сеанса
    /// </summary>
    public UserViewSectionSettings ViewSectionSettings { get; set; }

    /// <summary>
    /// Настройки параметров и правил создания спек с предыдущего сеанса
    /// </summary>
    public UserSchedulesSettings SchedulesSettings { get; set; }

    /// <summary>
    /// Сервис по получению значений параметров у элементов
    /// </summary>
    internal ParamValueService ParamValService { get; set; }

    /// <summary>
    /// Сервис по поиску арматуры в проекте или на виде
    /// </summary>
    internal RebarFinderService RebarFinder { get; set; }

    /// <summary>
    /// Компаратор для сравнения элементов по Id
    /// </summary>
    internal ElementComparer ComparerOfElements { get; }


    /// <summary>
    /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
    /// </summary>
    public ObservableCollection<string> ProjectSections { get; set; } = [];


    /// <summary>
    /// Выбранный пользователем комплект документации
    /// </summary>
    public string SelectedProjectSection {
        get => _selectedProjectSection;
        set => RaiseAndSetIfChanged(ref _selectedProjectSection, value);
    }

    /// <summary>
    /// Список всех найденных пилонов для работы в проекте (оболочек)
    /// </summary>
    public ObservableCollection<PylonSheetInfo> HostsInfo { get; set; } =
        [];

    /// <summary>
    /// Список пилонов (оболочек) для работы из выбранного пользователем комплекта документации
    /// </summary>
    public List<PylonSheetInfo> SelectedHostsInfo {
        get => _selectedHostsInfo;
        set => RaiseAndSetIfChanged(ref _selectedHostsInfo, value);
    }

    /// <summary>
    /// Рамки листов, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> TitleBlocks { get; set; } = [];

    /// <summary>
    /// Выбранная пользователем рамка листа
    /// </summary>
    public FamilySymbol SelectedTitleBlock {
        get => _selectedTitleBlock;
        set {
            RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
            ProjectSettings.TitleBlockNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Легенды, имеющиеся в проекте
    /// </summary>
    public List<View> Legends { get; set; } = [];

    /// <summary>
    /// Выбранная пользователем легенда
    /// </summary>
    public View SelectedLegend {
        get => _selectedLegend;
        set {
            RaiseAndSetIfChanged(ref _selectedLegend, value);
            ProjectSettings.LegendNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранная пользователем легенда
    /// </summary>
    public View SelectedRebarNode {
        get => _selectedRebarNode;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarNode, value);
            ProjectSettings.RebarNodeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Типоразмеры видов, имеющиеся в проекте
    /// </summary>
    public List<ViewFamilyType> ViewFamilyTypes { get; set; } = [];

    /// <summary>
    /// Выбранный пользователем типоразмер вида для создания новых видов
    /// </summary>
    public ViewFamilyType SelectedViewFamilyType {
        get => _selectedViewFamilyType;
        set {
            RaiseAndSetIfChanged(ref _selectedViewFamilyType, value);
            ViewSectionSettings.ViewFamilyTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Типоразмеры размеров, имеющиеся в проекте
    /// </summary>
    public List<DimensionType> DimensionTypes { get; set; } = [];

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    public DimensionType SelectedDimensionType {
        get => _selectedDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedDimensionType, value);
            ProjectSettings.DimensionTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Типоразмеры марок арматурных стержней, имеющиеся в проекте
    /// </summary>
    public List<FamilySymbol> RebarTagTypes { get; set; } = [];

    /// <summary>
    /// Выбранный пользователем типоразмер марки для обозначения каркаса
    /// </summary>
    public FamilySymbol SelectedSkeletonTagType {
        get => _selectedSkeletonTagType;
        set {
            RaiseAndSetIfChanged(ref _selectedSkeletonTagType, value);
            ProjectSettings.SkeletonTagTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с засечкой
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithSerif {
        get => _selectedRebarTagTypeWithSerif;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithSerif, value);
            ProjectSettings.RebarTagTypeWithSerifNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с шагом
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithStep {
        get => _selectedRebarTagTypeWithStep;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithStep, value);
            ProjectSettings.RebarTagTypeWithStepNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем типоразмер марки арматуры с количеством
    /// </summary>
    public FamilySymbol SelectedRebarTagTypeWithComment {
        get => _selectedRebarTagTypeWithComment;
        set {
            RaiseAndSetIfChanged(ref _selectedRebarTagTypeWithComment, value);
            ProjectSettings.RebarTagTypeWithCommentNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Типоразмеры высотных отметок, имеющиеся в проекте
    /// </summary>
    public List<SpotDimensionType> SpotDimensionTypes { get; set; } = [];

    /// <summary>
    /// Выбранный пользователем типоразмер высотной отметки
    /// </summary>
    public SpotDimensionType SelectedSpotDimensionType {
        get => _selectedSpotDimensionType;
        set {
            RaiseAndSetIfChanged(ref _selectedSpotDimensionType, value);
            ProjectSettings.SpotDimensionTypeNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Перечень шаблонов видов в проекте
    /// </summary>
    public List<ViewSection> ViewTemplatesInPj { get; set; } = [];

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов
    /// </summary>
    public View SelectedGeneralViewTemplate {
        get => _selectedGeneralViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralViewTemplate, value);
            ViewSectionSettings.GeneralViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида основных видов армирования
    /// </summary>
    public View SelectedGeneralRebarViewTemplate {
        get => _selectedGeneralRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedGeneralRebarViewTemplate, value);
            ViewSectionSettings.GeneralRebarViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов
    /// </summary>
    public View SelectedTransverseViewTemplate {
        get => _selectedTransverseViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseViewTemplate, value);
            ViewSectionSettings.TransverseViewTemplateNameTemp = value?.Name;
        }
    }

    /// <summary>
    /// Выбранный пользователем шаблон вида поперечных видов армирования
    /// </summary>
    public View SelectedTransverseRebarViewTemplate {
        get => _selectedTransverseRebarViewTemplate;
        set {
            RaiseAndSetIfChanged(ref _selectedTransverseRebarViewTemplate, value);
            ViewSectionSettings.TransverseRebarViewTemplateNameTemp = value?.Name;
        }
    }

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
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceRebarSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация материалов
    /// </summary>
    public ViewSchedule ReferenceMaterialSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для системной арматуры
    /// </summary>
    public ViewSchedule ReferenceSystemPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная ведомость деталей для IFC арматуры
    /// </summary>
    public ViewSchedule ReferenceIfcPartsSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonSchedule { get; set; }

    /// <summary>
    /// Эталонная спецификация арматуры
    /// </summary>
    public ViewSchedule ReferenceSkeletonByElemsSchedule { get; set; }

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
              _revitRepository.Document.StartTransaction("Получение возможных комплектов документации")) {
            _revitRepository.GetHostData(this);

            transaction.RollBack();
        }

        HostsInfo = new ObservableCollection<PylonSheetInfo>(_revitRepository.HostsInfo);
        ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
        OnPropertyChanged(nameof(HostsInfo));
        OnPropertyChanged(nameof(ProjectSections));
    }


    /// <summary>
    /// Обновляет список пилонов в соответствии с выбранным комплектом документации. 
    /// Отрабатывает в момент выбора комплекта документации в ComboBox
    /// </summary>
    private void GetHostMarksInGUI() {
        SelectedHostsInfo = [.. HostsInfo
            .Where(item => item.ProjectSection.Equals(SelectedProjectSection))
            .ToList()];

        SetHostsInfoFilters();
    }

    /// <summary>
    /// Дает возможность пользователю выбрать вручную нужный для работы пилон
    /// </summary>
    private void SelectPylon() {
        var elementid = _revitRepository.ActiveUIDocument.Selection
            .PickObject(ObjectType.Element, "Выберите пилон").ElementId;
        var element = _revitRepository.Document.GetElement(elementid);

        if(element != null) {
            HostsInfo.Clear();
            SelectedHostsInfo.Clear();
            SelectedProjectSection = string.Empty;
            _pylonSelectedManually = true;

            _revitRepository.GetHostData(this, [element]);

            HostsInfo = new ObservableCollection<PylonSheetInfo>(_revitRepository.HostsInfo);
            ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
            OnPropertyChanged(nameof(HostsInfo));
            OnPropertyChanged(nameof(ProjectSections));


            if(HostsInfo.Count > 0) {
                SelectedHostsInfo.Add(HostsInfo.FirstOrDefault());
                HostsInfo.FirstOrDefault().IsCheck = true;
                SelectedProjectSection = ProjectSections.FirstOrDefault();
            }
        }

        SelectionSettings.NeedWorkWithGeneralView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularView = false;
        SelectionSettings.NeedWorkWithTransverseViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseViewSecond = false;
        SelectionSettings.NeedWorkWithTransverseViewThird = false;
        SelectionSettings.NeedWorkWithRebarSchedule = false;
        SelectionSettings.NeedWorkWithMaterialSchedule = false;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = false;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = false;
        SelectionSettings.NeedWorkWithLegend = false;
        SelectionSettings.NeedWorkWithGeneralRebarView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = false;
        SelectionSettings.NeedWorkWithSkeletonSchedule = false;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = false;
        SelectionSettings.NeedWorkWithRebarNode = false;

        var mainWindow = new MainWindow {
            DataContext = this
        };
        mainWindow.ShowDialog();
    }


    /// <summary>
    /// Применяет изменения настроек плагина (передает данные из временных переменных в постоянные, по которым работает плагин)
    /// </summary>
    private void ApplySettings() {
        ErrorText = string.Empty;

        ProjectSettings.ApplyProjectSettings();
        ViewSectionSettings.ApplyViewSectionsSettings();
        SchedulesSettings.ApplySchedulesSettings();

        // Получаем заново список заполненных разделов проекта
        if(!_pylonSelectedManually) {
            GetRebarProjectSections();
        }

        FindReferenceSchedules();

        FindGeneralViewTemplate();
        FindGeneralRebarViewTemplate();
        FindTransverseViewTemplate();
        FindTransverseRebarViewTemplate();
        FindViewFamilyType();
        FindDimensionType();
        FindSpotDimensionType();
        FindSkeletonTagType();
        FindRebarTagTypeWithSerif();
        FindRebarTagTypeWithStep();
        FindRebarTagTypeWithComment();
        FindLegend();
        FindRebarNode();
        FindTitleBlock();

        _settingsEdited = false;
    }

    private void CheckSettings() {
        if(SelectedTitleBlock is null) {
            ErrorText = "Не выбран типоразмер рамки листа";
            return;
        }

        if(SelectedViewFamilyType is null) {
            ErrorText = "Не выбран типоразмер создаваемого вида";
            return;
        }

        if(SelectedGeneralViewTemplate is null) {
            ErrorText = "Не выбран шаблон основных видов";
            return;
        }

        if(SelectedGeneralRebarViewTemplate is null) {
            ErrorText = "Не выбран шаблон основных видов армирования";
            return;
        }

        if(SelectedTransverseViewTemplate is null) {
            ErrorText = "Не выбран шаблон поперечных видов";
            return;
        }

        if(SelectedTransverseRebarViewTemplate is null) {
            ErrorText = "Не выбран шаблон поперечных видов армирования";
            return;
        }

        if(SelectedLegend is null) {
            ErrorText = "Не выбрана легенда примечаний";
            return;
        }

        if(SelectedRebarNode is null) {
            ErrorText = "Не выбран узел армирования";
            return;
        }

        if(SelectedViewFamilyType is null) {
            ErrorText = "Не выбран типоразмер создаваемого вида";
            return;
        }

        if(SelectedDimensionType is null) {
            ErrorText = "Не выбран типоразмер для простановки размеров";
            return;
        }

        if(SelectedSpotDimensionType is null) {
            ErrorText = "Не выбран типоразмер высотной отметки";
            return;
        }

        if(SelectedSkeletonTagType is null) {
            ErrorText = "Не задан типоразмер марки каркаса";
            return;
        }
        if(SelectedRebarTagTypeWithSerif is null) {
            ErrorText = "Не задан типоразмер марки арматуры с засечкой";
            return;
        }
        if(SelectedRebarTagTypeWithStep is null) {
            ErrorText = "Не задан типоразмер марки арматуры без засечки";
            return;
        }

        ProjectSettings.CheckProjectSettings();
        ViewSectionSettings.CheckViewSectionsSettings();
    }

    /// <summary>
    /// Определяет можно ли применить изменения настроек плагина (передать данные из временные переменных в постоянные, по которым работает плагин). 
    /// Доступно при изменении одного из параметров настроек
    /// </summary>
    private bool CanApplySettings() {
        return _settingsEdited;
    }

    /// <summary>
    /// Анализирует выбранные пользователем элементы вида, создает лист, виды, спецификации, и размещает их на листе
    /// </summary>
    private void CreateSheetsNViews() {
        using var transaction = _revitRepository.Document.StartTransaction("Документатор пилонов");
        foreach(var hostsInfo in SelectedHostsInfo) {
            hostsInfo.Manager.WorkWithCreation();
        }
        transaction.Commit();
    }

    /// <summary>
    /// Ищет эталонные спецификации по указанным именам. На основе эталонных спек создаются спеки для пилонов путем копирования
    /// </summary>
    private void FindReferenceSchedules() {
        ReferenceRebarSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.RebarScheduleName));
        ReferenceMaterialSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.MaterialScheduleName));
        ReferenceSystemPartsSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.SystemPartsScheduleName));
        ReferenceIfcPartsSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.IfcPartsScheduleName));

        ReferenceSkeletonSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.SkeletonScheduleName));
        ReferenceSkeletonByElemsSchedule =
            _revitRepository.AllScheduleViews.FirstOrDefault(sch =>
                sch.Name.Equals(SchedulesSettings.SkeletonByElemsScheduleName));
    }

    /// <summary>
    /// Получает шаблон для основных видов по имени
    /// </summary>
    public void FindGeneralViewTemplate() {
        if(ViewSectionSettings.GeneralViewTemplateName != string.Empty) {
            SelectedGeneralViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.GeneralViewTemplateName));
        }
    }

    /// <summary>
    /// Получает шаблон для основных видов армирования по имени
    /// </summary>
    public void FindGeneralRebarViewTemplate() {
        if(ViewSectionSettings.GeneralRebarViewTemplateName != string.Empty) {
            SelectedGeneralRebarViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.GeneralRebarViewTemplateName));
        }
    }

    /// <summary>
    /// Получает шаблон для поперечных видов по имени
    /// </summary>
    public void FindTransverseViewTemplate() {
        if(ViewSectionSettings.TransverseViewTemplateName != string.Empty) {
            SelectedTransverseViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.TransverseViewTemplateName));
        }
    }

    /// <summary>
    /// Получает шаблон для поперечных видов армирования по имени
    /// </summary>
    public void FindTransverseRebarViewTemplate() {
        if(ViewSectionSettings.TransverseRebarViewTemplateName != string.Empty) {
            SelectedTransverseRebarViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.TransverseRebarViewTemplateName));
        }
    }

    /// <summary>
    /// Получает типоразмер вида для создаваемых видов
    /// </summary>
    public void FindViewFamilyType() {
        if(ViewSectionSettings.ViewFamilyTypeName != string.Empty) {
            SelectedViewFamilyType = ViewFamilyTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ViewSectionSettings.ViewFamilyTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер для простановки размеров
    /// </summary>
    public void FindDimensionType() {
        if(ProjectSettings.DimensionTypeName != string.Empty) {
            SelectedDimensionType = DimensionTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.DimensionTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер высотной отметки
    /// </summary>
    public void FindSpotDimensionType() {
        if(ProjectSettings.SpotDimensionTypeName != string.Empty) {
            SelectedSpotDimensionType = SpotDimensionTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.SpotDimensionTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматурного каркаса
    /// </summary>
    public void FindSkeletonTagType() {
        if(ProjectSettings.SkeletonTagTypeName != string.Empty) {
            SelectedSkeletonTagType = RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.SkeletonTagTypeName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с засечкой
    /// </summary>
    public void FindRebarTagTypeWithSerif() {
        if(ProjectSettings.RebarTagTypeWithSerifName != string.Empty) {
            SelectedRebarTagTypeWithSerif = RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.RebarTagTypeWithSerifName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с шагом
    /// </summary>
    public void FindRebarTagTypeWithStep() {
        if(ProjectSettings.RebarTagTypeWithStepName != string.Empty) {
            SelectedRebarTagTypeWithStep = RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.RebarTagTypeWithStepName));
        }
    }

    /// <summary>
    /// Получает типоразмер марки арматуры с количеством
    /// </summary>
    public void FindRebarTagTypeWithComment() {
        if(ProjectSettings.RebarTagTypeWithCommentName != string.Empty) {
            SelectedRebarTagTypeWithComment = RebarTagTypes
                .FirstOrDefault(familyType => familyType.Name.Equals(ProjectSettings.RebarTagTypeWithCommentName));
        }
    }

    /// <summary>
    /// Получает легенду примечания по имени
    /// </summary>
    public void FindLegend() {
        if(ProjectSettings.LegendName != string.Empty) {
            SelectedLegend = Legends
                .FirstOrDefault(view => view.Name.Contains(ProjectSettings.LegendName));
        }
    }

    /// <summary>
    /// Получает легенду узла армирования по имени
    /// </summary>
    public void FindRebarNode() {
        if(ProjectSettings.RebarNodeName != string.Empty) {
            SelectedRebarNode = Legends
                .FirstOrDefault(view => view.Name.Contains(ProjectSettings.RebarNodeName));
        }
    }

    /// <summary>
    /// Получает типоразмер рамки листа по имени типа
    /// </summary>
    public void FindTitleBlock() {
        if(ProjectSettings.TitleBlockName != string.Empty) {
            SelectedTitleBlock = TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name.Contains(ProjectSettings.TitleBlockName));
        }
    }


    /// <summary>
    /// Добавляет новое имя параметра фильтра спецификаций в настройках плагина
    /// </summary>
    private void AddScheduleFilterParam() {
        SchedulesSettings.ParamsForScheduleFilters.Add(
            new ScheduleFilterParamHelper("Введите название", "Введите название"));
        SettingsChanged();
    }

    /// <summary>
    /// Удаляет выбранное имя параметра фильтра спецификаций в настройках плагина
    /// </summary>
    private void DeleteScheduleFilterParam() {
        List<ScheduleFilterParamHelper> forDel = [];

        foreach(ScheduleFilterParamHelper param in SchedulesSettings.ParamsForScheduleFilters) {
            if(param.IsCheck) {
                forDel.Add(param);
            }
        }

        foreach(ScheduleFilterParamHelper param in forDel) {
            SchedulesSettings.ParamsForScheduleFilters.Remove(param);
        }

        SettingsChanged();
    }

    /// <summary>
    /// Ставит флаг, который показывает,что свойства изменились
    /// </summary>
    private void SettingsChanged() {
        _settingsEdited = true;
    }

    /// <summary>
    /// Определяет можно ли удалить выбранное имя параметра фильтра спецификаций в настройках плагина
    /// True, если выбрана штриховка в списке штриховок в настройках плагина
    /// </summary> 
    private bool CanChangeScheduleFilterParam() {
        foreach(var param in SchedulesSettings.ParamsForScheduleFilters) {
            if(param.IsCheck) { return true; }
        }

        return false;
    }

    /// <summary>
    /// Задает фильтрацию списка марок пилонов
    /// </summary>
    private void SetHostsInfoFilters() {
        _hostsInfoView = CollectionViewSource.GetDefaultView(SelectedHostsInfo);
        _hostsInfoView.Filter = item =>
                String.IsNullOrEmpty(HostsInfoFilter)
                    ? true
                    : ((PylonSheetInfo) item).PylonKeyName.IndexOf(HostsInfoFilter,
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
        foreach(PylonSheetInfo pylonSheetInfo in SelectedHostsInfo) {
            if(String.IsNullOrEmpty(HostsInfoFilter)
                   ? true
                   : (pylonSheetInfo.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >=
                  0)) {
                pylonSheetInfo.IsCheck = true;
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
        foreach(PylonSheetInfo pylonSheetInfo in SelectedHostsInfo) {
            if(String.IsNullOrEmpty(HostsInfoFilter)
                   ? true
                   : (pylonSheetInfo.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >=
                  0)) {
                pylonSheetInfo.IsCheck = false;
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
        SelectionSettings.NeedWorkWithRebarSchedule = true;
        SelectionSettings.NeedWorkWithMaterialSchedule = true;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = true;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = true;
        SelectionSettings.NeedWorkWithLegend = true;
        SelectionSettings.NeedWorkWithGeneralRebarView = true;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = true;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = true;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = true;
        SelectionSettings.NeedWorkWithSkeletonSchedule = true;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = true;
        SelectionSettings.NeedWorkWithRebarNode = true;
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
        SelectionSettings.NeedWorkWithRebarSchedule = false;
        SelectionSettings.NeedWorkWithMaterialSchedule = false;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = false;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = false;
        SelectionSettings.NeedWorkWithLegend = false;
        SelectionSettings.NeedWorkWithGeneralRebarView = false;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = false;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = false;
        SelectionSettings.NeedWorkWithSkeletonSchedule = false;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = false;
        SelectionSettings.NeedWorkWithRebarNode = false;
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
        SelectionSettings.NeedWorkWithRebarSchedule = !SelectionSettings.NeedWorkWithRebarSchedule;
        SelectionSettings.NeedWorkWithMaterialSchedule = !SelectionSettings.NeedWorkWithMaterialSchedule;
        SelectionSettings.NeedWorkWithSystemPartsSchedule = !SelectionSettings.NeedWorkWithSystemPartsSchedule;
        SelectionSettings.NeedWorkWithIfcPartsSchedule = !SelectionSettings.NeedWorkWithIfcPartsSchedule;
        SelectionSettings.NeedWorkWithLegend = !SelectionSettings.NeedWorkWithLegend;
        SelectionSettings.NeedWorkWithGeneralRebarView = !SelectionSettings.NeedWorkWithGeneralRebarView;
        SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView = !SelectionSettings.NeedWorkWithGeneralPerpendicularRebarView;
        SelectionSettings.NeedWorkWithTransverseRebarViewFirst = !SelectionSettings.NeedWorkWithTransverseRebarViewFirst;
        SelectionSettings.NeedWorkWithTransverseRebarViewSecond = !SelectionSettings.NeedWorkWithTransverseRebarViewSecond;
        SelectionSettings.NeedWorkWithSkeletonSchedule = !SelectionSettings.NeedWorkWithSkeletonSchedule;
        SelectionSettings.NeedWorkWithSkeletonByElemsSchedule = !SelectionSettings.NeedWorkWithSkeletonByElemsSchedule;
        SelectionSettings.NeedWorkWithRebarNode = !SelectionSettings.NeedWorkWithRebarNode;
    }
}
