using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Xml.Linq;

using Autodesk.AdvanceSteel.CADAccess;
using Autodesk.AdvanceSteel.Modelling;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
//using Autodesk.SteelConnectionsDB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;


using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.Models.UserSettings;
using RevitPylonDocumentation.Views;

using Transaction = Autodesk.Revit.DB.Transaction;
using View = Autodesk.Revit.DB.View;

namespace RevitPylonDocumentation.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private string _errorText;
        private bool _pylonSelectedManually = false;
        private string _selectedProjectSection = string.Empty;
        private ViewFamilyType _selectedViewFamilyType;
        private View _selectedGeneralViewTemplate;
        private View _selectedTransverseViewTemplate;
        private View _selectedLegend;
        private FamilySymbol _selectedTitleBlock;
        private List<PylonSheetInfo> _selectedHostsInfo = new List<PylonSheetInfo>();

        public bool SettingsEdited = false;

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

            SetHostsInfoFilters();

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


        public UserSelectionSettings SelectionSettings { get; set; }
        public UserProjectSettings ProjectSettings { get; set; }
        public UserViewSectionSettings ViewSectionSettings { get; set; }
        public UserSchedulesSettings SchedulesSettings { get; set; }


        /// <summary>
        /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
        /// </summary>
        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();


        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public string SelectedProjectSection {
            get => _selectedProjectSection;
            set => this.RaiseAndSetIfChanged(ref _selectedProjectSection, value);
        }

        /// <summary>
        /// Список всех найденных пилонов для работы в проекте (оболочек)
        /// </summary>
        public ObservableCollection<PylonSheetInfo> HostsInfo { get; set; } = new ObservableCollection<PylonSheetInfo>();

        /// <summary>
        /// Список пилонов (оболочек) для работы из выбранного пользователем комплекта документации
        /// </summary>
        public List<PylonSheetInfo> SelectedHostsInfo {
            get => _selectedHostsInfo;
            set => this.RaiseAndSetIfChanged(ref _selectedHostsInfo, value);
        }

        /// <summary>
        /// Рамки листов, имеющиеся в проекте
        /// </summary>
        public List<FamilySymbol> TitleBlocks { get; set; } = new List<FamilySymbol>();

        /// <summary>
        /// Выбранная пользователем рамка листа
        /// </summary>
        public FamilySymbol SelectedTitleBlock {
            get => _selectedTitleBlock;
            set {
                this.RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
                ProjectSettings.TitleBlockNameTemp = value?.Name;
            }
        }

        /// <summary>
        /// Легенды, имеющиеся в проекте
        /// </summary>
        public List<View> Legends { get; set; } = new List<View>();
        
        /// <summary>
        /// Выбранная пользователем легенда
        /// </summary>
        public View SelectedLegend {
            get => _selectedLegend;
            set {
                this.RaiseAndSetIfChanged(ref _selectedLegend, value);
                ProjectSettings.LegendNameTemp = value?.Name;
            }
        }

        /// <summary>
        /// Типоразмеры видов, имеющиеся в проекте
        /// </summary>
        public List<ViewFamilyType> ViewFamilyTypes { get; set; } = new List<ViewFamilyType>();
        
        /// <summary>
        /// Выбранный пользователем типоразмер вида для создания новых видов
        /// </summary>
        public ViewFamilyType SelectedViewFamilyType {
            get => _selectedViewFamilyType;
            set {
                this.RaiseAndSetIfChanged(ref _selectedViewFamilyType, value);
                ViewSectionSettings.ViewFamilyTypeNameTemp = value?.Name;
            }
        }

        /// <summary>
        /// Перечень шаблонов видов в проекте
        /// </summary>
        public List<ViewSection> ViewTemplatesInPj { get; set; } = new List<ViewSection>();
        
        /// <summary>
        /// Выбранный пользователем шаблон вида основных видов
        /// </summary>
        public View SelectedGeneralViewTemplate {
            get => _selectedGeneralViewTemplate;
            set {
                this.RaiseAndSetIfChanged(ref _selectedGeneralViewTemplate, value);
                ViewSectionSettings.GeneralViewTemplateNameTemp = value?.Name;
            }
        }

        /// <summary>
        /// Выбранный пользователем шаблон вида поперечных видов
        /// </summary>
        public View SelectedTransverseViewTemplate {
            get => _selectedTransverseViewTemplate;
            set {
                this.RaiseAndSetIfChanged(ref _selectedTransverseViewTemplate, value);
                TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP = value?.Name;
            }
        }





        // Инфо по пилонам
        /// <summary>
        /// Список всех марок пилонов (напр., "12.30.25-20⌀32")
        /// </summary>
        public ObservableCollection<string> HostMarks { get; set; } = new ObservableCollection<string>();
        /// <summary>
        /// Список меток основ, которые выбрал пользователь
        /// </summary>
        public System.Collections.IList SelectedHostMarks { get; set; }

        public string HostMarkForSearch {
            get => _hostMarkForSearch;
            set {
                _hostMarkForSearch = value;
            }
        }



        #region Свойства для параметров и правил
        #region Параметры
        public string PROJECT_SECTION { get; set; } = "обр_ФОП_Раздел проекта";
        public string PROJECT_SECTION_TEMP {
            get => _projectSectionTemp;
            set {
                this.RaiseAndSetIfChanged(ref _projectSectionTemp, value);
                _edited = true;
            }
        }

        public string MARK { get; set; } = "Марка";
        public string MARK_TEMP {
            get => _markTemp;
            set {
                this.RaiseAndSetIfChanged(ref _markTemp, value);
                _edited = true;
            }
        }

        // dispatcher grouping
        public string DISPATCHER_GROUPING_FIRST { get; set; } = "_Группа видов 1";
        public string DISPATCHER_GROUPING_FIRST_TEMP {
            get => _dispatcherGroupingFirstTemp;
            set {
                this.RaiseAndSetIfChanged(ref _dispatcherGroupingFirstTemp, value);
                _edited = true;
            }
        }
        public string DISPATCHER_GROUPING_SECOND { get; set; } = "_Группа видов 2";
        public string DISPATCHER_GROUPING_SECOND_TEMP {
            get => _dispatcherGroupingSecondTemp;
            set {
                this.RaiseAndSetIfChanged(ref _dispatcherGroupingSecondTemp, value);
                _edited = true;
            }
        }

        public string SHEET_SIZE { get; set; } = "А";
        public string SHEET_SIZE_TEMP {
            get => _sheetSizeTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetSizeTemp, value);
                _edited = true;
            }
        }

        public string SHEET_COEFFICIENT { get; set; } = "х";
        public string SHEET_COEFFICIENT_TEMP {
            get => _sheetCoefficientTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetCoefficientTemp, value);
                _edited = true;
            }
        }


        public string SHEET_PREFIX { get; set; } = "Пилон ";
        public string SHEET_PREFIX_TEMP {
            get => _sheetPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetPrefixTemp, value);
                _edited = true;
            }
        }

        public string SHEET_SUFFIX { get; set; } = "";
        public string SHEET_SUFFIX_TEMP {
            get => _sheetSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _sheetSuffixTemp, value);
                _edited = true;
            }
        }



        public string GENERAL_VIEW_PREFIX { get; set; } = "";
        public string GENERAL_VIEW_PREFIX_TEMP {
            get => _generalViewPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPrefixTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_SUFFIX { get; set; } = "";
        public string GENERAL_VIEW_SUFFIX_TEMP {
            get => _generalViewSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewSuffixTemp, value);
                _edited = true;
            }
        }


        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; } = "Пилон ";
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP {
            get => _generalViewPerpendicularPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPerpendicularPrefixTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; } = "_Перпендикулярный";
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP {
            get => _generalViewPerpendicularSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewPerpendicularSuffixTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_TEMPLATE_NAME { get; set; } = "КЖ0.2_пилоны_орг.ур.-2";
        public string GENERAL_VIEW_TEMPLATE_NAME_TEMP {
            get => _generalViewTemplateNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewTemplateNameTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_X_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_X_OFFSET_TEMP {
            get => _generalViewXOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewXOffsetTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_Y_TOP_OFFSET { get; set; } = "2300";
        public string GENERAL_VIEW_Y_TOP_OFFSET_TEMP {
            get => _generalViewYTopOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewYTopOffsetTemp, value);
                _edited = true;
            }
        }

        public string GENERAL_VIEW_Y_BOTTOM_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP {
            get => _generalViewYBottomOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _generalViewYBottomOffsetTemp, value);
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_FIRST_PREFIX_TEMP {
            get => _transverseViewFirstPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewFirstPrefixTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; } = "_Сеч.1-1";
        public string TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP {
            get => _transverseViewFirstSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewFirstSuffixTemp, value);
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_SECOND_PREFIX_TEMP {
            get => _transverseViewSecondPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewSecondPrefixTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; } = "_Сеч.2-2";
        public string TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP {
            get => _transverseViewSecondSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewSecondSuffixTemp, value);
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_THIRD_PREFIX_TEMP {
            get => _transverseViewThirdPrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewThirdPrefixTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; } = "_Сеч.3-3";
        public string TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP {
            get => _transverseViewThirdSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewThirdSuffixTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_TEMPLATE_NAME { get; set; } = "";
        public string TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP {
            get => _transverseViewTemplateNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewTemplateNameTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_X_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_X_OFFSET_TEMP {
            get => _transverseViewXOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewXOffsetTemp, value);
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_Y_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_Y_OFFSET_TEMP {
            get => _transverseViewYOffsetTemp;
            set {
                this.RaiseAndSetIfChanged(ref _transverseViewYOffsetTemp, value);
                _edited = true;
            }
        }

        public string REBAR_SCHEDULE_PREFIX { get; set; } = "Пилон ";
        public string REBAR_SCHEDULE_PREFIX_TEMP {
            get => _rebarSchedulePrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _rebarSchedulePrefixTemp, value);
                _edited = true;
            }
        }

        public string REBAR_SCHEDULE_SUFFIX { get; set; } = "";
        public string REBAR_SCHEDULE_SUFFIX_TEMP {
            get => _rebarScheduleSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _rebarScheduleSuffixTemp, value);
                _edited = true;
            }
        }


        public string MATERIAL_SCHEDULE_PREFIX { get; set; } = "!СМ_Пилон ";
        public string MATERIAL_SCHEDULE_PREFIX_TEMP {
            get => _materialSchedulePrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _materialSchedulePrefixTemp, value);
                _edited = true;
            }
        }

        public string MATERIAL_SCHEDULE_SUFFIX { get; set; } = "";
        public string MATERIAL_SCHEDULE_SUFFIX_TEMP {
            get => _materialScheduleSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _materialScheduleSuffixTemp, value);
                _edited = true;
            }
        }

        public string SYSTEM_PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_СИС_";
        public string SYSTEM_PARTS_SCHEDULE_PREFIX_TEMP {
            get => _systemPartsSchedulePrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _systemPartsSchedulePrefixTemp, value);
                _edited = true;
            }
        }

        public string SYSTEM_PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string SYSTEM_PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _systemPartsScheduleSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _systemPartsScheduleSuffixTemp, value);
                _edited = true;
            }
        }


        public string IFC_PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_IFC_";
        public string IFC_PARTS_SCHEDULE_PREFIX_TEMP {
            get => _IFCPartsSchedulePrefixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _IFCPartsSchedulePrefixTemp, value);
                _edited = true;
            }
        }

        public string IFC_PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string IFC_PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _IFCPartsScheduleSuffixTemp;
            set {
                this.RaiseAndSetIfChanged(ref _IFCPartsScheduleSuffixTemp, value);
                _edited = true;
            }
        }

        private bool _needWorkWithTransverseViewFirst = false;
        public bool NeedWorkWithTransverseViewFirst {
            get => _needWorkWithTransverseViewFirst;
            set {
                _needWorkWithTransverseViewFirst = value;
            }
        }

        private bool _needWorkWithTransverseViewSecond = false;
        public bool NeedWorkWithTransverseViewSecond {
            get => _needWorkWithTransverseViewSecond;
            set {
                _needWorkWithTransverseViewSecond = value;
            }
        }

        private bool _needWorkWithTransverseViewThird = false;
        public bool NeedWorkWithTransverseViewThird {
            get => _needWorkWithTransverseViewThird;
            set {
                _needWorkWithTransverseViewThird = value;
            }
        }

        private bool _needWorkWithRebarSchedule = false;
        public bool NeedWorkWithRebarSchedule {
            get => _needWorkWithRebarSchedule;
            set {
                _needWorkWithRebarSchedule = value;
            }
        }

        private bool _needWorkWithMaterialSchedule = false;
        public bool NeedWorkWithMaterialSchedule {
            get => _needWorkWithMaterialSchedule;
            set {
                _needWorkWithMaterialSchedule = value;
            }
        }

        private bool _needWorkWithSystemPartsSchedule = false;
        public bool NeedWorkWithSystemPartsSchedule {
            get => _needWorkWithSystemPartsSchedule;
            set {
                _needWorkWithSystemPartsSchedule = value;
            }
        }

        private bool _needWorkWithIFCPartsSchedule = false;
        public bool NeedWorkWithIFCPartsSchedule {
            get => _needWorkWithIFCPartsSchedule;
            set {
                _needWorkWithIFCPartsSchedule = value;
            }
        }



        public string REBAR_SCHEDULE_NAME { get; set; } = "!СА_Базовая";
        public string REBAR_SCHEDULE_NAME_TEMP {
            get => _rebarScheduleNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _rebarScheduleNameTemp, value);
                _edited = true;
            }
        }

        public string MATERIAL_SCHEDULE_NAME { get; set; } = "!СМ";
        public string MATERIAL_SCHEDULE_NAME_TEMP {
            get => _materialScheduleNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _materialScheduleNameTemp, value);
                _edited = true;
            }
        }


        public string SYSTEM_PARTS_SCHEDULE_NAME { get; set; } = "!ВД_СИС";
        public string SYSTEM_PARTS_SCHEDULE_NAME_TEMP {
            get => _systemPartsScheduleNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _systemPartsScheduleNameTemp, value);
                _edited = true;
            }
        }

        public string IFC_PARTS_SCHEDULE_NAME { get; set; } = "!ВД_IFC";
        public string IFC_PARTS_SCHEDULE_NAME_TEMP {
            get => _IFCPartsScheduleNameTemp;
            set {
                this.RaiseAndSetIfChanged(ref _IFCPartsScheduleNameTemp, value);
                _edited = true;
            }
        }


        public string REBAR_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string REBAR_SCHEDULE_DISP1_TEMP {
            get => _rebarScheduleDisp1Temp;
            set {
                this.RaiseAndSetIfChanged(ref _rebarScheduleDisp1Temp, value);
                _edited = true;
            }
        }
        public string MATERIAL_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string MATERIAL_SCHEDULE_DISP1_TEMP {
            get => _materialScheduleDisp1Temp;
            set {
                this.RaiseAndSetIfChanged(ref _materialScheduleDisp1Temp, value);
                _edited = true;
            }
        }
        public string SYSTEM_PARTS_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string SYSTEM_PARTS_SCHEDULE_DISP1_TEMP {
            get => _systemPartsScheduleDisp1Temp;
            set {
                this.RaiseAndSetIfChanged(ref _systemPartsScheduleDisp1Temp, value);
                _edited = true;
            }
        }
        public string IFC_PARTS_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string IFC_PARTS_SCHEDULE_DISP1_TEMP {
            get => _IFCPartsScheduleDisp1Temp;
            set {
                this.RaiseAndSetIfChanged(ref _IFCPartsScheduleDisp1Temp, value);
                _edited = true;
            }
        }



        public string REBAR_SCHEDULE_DISP2 { get; set; } = "!СА_Пилоны";
        public string REBAR_SCHEDULE_DISP2_TEMP {
            get => _rebarScheduleDisp2Temp;
            set {
                this.RaiseAndSetIfChanged(ref _rebarScheduleDisp2Temp, value);
                _edited = true;
            }
        }


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
        public ViewSchedule ReferenceIFCPartsSchedule { get; set; }

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
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
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
            if(ErrorText == string.Empty) {
                return true;
            }
            return false;
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
        private void GetRebarProjectSections() 
        {
            // Пользователь может перезадать параметр раздела, поэтому сначала чистим
            ProjectSections.Clear();

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Получение возможных комплектов документации")) {

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
        private void GetHostMarksInGUI() 
        {
            SelectedHostsInfo = new List<PylonSheetInfo>(HostsInfo
                .Where(item => item.ProjectSection.Equals(SelectedProjectSection))
                .ToList());

            SetHostsInfoFilters();
        }

        /// <summary>
        /// Дает возможность пользователю выбрать вручную нужный для работы пилон
        /// </summary>
        private void SelectPylon() {

            ElementId elementid = _revitRepository.ActiveUIDocument.Selection.PickObject(ObjectType.Element, "Выберите пилон").ElementId;
            Element element = _revitRepository.Document.GetElement(elementid);

            if(element != null) {

                HostsInfo.Clear();
                SelectedHostsInfo.Clear();
                SelectedProjectSection = string.Empty;
                _pylonSelectedManually = true;

                _revitRepository.GetHostData(this, new List<Element> { element });

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
            SelectionSettings.NeedWorkWithIFCPartsSchedule = false;
            SelectionSettings.NeedWorkWithLegend = false;


            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = this;
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
            FindTransverseViewTemplate();
            FindViewFamilyType();
            FindLegend();
            FindTitleBlock();

            SettingsEdited = false;
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
            if(SelectedTransverseViewTemplate is null) {
                ErrorText = "Не выбран шаблон поперечных видов";
                return;
            }
            if(SelectedLegend is null) {
                ErrorText = "Не выбрана легенда примечаний";
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
            if(SettingsEdited) {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Анализирует выбранные пользователем элементы вида, создает лист, виды, спецификации, и размещает их на листе
        /// </summary>
        private void CreateSheetsNViews() {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор пилонов")) {

                foreach(PylonSheetInfo hostsInfo in SelectedHostsInfo) {

                    hostsInfo.Manager.WorkWithCreation();
                }
                transaction.Commit();
            }
        }

        /// <summary>
        /// Ищет эталонные спецификации по указанным именам. На основе эталонных спек создаются спеки для пилонов путем копирования
        /// </summary>
        private void FindReferenceSchedules() {
            ReferenceRebarSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.RebarScheduleName)) as ViewSchedule;
            ReferenceMaterialSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.MaterialScheduleName)) as ViewSchedule;
            ReferenceSystemPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.SytemPartsScheduleName)) as ViewSchedule;
            ReferenceIFCPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.IFCPartsScheduleName)) as ViewSchedule;
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
        /// Получает шаблон для поперечных видов по имени
        /// </summary>
        public void FindTransverseViewTemplate() {

            if(ViewSectionSettings.TransverseViewTemplateName != string.Empty) {
                SelectedTransverseViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.TransverseViewTemplateName));
            }
            
        }
        /// <summary>
        /// Получает типоразмер вида для создаваемых видов
        /// </summary>
        public void FindViewFamilyType() {

            if(ViewSectionSettings.ViewFamilyTypeName != string.Empty) {
                SelectedViewFamilyType = ViewFamilyTypes
                .FirstOrDefault(familyTypes => familyTypes.Name.Equals(ViewSectionSettings.ViewFamilyTypeName));
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

            SchedulesSettings.ParamsForScheduleFilters.Add(new ScheduleFilterParamHelper("Введите название", "Введите название"));
            SettingsChanged();
        }

        /// <summary>
        /// Удаляет выбранное имя параметра фильтра спецификаций в настройках плагина
        /// </summary>
        private void DeleteScheduleFilterParam() {

            List <ScheduleFilterParamHelper> forDel= new List <ScheduleFilterParamHelper>();

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
        /// Добавляет новое имя параметра фильтра спецификаций в настройках плагина
        /// </summary>
        private void SettingsChanged() {

            SettingsEdited = true;
        }

        /// <summary>
        /// Определяет можно ли удалить выбранное имя параметра фильтра спецификаций в настройках плагина
        /// True, если выбрана штриховка в списке штриховок в настройках плагина
        /// </summary> 
        private bool CanChangeScheduleFilterParam() {

            foreach(ScheduleFilterParamHelper param in SchedulesSettings.ParamsForScheduleFilters) {
                if(param.IsCheck) { return true; }
            }
            return false;
        }

        /// <summary>
        /// Задает фильтрацию списка марок пилонов
        /// </summary>
        private void SetHostsInfoFilters() {

            _hostsInfoView = CollectionViewSource.GetDefaultView(SelectedHostsInfo);
            _hostsInfoView.Filter = item => String.IsNullOrEmpty(HostsInfoFilter) ? true :
                ((PylonSheetInfo) item).PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >= 0;
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
                
                if(String.IsNullOrEmpty(HostsInfoFilter) ? true :
                (pylonSheetInfo.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
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

                if(String.IsNullOrEmpty(HostsInfoFilter) ? true :
                (pylonSheetInfo.PylonKeyName.IndexOf(HostsInfoFilter, StringComparison.OrdinalIgnoreCase) >= 0)) {
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
            SelectionSettings.NeedWorkWithIFCPartsSchedule = true;
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
            SelectionSettings.NeedWorkWithRebarSchedule = false;
            SelectionSettings.NeedWorkWithMaterialSchedule = false;
            SelectionSettings.NeedWorkWithSystemPartsSchedule = false;
            SelectionSettings.NeedWorkWithIFCPartsSchedule = false;
            SelectionSettings.NeedWorkWithLegend = false;
        }
    }
}