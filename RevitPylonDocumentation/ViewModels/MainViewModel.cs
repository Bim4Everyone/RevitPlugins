using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

using MS.WindowsAPICodePack.Internal;

using RevitPylonDocumentation.Models;
using RevitPylonDocumentation.Models.PylonSheetNView;
using RevitPylonDocumentation.Models.UserSettings;

using Reference = Autodesk.Revit.DB.Reference;
using Transaction = Autodesk.Revit.DB.Transaction;
using Transform = Autodesk.Revit.DB.Transform;
using View = Autodesk.Revit.DB.View;
using Wall = Autodesk.Revit.DB.Wall;

namespace RevitPylonDocumentation.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        public readonly RevitRepository _revitRepository;

        /// <summary>
        /// Указывает вносил ли пользователь изменения в настройки
        /// </summary>
        private string _errorText;
        private string _selectedProjectSection = string.Empty;
        private List<PylonSheetInfo> _selectedHostsInfo = new List<PylonSheetInfo>();

        public bool SettingsEdited;



        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            SelectionSettings = new UserSelectionSettings();
            ProjectSettings = new UserProjectSettings(this);
            ViewSectionSettings = new UserViewSectionSettings(this);
            SchedulesSettings = new UserSchedulesSettings(this);

            GetRebarProjectSections();

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;

            TitleBlocks = _revitRepository.TitleBlocksInProject;
            SelectedTitleBlocks = TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name == ProjectSettings.DEF_TITLEBLOCK_NAME);

            Legends = _revitRepository.LegendsInProject;
            SelectedLegend = Legends
                .FirstOrDefault(view => view.Name.Contains("илон"));


            ViewTemplatesInPj = _revitRepository.AllViewTemplates;
            FindGeneralViewTemplate();
            FindTransverseViewTemplate();

            FindReferenceSchedules();
            
            GetHostMarksInGUICommand = new RelayCommand(GetHostMarksInGUI);



            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView);
            TestCommand = new RelayCommand(CreateSheetsNViews);

            ApplySettingsCommands = new RelayCommand(ApplySettings, CanApplySettings);

            AddScheduleFilterParamCommand = new RelayCommand(AddScheduleFilterParam);
            DeleteScheduleFilterParamCommand = new RelayCommand(DeleteScheduleFilterParam, CanChangeScheduleFilterParam);
        }

        

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }


        public ICommand ApplySettingsCommands { get; }
        public ICommand GetHostMarksInGUICommand { get; }
        public ICommand TestCommand { get; }



        public ICommand AddScheduleFilterParamCommand { get; }
        public ICommand DeleteScheduleFilterParamCommand { get; }




        public UserSelectionSettings SelectionSettings { get; set; }
        public UserProjectSettings ProjectSettings { get; set; }
        public UserViewSectionSettings ViewSectionSettings { get; set; }
        public UserSchedulesSettings SchedulesSettings { get; set; }



        /// <summary>
        /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
        /// </summary>
        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();

        public ObservableCollection<PylonSheetInfo> HostsInfo { get; set; } = new ObservableCollection<PylonSheetInfo>();





        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public string SelectedProjectSection {
            get => _selectedProjectSection;
            set => this.RaiseAndSetIfChanged(ref _selectedProjectSection, value);
        }


        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public List<PylonSheetInfo> SelectedHostsInfo {
            get => _selectedHostsInfo;
            set => this.RaiseAndSetIfChanged(ref _selectedHostsInfo, value);
        }




        // Вспомогательные для документации
        /// <summary>
        /// Рамки листов, имеющиеся в проекте
        /// </summary>
        public List<FamilySymbol> TitleBlocks { get; set; } = new List<FamilySymbol>();
        /// <summary>
        /// Выбранная пользователем рамка листа
        /// </summary>
        public FamilySymbol SelectedTitleBlocks { get; set; }
        /// <summary>
        /// Легенды, имеющиеся в проекте
        /// </summary>
        public List<View> Legends { get; set; } = new List<View>();
        /// <summary>
        /// Выбранная пользователем легенда
        /// </summary>
        public View SelectedLegend { get; set; }
        /// <summary>
        /// Типоразмеры видов, имеющиеся в проекте
        /// </summary>
        public List<ViewFamilyType> ViewFamilyTypes { get; set; } = new List<ViewFamilyType>();
        /// <summary>
        /// Выбранный пользователем типоразмер вида для создания новых видов
        /// </summary>
        public static ViewFamilyType SelectedViewFamilyType { get; set; }
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
        /// Перечень шаблонов видов в проекте
        /// </summary>
        public List<ViewSection> ViewTemplatesInPj { get; set; } = new List<ViewSection>();
        /// <summary>
        /// Выбранный пользователем шаблон вида основных видов
        /// </summary>
        public View SelectedGeneralViewTemplate { get; set; }
        /// <summary>
        /// Выбранный пользователем шаблон вида поперечных видов
        /// </summary>
        //public View SelectedTransverseViewTemplate { get; set; }




        private View _selectedTransverseViewTemplate;
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



        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }




        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {

            SaveConfig();

            CreateSheetsNViews(null);
        }


        private void LoadConfig() {
            
            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            _pluginConfig.GetConfigProps(settings, this);

            FindGeneralViewTemplate();
            FindTransverseViewTemplate();
        }

        private void SaveConfig() {
            
            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            _pluginConfig.SetConfigProps(settings, this);

            _pluginConfig.SaveProjectConfig();
        }







        /// <summary>
        /// Ищет эталонные спецификации по указанным именам. На основе эталонных спек создаются спеки для пилонов путем копирования
        /// </summary>
        private void FindReferenceSchedules() {
            ReferenceRebarSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.REBAR_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceMaterialSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.MATERIAL_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceSystemPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.SYSTEM_PARTS_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceIFCPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SchedulesSettings.IFC_PARTS_SCHEDULE_NAME)) as ViewSchedule;
        }




        // Получаем названия Комплектов документации по пилонам
        private void GetRebarProjectSections() 
        {
            // Пользователь может перезадать параметр раздела, поэтому сначала чистим
            ProjectSections.Clear();
            ErrorText = string.Empty;

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление видов")) {

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
        private void GetHostMarksInGUI(object p) 
        {
            SelectedHostsInfo = new List<PylonSheetInfo>(HostsInfo
                .Where(item => item.ProjectSection.Equals(SelectedProjectSection))
                .ToList());
        }



        private void ApplySettings(object p) {


            ProjectSettings.ApplyProjectSettings();
            ViewSectionSettings.ApplyViewSectionsSettings();
            SchedulesSettings.ApplySchedulesSettings();

            SettingsEdited = false;

            // Получаем заново список заполненных разделов проекта
            GetRebarProjectSections();
        }
        private bool CanApplySettings(object p) {
            if(SettingsEdited) {
                return true;
            }
            return false;
        }


        private void CreateSheetsNViews(object p) {

            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление видов")) {

                foreach(PylonSheetInfo hostsInfo in SelectedHostsInfo) {

                    // Если текущий PylonSheetInfo не выбран для работы - continue
                    if(!hostsInfo.IsCheck) { continue; } else {
                        hostsInfo.GetViewNamesForWork();
                    }

                    // Если листы был в проекте (когда плагин запускают для создания/размещения видов), то мы об этом знаем из RevitRepository
                    if(hostsInfo.PylonViewSheet is null) {

                        hostsInfo.CreateSheet();
                    } else {
                        
                        hostsInfo.FindTitleBlock();
                        hostsInfo.GetTitleBlockSize();
                        hostsInfo.FindViewsNViewportsOnSheet();
                        hostsInfo.FindSchedulesNViewportsOnSheet();
                        hostsInfo.FindNoteLegendOnSheet();
                    }
                    
                    // Если вдруг по какой-то причине лист не был создан, то создание видов/видовых экранов не выполняем 
                    if(hostsInfo.PylonViewSheet is null) { continue; }

                                                    //////////////////
                                                    // ОСНОВНОЙ ВИД //
                                                    //////////////////
                    
                    if(SelectionSettings.NeedWorkWithGeneralView) {
                        
                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                        if(hostsInfo.GeneralView.ViewElement is null) {
                            
                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.GeneralView.ViewSectionCreator.TryCreateGeneralView(SelectedViewFamilyType)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.GeneralView);
                            }
                        }
                        // Тут точно получили вид
                    }


                                            ///////////////////////////////////
                                            // ОСНОВНОЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД //
                                            ///////////////////////////////////

                    if(SelectionSettings.NeedWorkWithGeneralPerpendicularView) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.GeneralViewPerpendicular.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.GeneralViewPerpendicular.ViewSectionCreator.TryCreateGeneralPerpendicularView(SelectedViewFamilyType)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.GeneralViewPerpendicular);
                            }
                        }
                        // Тут точно получили вид
                    }


                                                ///////////////////////////
                                                // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД //
                                                ///////////////////////////

                    if(SelectionSettings.NeedWorkWithTransverseViewFirst) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewFirst.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewFirst.ViewSectionCreator.TryCreateTransverseView(SelectedViewFamilyType, 1)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewFirst);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                ///////////////////////////
                                                // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД //
                                                ///////////////////////////

                    if(SelectionSettings.NeedWorkWithTransverseViewSecond) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewSecond.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewSecond.ViewSectionCreator.TryCreateTransverseView(SelectedViewFamilyType, 2)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewSecond);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                ///////////////////////////
                                                // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД //
                                                ///////////////////////////

                    if(SelectionSettings.NeedWorkWithTransverseViewThird) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewThird.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewThird.ViewSectionCreator.TryCreateTransverseView(SelectedViewFamilyType, 3)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewThird);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                ///////////////////////////
                                                // СПЕЦИФИКАЦИЯ АРМАТУРЫ //
                                                ///////////////////////////

                    if(SelectionSettings.NeedWorkWithRebarSchedule) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.RebarSchedule.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.RebarSchedule.ViewScheduleCreator.TryCreateRebarSchedule()) {
                                _revitRepository.FindViewScheduleInPj(hostsInfo.RebarSchedule);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                /////////////////////////////
                                                // СПЕЦИФИКАЦИЯ МАТЕРИАЛОВ //
                                                /////////////////////////////

                    if(SelectionSettings.NeedWorkWithMaterialSchedule) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.MaterialSchedule.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.MaterialSchedule.ViewScheduleCreator.TryCreateMaterialSchedule()) {
                                _revitRepository.FindViewScheduleInPj(hostsInfo.MaterialSchedule);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                /////////////////////////////////
                                                // ВЕДОМОСТЬ СИСТЕМНЫХ ДЕТАЛЕЙ //
                                                /////////////////////////////////

                    if(SelectionSettings.NeedWorkWithSystemPartsSchedule) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.SystemPartsSchedule.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.SystemPartsSchedule.ViewScheduleCreator.TryCreateSystemPartsSchedule()) {
                                _revitRepository.FindViewScheduleInPj(hostsInfo.SystemPartsSchedule);
                            }
                        }
                        // Тут точно получили вид
                    }

                                                    ///////////////////////////
                                                    // ВЕДОМОСТЬ IFC ДЕТАЛЕЙ //
                                                    ///////////////////////////

                    if(SelectionSettings.NeedWorkWithIFCPartsSchedule) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.IFCPartsSchedule.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.IFCPartsSchedule.ViewScheduleCreator.TryCreateIFCPartsSchedule()) {
                                _revitRepository.FindViewScheduleInPj(hostsInfo.IFCPartsSchedule);
                            }
                        }
                        // Тут точно получили вид
                    }


                    // Принудительно регеним документ, иначе запрашиваемые габариты видовых экранов будут некорректны
                    _revitRepository.Document.Regenerate();


                    if(SelectionSettings.NeedWorkWithGeneralView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralView.ViewportElement is null) {

                            hostsInfo.GeneralView.ViewSectionPlacer.PlaceGeneralViewport();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithGeneralPerpendicularView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralViewPerpendicular.ViewportElement is null) {

                            hostsInfo.GeneralViewPerpendicular.ViewSectionPlacer.PlaceGeneralPerpendicularViewport();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithTransverseViewFirst) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewFirst.ViewportElement is null) {

                            hostsInfo.TransverseViewFirst.ViewSectionPlacer.PlaceTransverseFirstViewPorts();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithTransverseViewSecond) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewSecond.ViewportElement is null) {

                            hostsInfo.TransverseViewSecond.ViewSectionPlacer.PlaceTransverseSecondViewPorts();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithTransverseViewThird) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewThird.ViewportElement is null) {

                            hostsInfo.TransverseViewThird.ViewSectionPlacer.PlaceTransverseThirdViewPorts();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithRebarSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.RebarSchedule.ViewportElement is null) {

                            hostsInfo.RebarSchedule.ViewSchedulePlacer.PlaceRebarSchedule();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithMaterialSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.MaterialSchedule.ViewportElement is null) {

                            hostsInfo.MaterialSchedule.ViewSchedulePlacer.PlaceMaterialSchedule();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithSystemPartsSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.SystemPartsSchedule.ViewportElement is null) {

                            hostsInfo.SystemPartsSchedule.ViewSchedulePlacer.PlaceSystemPartsSchedule();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithIFCPartsSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.IFCPartsSchedule.ViewportElement is null) {

                            hostsInfo.IFCPartsSchedule.ViewSchedulePlacer.PlaceIFCPartsSchedule();
                        }
                    }
                    if(SelectionSettings.NeedWorkWithLegend) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.LegendView.ViewportElement is null) {

                            hostsInfo.LegendView.LegendPlacer.PlaceNoteLegend();
                        }
                    }
                }

                transaction.Commit();
            }
        }



        public void FindGeneralViewTemplate() {
            SelectedGeneralViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.GENERAL_VIEW_TEMPLATE_NAME));
        }
        public void FindTransverseViewTemplate() {
            SelectedTransverseViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(ViewSectionSettings.TRANSVERSE_VIEW_TEMPLATE_NAME));
        }



        public void SomeMagicFunc() {
            Solid union = null;

            Document familyDocument = null;


            // Element selection
            IList<Reference> selectelem = _revitRepository.ActiveUIDocument.Selection.PickObjects(ObjectType.Element);





            List<Element> elems = new List<Element>();

            // I don't know how to convert from reference to element.
            foreach(Reference elem in selectelem) {
                elems.Add(_revitRepository.Document.GetElement(elem));
            }


            // Solid Union
            union = GetTargetSolids(elems);



            // create a new family document using Generic Model.rft template
            string templateFileName = @"C:\ProgramData\Autodesk\RVT 2022\Family Templates\Russian\Метрическая система, типовая модель.rft";

            familyDocument = _revitRepository.UIApplication.Application.NewFamilyDocument(templateFileName);







            // Create Generic Model
            using(Transaction trans = new Transaction(familyDocument, "Transaction")) {
                trans.Start();
                FreeFormElement f = FreeFormElement.Create(familyDocument, union);

                TaskDialog.Show("f", f.Id.ToString());
                trans.Commit();
            }

            familyDocument.LoadFamily(_revitRepository.Document, new Ffffff());
        }

        public static Solid GetTargetSolids(List<Element> elements) {
            List<Solid> solids = new List<Solid>();


            foreach(Element elem in elements) {

                Options options = new Options();
                options.DetailLevel = ViewDetailLevel.Fine;
                GeometryElement geomElem = elem.get_Geometry(options);
                foreach(GeometryObject geomObj in geomElem) {
                    if(geomObj is Solid) {
                        Solid solid = (Solid) geomObj;
                        if(solid.Faces.Size > 0 && solid.Volume > 0.0) {
                            solids.Add(solid);
                        }
                        // Single-level recursive check of instances. If viable solids are more than
                        // one level deep, this example ignores them.
                    } else if(geomObj is GeometryInstance) {
                        GeometryInstance geomInst = (GeometryInstance) geomObj;
                        GeometryElement instGeomElem = geomInst.GetInstanceGeometry();
                        foreach(GeometryObject instGeomObj in instGeomElem) {
                            if(instGeomObj is Solid) {
                                Solid solid = (Solid) instGeomObj;
                                if(solid.Faces.Size > 0 && solid.Volume > 0.0) {
                                    solids.Add(solid);
                                }
                            }
                        }
                    }
                }
            }


            Solid sss = solids[0];

            for(int i = 1; i < solids.Count; i++) {
                try {
                    sss = BooleanOperationsUtils.ExecuteBooleanOperation(sss, solids[i], BooleanOperationsType.Union);
                } catch(Autodesk.Revit.Exceptions.InvalidOperationException) {

                }
            }



            return sss;
        }




        /// <summary>
        /// Добавляет новое имя параметра фильтра спецификаций в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void AddScheduleFilterParam(object p) {

            SchedulesSettings.ParamsForScheduleFilters.Add(new ScheduleFilterParamHelper("Введите название", "Введите название"));
        }

        /// <summary>
        /// Удаляет выбранное имя параметра фильтра спецификаций в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void DeleteScheduleFilterParam(object p) {

            List <ScheduleFilterParamHelper> forDel= new List <ScheduleFilterParamHelper>();

            foreach(ScheduleFilterParamHelper param in SchedulesSettings.ParamsForScheduleFilters) {
                if(param.IsCheck) {
                    forDel.Add(param);
                }
            }

            foreach(ScheduleFilterParamHelper param in forDel) {
                SchedulesSettings.ParamsForScheduleFilters.Remove(param);
            }
        }

        /// <summary>
        /// Определяет можно ли выбранное имя параметра фильтра спецификаций в настройках плагина
        /// True, если выбрана штриховка в списке штриховок в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private bool CanChangeScheduleFilterParam(object p) {
            
            foreach(ScheduleFilterParamHelper param in SchedulesSettings.ParamsForScheduleFilters) {
                if(param.IsCheck) {
                    return true;
                }
            }
            
            return false;
        }
    }


    public class Ffffff : IFamilyLoadOptions {
        public bool OnFamilyFound(bool familyInUse, out bool overwriteParameterValues) {
            throw new NotImplementedException();
        }

        public bool OnSharedFamilyFound(Family sharedFamily, bool familyInUse, out FamilySource source, out bool overwriteParameterValues) {
            throw new NotImplementedException();
        }
    }

    public class BooleanConverter : IValueConverter {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {

            if(value is true) { return false; } else { return true; }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) {
            
            if(value is true) { return false; } else { return true; }
        }
    }
}