using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        private bool _edited = false;
        private string _errorText;


        private string _selectedProjectSection = string.Empty;
        private string _hostMarkForSearch = string.Empty;


        private string _projectSectionTemp = "обр_ФОП_Раздел проекта";
        private string _markTemp = "Марка";
        private string _dispatcherGroupingFirstTemp = "_Группа видов 1";
        private string _dispatcherGroupingSecondTemp = "_Группа видов 1";
        private string _sheetSizeTemp = "А";
        private string _sheetCoefficientTemp = "х";

        private string _sheetPrefixTemp = "Пилон ";
        private string _sheetSuffixTemp = "";

        private string _generalViewPrefixTemp = "";
        private string _generalViewSuffixTemp = "";
        private string _generalViewPerpendicularPrefixTemp = "Пилон ";
        private string _generalViewPerpendicularSuffixTemp = "_Перпендикулярный";
        private string _generalViewTemplateNameTemp = "КЖ0.2_пилоны_орг.ур.-2";
        private string _generalViewXOffsetTemp = "200";
        private string _generalViewYTopOffsetTemp = "2300";
        private string _generalViewYBottomOffsetTemp = "200";

        private string _transverseViewFirstPrefixTemp = "";
        private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
        private string _transverseViewSecondPrefixTemp = "";
        private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
        private string _transverseViewThirdPrefixTemp = "";
        private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";
        private string _transverseViewTemplateNameTemp = "";
        private string _transverseViewXOffsetTemp = "200";
        private string _transverseViewYOffsetTemp = "200";

        private string _rebarSchedulePrefixTemp = "Пилон ";
        private string _rebarScheduleSuffixTemp = "";
        private string _materialSchedulePrefixTemp = "!СМ_Пилон ";
        private string _materialScheduleSuffixTemp = "";
        private string _systemPartsSchedulePrefixTemp = "!ВД_СИС_";
        private string _systemPartsScheduleSuffixTemp = "";
        private string _IFCPartsSchedulePrefixTemp = "!ВД_IFC_";
        private string _IFCPartsScheduleSuffixTemp = "";

        private string _rebarScheduleNameTemp = "!СА_Базовая";
        private string _materialScheduleNameTemp = "!СМ";
        private string _systemPartsScheduleNameTemp = "!ВД_СИС";
        private string _IFCPartsScheduleNameTemp = "!ВД_IFC";

        private string _rebarScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _rebarScheduleDisp2Temp = "!СА_Пилоны";
        private string _materialScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _materialScheduleDisp2Temp = "СМ_Пилоны";
        private string _systemPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _systemPartsScheduleDisp2Temp = "ВД_СИС_Пилоны";
        private string _IFCPartsScheduleDisp1Temp = "обр_ФОП_Раздел проекта";
        private string _IFCPartsScheduleDisp2Temp = "ВД_IFC_Пилоны";
        private string _typicalPylonFilterParameterTemp = "обр_ФОП_Фильтрация 1";
        private string _typicalPylonFilterValueTemp = "на 1 шт.";

        public static string DEF_TITLEBLOCK_NAME = "Создать типы по комплектам";

        private List<PylonSheetInfo> _selectedHostsInfo = new List<PylonSheetInfo>();
        //private ScheduleFilterParamHelper _selectedScheduleFilterParam;




        /// <summary>
        /// Инфо про существующие в проекте листы пилонов
        /// </summary>
        public Dictionary<string, PylonSheetInfo> existingPylonSheetsInfo = new Dictionary<string, PylonSheetInfo>();
        /// <summary>
        /// Инфо про листы пилонов, которые нужно создать
        /// </summary>
        public Dictionary<string, PylonSheetInfo> missingPylonSheetsInfo = new Dictionary<string, PylonSheetInfo>();
        // Вспомогательный список
        public Dictionary<string, List<FamilyInstance>> hostData = new Dictionary<string, List<FamilyInstance>>();

        public ReportHelper reportHelper = new ReportHelper();


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            GetRebarProjectSections();

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;

            TitleBlocks = _revitRepository.TitleBlocksInProject;
            SelectedTitleBlocks = TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name == DEF_TITLEBLOCK_NAME);

            Legends = _revitRepository.LegendsInProject;
            SelectedLegend = Legends
                .FirstOrDefault(view => view.Name.Contains("илон"));


            ViewTemplatesInPj = _revitRepository.AllViewTemplates;
            SelectedGeneralViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(GENERAL_VIEW_TEMPLATE_NAME));
            SelectedTransverseViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(TRANSVERSE_VIEW_TEMPLATE_NAME));

            FindReferenceSchedules();
            
            GetHostMarksInGUICommand = new RelayCommand(GetHostMarksInGUI);


            TestCommand = new RelayCommand(Test);

            CreateSheetsCommand = new RelayCommand(CreateSheets, CanCreateSheets);
            ApplySettingsCommands = new RelayCommand(ApplySettings, CanApplySettings);

            AddScheduleFilterParamCommand = new RelayCommand(AddScheduleFilterParam);
            DeleteScheduleFilterParamCommand = new RelayCommand(DeleteScheduleFilterParam, CanChangeScheduleFilterParam);

        }




        public ICommand ApplySettingsCommands { get; }
        public ICommand CreateSheetsCommand { get; }
        public ICommand GetHostMarksInGUICommand { get; }
        public ICommand TestCommand { get; }




        public ICommand AddScheduleFilterParamCommand { get; }
        public ICommand DeleteScheduleFilterParamCommand { get; }





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
        public static View SelectedLegend { get; set; }
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
        public View SelectedTransverseViewTemplate { get; set; }



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
                _projectSectionTemp = value;
                _edited = true;
            }
        }

        public string MARK { get; set; } = "Марка";
        public string MARK_TEMP {
            get => _markTemp;
            set {
                _markTemp = value;
                _edited = true;
            }
        }

        // dispatcher grouping
        public string DISPATCHER_GROUPING_FIRST { get; set; } = "_Группа видов 1";
        public string DISPATCHER_GROUPING_FIRST_TEMP {
            get => _dispatcherGroupingFirstTemp;
            set {
                _dispatcherGroupingFirstTemp = value;
                _edited = true;
            }
        }
        public string DISPATCHER_GROUPING_SECOND { get; set; } = "_Группа видов 2";
        public string DISPATCHER_GROUPING_SECOND_TEMP {
            get => _dispatcherGroupingSecondTemp;
            set {
                _dispatcherGroupingSecondTemp = value;
                _edited = true;
            }
        }

        public string SHEET_SIZE { get; set; } = "А";
        public string SHEET_SIZE_TEMP {
            get => _sheetSizeTemp;
            set {
                _sheetSizeTemp = value;
                _edited = true;
            }
        }

        public string SHEET_COEFFICIENT { get; set; } = "х";
        public string SHEET_COEFFICIENT_TEMP {
            get => _sheetCoefficientTemp;
            set {
                _sheetCoefficientTemp = value;
                _edited = true;
            }
        }


        public string SHEET_PREFIX { get; set; } = "Пилон ";
        public string SHEET_PREFIX_TEMP {
            get => _sheetPrefixTemp;
            set {
                _sheetPrefixTemp = value;
                _edited = true;
            }
        }

        public string SHEET_SUFFIX { get; set; } = "";
        public string SHEET_SUFFIX_TEMP {
            get => _sheetSuffixTemp;
            set {
                _sheetSuffixTemp = value;
                _edited = true;
            }
        }



        public string GENERAL_VIEW_PREFIX { get; set; } = "";
        public string GENERAL_VIEW_PREFIX_TEMP {
            get => _generalViewPrefixTemp;
            set {
                _generalViewPrefixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_SUFFIX { get; set; } = "";
        public string GENERAL_VIEW_SUFFIX_TEMP {
            get => _generalViewSuffixTemp;
            set {
                _generalViewSuffixTemp = value;
                _edited = true;
            }
        }


        public string GENERAL_VIEW_PERPENDICULAR_PREFIX { get; set; } = "Пилон ";
        public string GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP {
            get => _generalViewPerpendicularPrefixTemp;
            set {
                _generalViewPerpendicularPrefixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX { get; set; } = "_Перпендикулярный";
        public string GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP {
            get => _generalViewPerpendicularSuffixTemp;
            set {
                _generalViewPerpendicularSuffixTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_TEMPLATE_NAME { get; set; } = "КЖ0.2_пилоны_орг.ур.-2";
        public string GENERAL_VIEW_TEMPLATE_NAME_TEMP {
            get => _generalViewTemplateNameTemp;
            set {
                _generalViewTemplateNameTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_X_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_X_OFFSET_TEMP {
            get => _generalViewXOffsetTemp;
            set {
                _generalViewXOffsetTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_Y_TOP_OFFSET { get; set; } = "2300";
        public string GENERAL_VIEW_Y_TOP_OFFSET_TEMP {
            get => _generalViewYTopOffsetTemp;
            set {
                _generalViewYTopOffsetTemp = value;
                _edited = true;
            }
        }

        public string GENERAL_VIEW_Y_BOTTOM_OFFSET { get; set; } = "200";
        public string GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP {
            get => _generalViewYBottomOffsetTemp;
            set {
                _generalViewYBottomOffsetTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_FIRST_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_FIRST_PREFIX_TEMP {
            get => _transverseViewFirstPrefixTemp;
            set {
                _transverseViewFirstPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_FIRST_SUFFIX { get; set; } = "_Сеч.1-1";
        public string TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP {
            get => _transverseViewFirstSuffixTemp;
            set {
                _transverseViewFirstSuffixTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_SECOND_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_SECOND_PREFIX_TEMP {
            get => _transverseViewSecondPrefixTemp;
            set {
                _transverseViewSecondPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_SECOND_SUFFIX { get; set; } = "_Сеч.2-2";
        public string TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP {
            get => _transverseViewSecondSuffixTemp;
            set {
                _transverseViewSecondSuffixTemp = value;
                _edited = true;
            }
        }


        public string TRANSVERSE_VIEW_THIRD_PREFIX { get; set; } = "";
        public string TRANSVERSE_VIEW_THIRD_PREFIX_TEMP {
            get => _transverseViewThirdPrefixTemp;
            set {
                _transverseViewThirdPrefixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_THIRD_SUFFIX { get; set; } = "_Сеч.3-3";
        public string TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP {
            get => _transverseViewThirdSuffixTemp;
            set {
                _transverseViewThirdSuffixTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_TEMPLATE_NAME { get; set; } = "";
        public string TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP {
            get => _transverseViewTemplateNameTemp;
            set {
                _transverseViewTemplateNameTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_X_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_X_OFFSET_TEMP {
            get => _transverseViewXOffsetTemp;
            set {
                _transverseViewXOffsetTemp = value;
                _edited = true;
            }
        }

        public string TRANSVERSE_VIEW_Y_OFFSET { get; set; } = "200";
        public string TRANSVERSE_VIEW_Y_OFFSET_TEMP {
            get => _transverseViewYOffsetTemp;
            set {
                _transverseViewYOffsetTemp = value;
                _edited = true;
            }
        }

        public string REBAR_SCHEDULE_PREFIX { get; set; } = "Пилон ";
        public string REBAR_SCHEDULE_PREFIX_TEMP {
            get => _rebarSchedulePrefixTemp;
            set {
                _rebarSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string REBAR_SCHEDULE_SUFFIX { get; set; } = "";
        public string REBAR_SCHEDULE_SUFFIX_TEMP {
            get => _rebarScheduleSuffixTemp;
            set {
                _rebarScheduleSuffixTemp = value;
                _edited = true;
            }
        }


        public string MATERIAL_SCHEDULE_PREFIX { get; set; } = "!СМ_Пилон ";
        public string MATERIAL_SCHEDULE_PREFIX_TEMP {
            get => _materialSchedulePrefixTemp;
            set {
                _materialSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string MATERIAL_SCHEDULE_SUFFIX { get; set; } = "";
        public string MATERIAL_SCHEDULE_SUFFIX_TEMP {
            get => _materialScheduleSuffixTemp;
            set {
                _materialScheduleSuffixTemp = value;
                _edited = true;
            }
        }

        public string SYSTEM_PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_СИС_";
        public string SYSTEM_PARTS_SCHEDULE_PREFIX_TEMP {
            get => _systemPartsSchedulePrefixTemp;
            set {
                _systemPartsSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string SYSTEM_PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string SYSTEM_PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _systemPartsScheduleSuffixTemp;
            set {
                _systemPartsScheduleSuffixTemp = value;
                _edited = true;
            }
        }


        public string IFC_PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_IFC_";
        public string IFC_PARTS_SCHEDULE_PREFIX_TEMP {
            get => _IFCPartsSchedulePrefixTemp;
            set {
                _IFCPartsSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string IFC_PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string IFC_PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _IFCPartsScheduleSuffixTemp;
            set {
                _IFCPartsScheduleSuffixTemp = value;
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




        public string REBAR_SCHEDULE_NAME { get; set; } = "!СА_Базовая";
        public string REBAR_SCHEDULE_NAME_TEMP {
            get => _rebarScheduleNameTemp;
            set {
                _rebarScheduleNameTemp = value;
                _edited = true;
            }
        }

        public string MATERIAL_SCHEDULE_NAME { get; set; } = "!СМ";
        public string MATERIAL_SCHEDULE_NAME_TEMP {
            get => _materialScheduleNameTemp;
            set {
                _materialScheduleNameTemp = value;
                _edited = true;
            }
        }


        public string SYSTEM_PARTS_SCHEDULE_NAME { get; set; } = "!ВД_СИС";
        public string SYSTEM_PARTS_SCHEDULE_NAME_TEMP {
            get => _systemPartsScheduleNameTemp;
            set {
                _systemPartsScheduleNameTemp = value;
                _edited = true;
            }
        }

        public string IFC_PARTS_SCHEDULE_NAME { get; set; } = "!ВД_IFC";
        public string IFC_PARTS_SCHEDULE_NAME_TEMP {
            get => _IFCPartsScheduleNameTemp;
            set {
                _IFCPartsScheduleNameTemp = value;
                _edited = true;
            }
        }


        public string REBAR_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string REBAR_SCHEDULE_DISP1_TEMP {
            get => _rebarScheduleDisp1Temp;
            set {
                _rebarScheduleDisp1Temp = value;
                _edited = true;
            }
        }
        public string MATERIAL_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string MATERIAL_SCHEDULE_DISP1_TEMP {
            get => _materialScheduleDisp1Temp;
            set {
                _materialScheduleDisp1Temp = value;
                _edited = true;
            }
        }
        public string SYSTEM_PARTS_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string SYSTEM_PARTS_SCHEDULE_DISP1_TEMP {
            get => _systemPartsScheduleDisp1Temp;
            set {
                _systemPartsScheduleDisp1Temp = value;
                _edited = true;
            }
        }
        public string IFC_PARTS_SCHEDULE_DISP1 { get; set; } = "обр_ФОП_Раздел проекта";
        public string IFC_PARTS_SCHEDULE_DISP1_TEMP {
            get => _IFCPartsScheduleDisp1Temp;
            set {
                _IFCPartsScheduleDisp1Temp = value;
                _edited = true;
            }
        }



        public string REBAR_SCHEDULE_DISP2 { get; set; } = "!СА_Пилоны";
        public string REBAR_SCHEDULE_DISP2_TEMP {
            get => _rebarScheduleDisp2Temp;
            set {
                _rebarScheduleDisp2Temp = value;
                _edited = true;
            }
        }
        public string MATERIAL_SCHEDULE_DISP2 { get; set; } = "СМ_Пилоны";
        public string MATERIAL_SCHEDULE_DISP2_TEMP {
            get => _materialScheduleDisp2Temp;
            set {
                _materialScheduleDisp2Temp = value;
                _edited = true;
            }
        }
        public string SYSTEM_PARTS_SCHEDULE_DISP2 { get; set; } = "ВД_СИС_Пилоны";
        public string SYSTEM_PARTS_SCHEDULE_DISP2_TEMP {
            get => _systemPartsScheduleDisp2Temp;
            set {
                _systemPartsScheduleDisp2Temp = value;
                _edited = true;
            }
        }
        public string IFC_PARTS_SCHEDULE_DISP2 { get; set; } = "ВД_IFC_Пилоны";
        public string IFC_PARTS_SCHEDULE_DISP2_TEMP {
            get => _IFCPartsScheduleDisp2Temp;
            set {
                _IFCPartsScheduleDisp2Temp = value;
                _edited = true;
            }
        }

        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; } = "обр_ФОП_Фильтрация 1";
        public string TYPICAL_PYLON_FILTER_PARAMETER_TEMP {
            get => _typicalPylonFilterParameterTemp;
            set {
                _typicalPylonFilterParameterTemp = value;
                _edited = true;
            }
        }


        public string TYPICAL_PYLON_FILTER_VALUE { get; set; } = "на 1 шт.";
        public string TYPICAL_PYLON_FILTER_VALUE_TEMP {
            get => _typicalPylonFilterValueTemp;
            set {
                _typicalPylonFilterValueTemp = value;
                _edited = true;
            }
        }


        private ObservableCollection<ScheduleFilterParamHelper> _paramsForScheduleFilters = new ObservableCollection<ScheduleFilterParamHelper>() {
            new ScheduleFilterParamHelper("обр_ФОП_Форма_номер", ""),
            new ScheduleFilterParamHelper("обр_ФОП_Раздел проекта", "обр_ФОП_Раздел проекта"),
            new ScheduleFilterParamHelper("обр_Метка основы_универсальная", "Марка"),
            new ScheduleFilterParamHelper("обр_ФОП_Орг. уровень", "обр_ФОП_Орг. уровень")
        };

        public ObservableCollection<ScheduleFilterParamHelper> ParamsForScheduleFilters {
            get => _paramsForScheduleFilters;
            set {
                _paramsForScheduleFilters = value;
                _edited = true;
            }
        }
        #endregion


        public string Report {
            get => reportHelper.GetAsString();
            set {
                reportHelper.AppendLine(value);
                this.OnPropertyChanged();
            }
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }





        /// <summary>
        /// Находим эталонные спецификации
        /// </summary>
        private void FindReferenceSchedules() {
            ReferenceRebarSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(REBAR_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceMaterialSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(MATERIAL_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceSystemPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(SYSTEM_PARTS_SCHEDULE_NAME)) as ViewSchedule;
            ReferenceIFCPartsSchedule = _revitRepository.AllScheduleViews.FirstOrDefault(sch => sch.Name.Equals(IFC_PARTS_SCHEDULE_NAME)) as ViewSchedule;
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
                //transaction.Commit();
            }

            

            HostsInfo = new ObservableCollection<PylonSheetInfo>(_revitRepository.HostsInfo);
            ProjectSections = new ObservableCollection<string>(_revitRepository.HostProjectSections);
            OnPropertyChanged(nameof(HostsInfo));
            OnPropertyChanged(nameof(ProjectSections));
        }


        // Метод для авто обновления списка марок пилонов при выборе рабочего набора
        private void GetHostMarksInGUI(object p) 
        {
            //ErrorText= string.Empty;

            SelectedHostsInfo = new List<PylonSheetInfo>(HostsInfo
                .Where(item => item.ProjectSection.Equals(SelectedProjectSection))
                .ToList());
        }




        private void ApplySettings(object p) {
            // Необходимо будет написать метод проверки имен параметров - есть ли такие параметры у нужных категорий
            

            // Устанавливаем флаг, что применили параметры и перезаписываем параметры
            PROJECT_SECTION = _projectSectionTemp;
            MARK = _markTemp;
            DISPATCHER_GROUPING_FIRST = _dispatcherGroupingFirstTemp;
            DISPATCHER_GROUPING_SECOND = _dispatcherGroupingSecondTemp;
            SHEET_SIZE = _sheetSizeTemp;
            SHEET_COEFFICIENT = _sheetCoefficientTemp;
            SHEET_PREFIX = _sheetPrefixTemp;
            SHEET_SUFFIX = _sheetSuffixTemp;
            TYPICAL_PYLON_FILTER_PARAMETER = _typicalPylonFilterParameterTemp;
            TYPICAL_PYLON_FILTER_VALUE = _typicalPylonFilterValueTemp;


            GENERAL_VIEW_PREFIX = _generalViewPrefixTemp;
            GENERAL_VIEW_SUFFIX = _generalViewSuffixTemp;
            GENERAL_VIEW_PERPENDICULAR_PREFIX = _generalViewPerpendicularPrefixTemp;
            GENERAL_VIEW_PERPENDICULAR_SUFFIX = _generalViewPerpendicularSuffixTemp;
            GENERAL_VIEW_TEMPLATE_NAME = _generalViewTemplateNameTemp;
            GENERAL_VIEW_X_OFFSET = _generalViewXOffsetTemp;
            GENERAL_VIEW_Y_TOP_OFFSET = _generalViewYTopOffsetTemp;
            GENERAL_VIEW_Y_BOTTOM_OFFSET = _generalViewYBottomOffsetTemp;


            TRANSVERSE_VIEW_FIRST_PREFIX = _transverseViewFirstPrefixTemp;
            TRANSVERSE_VIEW_FIRST_SUFFIX = _transverseViewFirstSuffixTemp;
            TRANSVERSE_VIEW_SECOND_PREFIX = _transverseViewSecondPrefixTemp;
            TRANSVERSE_VIEW_SECOND_SUFFIX = _transverseViewSecondSuffixTemp;
            TRANSVERSE_VIEW_THIRD_PREFIX = _transverseViewThirdPrefixTemp;
            TRANSVERSE_VIEW_THIRD_SUFFIX = _transverseViewThirdSuffixTemp;
            TRANSVERSE_VIEW_TEMPLATE_NAME = _transverseViewTemplateNameTemp;
            TRANSVERSE_VIEW_X_OFFSET = _transverseViewXOffsetTemp;
            TRANSVERSE_VIEW_Y_OFFSET = _transverseViewYOffsetTemp;


            REBAR_SCHEDULE_PREFIX = _rebarSchedulePrefixTemp;
            REBAR_SCHEDULE_SUFFIX = _rebarScheduleSuffixTemp;
            REBAR_SCHEDULE_NAME = _rebarScheduleNameTemp;
            REBAR_SCHEDULE_DISP1 = _rebarScheduleDisp1Temp;
            REBAR_SCHEDULE_DISP2 = _rebarScheduleDisp2Temp;

            MATERIAL_SCHEDULE_PREFIX = _materialSchedulePrefixTemp;
            MATERIAL_SCHEDULE_SUFFIX = _materialScheduleSuffixTemp;
            MATERIAL_SCHEDULE_NAME = _materialScheduleNameTemp;
            MATERIAL_SCHEDULE_DISP1 = _materialScheduleDisp1Temp;
            MATERIAL_SCHEDULE_DISP2 = _materialScheduleDisp2Temp;

            SYSTEM_PARTS_SCHEDULE_PREFIX = _systemPartsSchedulePrefixTemp;
            SYSTEM_PARTS_SCHEDULE_SUFFIX = _systemPartsScheduleSuffixTemp;
            SYSTEM_PARTS_SCHEDULE_NAME = _systemPartsScheduleNameTemp;
            SYSTEM_PARTS_SCHEDULE_DISP1 = _systemPartsScheduleDisp1Temp;
            SYSTEM_PARTS_SCHEDULE_DISP2 = _systemPartsScheduleDisp2Temp;

            IFC_PARTS_SCHEDULE_PREFIX = _IFCPartsSchedulePrefixTemp;
            IFC_PARTS_SCHEDULE_SUFFIX = _IFCPartsScheduleSuffixTemp;
            IFC_PARTS_SCHEDULE_NAME = _IFCPartsScheduleNameTemp;
            IFC_PARTS_SCHEDULE_DISP1 = _IFCPartsScheduleDisp1Temp;
            IFC_PARTS_SCHEDULE_DISP2 = _IFCPartsScheduleDisp2Temp;


            _edited = false;

            // Получаем заново список заполненных разделов проекта
            GetRebarProjectSections();
        }
        private bool CanApplySettings(object p) {
            if(_edited) {
                return true;
            }
            return false;
        }



        private void CreateSheets(object p) {
            // Забираем список выбранных элементов через CommandParameter
            SelectedHostMarks = p as System.Collections.IList;

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<string> selectedHostMarks = new List<string>();
            foreach(var item in SelectedHostMarks) {
                string hostMark = item as string;
                if(hostMark == null) {
                    continue;
                }
                selectedHostMarks.Add(hostMark);
            }


            string report = string.Empty;
            // Получаем инфо о листах, которые нужно создать
            #region Отчет
            Report = "Приступаем к созданию документации по выбранным пилонам";
            Report = "Анализируем...";
            #endregion
            AnalyzeExistingSheets();



            // Проверка,если ли листы для создания
            if(missingPylonSheetsInfo.Keys.Count > 0) {
                #region Отчет
                Report = "Пользователь выбрал типоразмер рамки листа: " + SelectedTitleBlocks.Name;
                Report = "Пользователь выбрал легенду примечаний: " + SelectedLegend.Name;
                Report = Environment.NewLine + "Приступаю к созданию листов";
                #endregion
            } else {
                #region Отчет
                Report = "Все запрошенные листы уже созданы";
                Report = "Работа завершена";
                #endregion
            }


            Transaction transaction = new Transaction(_revitRepository.Document, "Создание листов пилонов");
            transaction.Start();


            foreach(string sheetKeyName in missingPylonSheetsInfo.Keys) {
                // Лист 
                #region Отчет
                Report = Environment.NewLine + 
                    "---------------------------------------------------------------------------" +
                    "---------------------------------------------------------------------------";
                Report = " - Пилон " + sheetKeyName;
                Report = "\tЛист создан";
                #endregion

                ViewSheet viewSheet = ViewSheet.Create(_revitRepository.Document, SelectedTitleBlocks.Id);
                viewSheet.Name = "Пилон " + sheetKeyName;


                Autodesk.Revit.DB.Parameter viewSheetGroupingParameter = viewSheet.LookupParameter(DISPATCHER_GROUPING_FIRST);
                if(viewSheetGroupingParameter == null) {
                    #region Отчет
                    Report = "\tПараметр \"" + DISPATCHER_GROUPING_FIRST + "\" не заполнен, т.к. не был найден у листа";
                    #endregion
                } else {
                    viewSheetGroupingParameter.Set(SelectedProjectSection);
                    #region Отчет
                    Report = "\tГруппировка по параметру \"" + DISPATCHER_GROUPING_FIRST + "\": " + SelectedProjectSection;
                    #endregion
                }


                // Фиксируем инфо про легенду
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewElement = SelectedLegend;
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewportTypeName = "Без названия";
                missingPylonSheetsInfo[sheetKeyName].LegendView.ViewportCenter = new XYZ(-0.34, 0.30, 0);

                missingPylonSheetsInfo[sheetKeyName].PylonViewSheet = viewSheet;

                // Рамка листа
                FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, viewSheet.Id)
                    .OfCategory(BuiltInCategory.OST_TitleBlocks)
                    .WhereElementIsNotElementType()
                    .FirstElement() as FamilyInstance;

                missingPylonSheetsInfo[sheetKeyName].TitleBlock = titleBlock;

                // Пытаемся задать габарит листа А3 и получаем габариты рамки
                missingPylonSheetsInfo[sheetKeyName].SetTitleBlockSize(_revitRepository.Document);


                // Размещение видовых экранов
                //missingPylonSheetsInfo[sheetKeyName].PlaceGeneralViewport();
                //missingPylonSheetsInfo[sheetKeyName].PlaceTransverseViewPorts();

                // Размещение спецификаций
                missingPylonSheetsInfo[sheetKeyName].PlaceRebarSchedule();
                missingPylonSheetsInfo[sheetKeyName].PlaceMaterialSchedule();
                missingPylonSheetsInfo[sheetKeyName].PlacePartsSchedule();


                // Размещение легенды
                missingPylonSheetsInfo[sheetKeyName].PlaceLegend(_revitRepository);
            }

            transaction.Commit();
        }
        private bool CanCreateSheets(object p) {
            if(ErrorText.Length > 0) {
                return false;
            }
            // Проверяем правила поиска видов на уникальность
            string genView = GENERAL_VIEW_PREFIX + GENERAL_VIEW_SUFFIX;
            string tranViewFirst = TRANSVERSE_VIEW_FIRST_PREFIX + TRANSVERSE_VIEW_FIRST_SUFFIX;
            string tranViewSecond = TRANSVERSE_VIEW_SECOND_PREFIX + TRANSVERSE_VIEW_SECOND_SUFFIX;
            string tranViewThird = TRANSVERSE_VIEW_THIRD_PREFIX + TRANSVERSE_VIEW_THIRD_SUFFIX;

            if(genView == tranViewFirst || genView == tranViewSecond || genView == tranViewThird || tranViewFirst == tranViewSecond
                || tranViewFirst == tranViewThird || tranViewSecond == tranViewThird) {
                ErrorText = "Правила поиска видов некорректны. Задайте уникальные правила в настройках";
                return false;
            }

            // Проверяем правила поиска спек на уникальность
            string rebSchedule = REBAR_SCHEDULE_PREFIX + REBAR_SCHEDULE_SUFFIX;
            string matSchedule = MATERIAL_SCHEDULE_PREFIX + MATERIAL_SCHEDULE_SUFFIX;
            string partsSchedule = SYSTEM_PARTS_SCHEDULE_PREFIX + SYSTEM_PARTS_SCHEDULE_SUFFIX;

            if(rebSchedule == matSchedule || rebSchedule == partsSchedule || matSchedule == partsSchedule) {
                ErrorText = "Правила поиска спецификаций некорректны. Задайте уникальные правила в настройках";
                return false;
            }

            return true;
        }


        public void AnalyzeExistingSheets() {
            var allExistingSheets = new FilteredElementCollector(_revitRepository.Document)
                .OfClass(typeof(ViewSheet))
                .WhereElementIsNotElementType()
                .ToElements();

            Report = "В проекте найдено листов: " + allExistingSheets.Count.ToString();

            // Перевод списка выбранных марок пилонов в формат листа строк
            List<string> selectedHostMarks = new List<string>();
            foreach(var item in SelectedHostMarks) {
                string hostMark = item as string;
                if(hostMark == null) {
                    continue;
                }

                selectedHostMarks.Add(hostMark);
            }

            
            // Формируем список листов, выбранных для обработки, но уже существущих - existingPylonSheetsInfo
            foreach(var item in allExistingSheets) {
                ViewSheet sheet = item as ViewSheet;
                string sheetKeyName;
                
                if(sheet == null || !sheet.Name.Contains("Пилон") || !sheet.Name.Contains(" ") || sheet.Name.Split(' ').Length < 2) {
                    continue;
                } else {
                    sheetKeyName = sheet.Name.Split(' ')[1];
                }

                if(selectedHostMarks.Contains(sheetKeyName)) {
                    existingPylonSheetsInfo.Add(sheetKeyName, new PylonSheetInfo(this, _revitRepository, sheetKeyName) {
                        PylonViewSheet = sheet,
                    });
                    selectedHostMarks.Remove(sheetKeyName);     // Удаляем имена листов, которые уже есть
                }
            }
            Report = "Из них выбрано для обработки среди уже существующих: " + existingPylonSheetsInfo.Count.ToString();
            if(existingPylonSheetsInfo.Count > 0) {
                Report = "Среди которых: ";
                foreach(string sheetName in existingPylonSheetsInfo.Keys) {
                    Report = " - " + sheetName;
                }
            }


            // Формируем список листов, выбранных для обработки и еще не созданных - wbGeneratingPylonSheetsInfo
            if(selectedHostMarks.Count > 0) {
                foreach(string hostMark in selectedHostMarks) {
                    missingPylonSheetsInfo.Add(hostMark, new PylonSheetInfo(this, _revitRepository, hostMark));
                }
            }
            Report = "Будут созданы листы в количестве: " + missingPylonSheetsInfo.Count.ToString();
            if(missingPylonSheetsInfo.Count > 0) {
                Report = "Среди которых: ";
                foreach(string sheetName in missingPylonSheetsInfo.Keys) {
                    Report = " - " + sheetName;
                }
            }
        }






        private void Test(object p) {


            using(Transaction transaction = _revitRepository.Document.StartTransaction("Добавление видов")) {

                foreach(PylonSheetInfo hostsInfo in SelectedHostsInfo) {

                    // Если текущий PylonSheetInfo не выбран для работы - continue
                    if(!hostsInfo.IsCheck) { continue; } else {
                        hostsInfo.WriteViewNames();
                    }

                    // Если листы был в проекте (когда плагин запускают для создания/размещения видов), то мы об этом знаем из RevitRepository
                    if(hostsInfo.PylonViewSheet is null) {

                        hostsInfo.CreateSheet();
                    } else {
                        
                        hostsInfo.FindTitleBlock();
                        hostsInfo.GetTitleBlockSize();
                        hostsInfo.FindViewsNViewportsOnSheet();
                    }
                    
                    // Если вдруг по какой-то причине лист не был создан, то создание видов/видовых экранов не выполняем 
                    if(hostsInfo.PylonViewSheet is null) { continue; }

                                                    //////////////////
                                                    // ОСНОВНОЙ ВИД //
                                                    //////////////////
                    
                    if(NeedWorkWithGeneralView) {
                        
                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего
                        if(hostsInfo.GeneralView.ViewElement is null) {
                            
                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.GeneralView.ViewCreator.TryCreateGeneralView(SelectedViewFamilyType)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.GeneralView);
                            }
                        }
                        // Тут точно получили вид
                    }


                                            ///////////////////////////////////
                                            // ОСНОВНОЙ ПЕРПЕНДИКУЛЯРНЫЙ ВИД //
                                            ///////////////////////////////////

                    if(NeedWorkWithGeneralPerpendicularView) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.GeneralViewPerpendicular.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.GeneralViewPerpendicular.ViewCreator.TryCreateGeneralPerpendicularView(SelectedViewFamilyType)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.GeneralViewPerpendicular);
                            }
                        }
                        // Тут точно получили вид
                    }


                                            ///////////////////////////
                                            // ПЕРВЫЙ ПОПЕРЕЧНЫЙ ВИД //
                                            ///////////////////////////

                    if(NeedWorkWithTransverseViewFirst) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewFirst.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewFirst.ViewCreator.TryCreateTransverseView(SelectedViewFamilyType, 1)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewFirst);
                            }
                        }
                        // Тут точно получили вид
                    }

                                            ///////////////////////////
                                            // ВТОРОЙ ПОПЕРЕЧНЫЙ ВИД //
                                            ///////////////////////////

                    if(NeedWorkWithTransverseViewSecond) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewSecond.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewSecond.ViewCreator.TryCreateTransverseView(SelectedViewFamilyType, 2)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewSecond);
                            }
                        }
                        // Тут точно получили вид
                    }

                                            ///////////////////////////
                                            // ТРЕТИЙ ПОПЕРЕЧНЫЙ ВИД //
                                            ///////////////////////////

                    if(NeedWorkWithTransverseViewThird) {

                        // Здесь может быть два варианта: 1) найден и вид, и видовой экран; 2) не найдено ничего

                        if(hostsInfo.TransverseViewThird.ViewElement is null) {

                            // Если вид не найден, то сначала пытаемся создать вид, а потом, если создание не успешно - будем искать в проекте
                            if(!hostsInfo.TransverseViewThird.ViewCreator.TryCreateTransverseView(SelectedViewFamilyType, 3)) {
                                _revitRepository.FindViewSectionInPj(hostsInfo.TransverseViewThird);
                            }
                        }
                        // Тут точно получили вид
                    }

                    // Принудительно регеним документ, иначе запрашиваемые габариты видовых экранов будут некорректны
                    _revitRepository.Document.Regenerate();


                    if(NeedWorkWithGeneralView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralView.ViewportElement is null) {

                            hostsInfo.GeneralView.ViewPlacer.PlaceGeneralViewport();
                        }
                    }
                    if(NeedWorkWithGeneralPerpendicularView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralViewPerpendicular.ViewportElement is null) {

                            hostsInfo.GeneralViewPerpendicular.ViewPlacer.PlaceGeneralPerpendicularViewport();
                        }
                    }
                    if(NeedWorkWithTransverseViewFirst) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewFirst.ViewportElement is null) {

                            hostsInfo.TransverseViewFirst.ViewPlacer.PlaceTransverseFirstViewPorts();
                        }
                    }
                    if(NeedWorkWithTransverseViewSecond) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewSecond.ViewportElement is null) {

                            hostsInfo.TransverseViewSecond.ViewPlacer.PlaceTransverseSecondViewPorts();
                        }
                    }
                    if(NeedWorkWithTransverseViewThird) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewThird.ViewportElement is null) {

                            hostsInfo.TransverseViewThird.ViewPlacer.PlaceTransverseThirdViewPorts();
                        }
                    }





                    //if(hostsInfo.PylonViewSheet != null && NeedWorkWithGeneralView) {


                    //    hostsInfo.GeneralView.ViewCreator.CreateGeneralView(SelectedViewFamilyType);
                    //}
                    //if(hostsInfo.PylonViewSheet != null && hostsInfo.GeneralView.ViewElement != null && NeedWorkWithGeneralView) {

                    //    hostsInfo.GeneralView.ViewPlacer.PlaceGeneralViewport();
                    //}








                    //if(hostsInfo.GeneralView.InProjectEditableInGUI && hostsInfo.GeneralView.InProject) {

                    //    hostsInfo.GeneralView.ViewCreator.CreateGeneralView(SelectedViewFamilyType);
                    //}

                    //if(hostsInfo.GeneralViewPerpendicular.InProjectEditableInGUI && hostsInfo.GeneralViewPerpendicular.InProject) {

                    //    hostsInfo.GeneralViewPerpendicular.ViewCreator.CreateGeneralPerpendicularView(SelectedViewFamilyType);
                    //}





                    //if(hostsInfo.TransverseViewFirst.InProjectEditableInGUI && hostsInfo.TransverseViewFirst.InProject) {

                    //    hostsInfo.TransverseViewFirst.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 1);
                    //}

                    //if(hostsInfo.TransverseViewSecond.InProjectEditableInGUI && hostsInfo.TransverseViewSecond.InProject) {

                    //    hostsInfo.TransverseViewSecond.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 2);
                    //}

                    //if(hostsInfo.TransverseViewThird.InProjectEditableInGUI && hostsInfo.TransverseViewThird.InProject) {

                    //    hostsInfo.TransverseViewThird.ViewCreator.CreateTransverseView(SelectedViewFamilyType, 3);
                    //}

                    //if(hostsInfo.RebarSchedule.InProjectEditableInGUI && hostsInfo.RebarSchedule.InProject) {

                    //    hostsInfo.RebarSchedule.ViewCreator.CreateRebarSchedule();
                    //}

                    //if(hostsInfo.MaterialSchedule.InProjectEditableInGUI && hostsInfo.MaterialSchedule.InProject) {

                    //    hostsInfo.MaterialSchedule.ViewCreator.CreateMaterialSchedule();
                    //}

                    //if(hostsInfo.SystemPartsSchedule.InProjectEditableInGUI && hostsInfo.SystemPartsSchedule.InProject) {

                    //    hostsInfo.SystemPartsSchedule.ViewCreator.CreateSystemPartsSchedule();
                    //}

                    //if(hostsInfo.IFCPartsSchedule.InProjectEditableInGUI && hostsInfo.IFCPartsSchedule.InProject) {

                    //    hostsInfo.IFCPartsSchedule.ViewCreator.CreateIFCPartsSchedule();
                    //}


                    //_revitRepository.Document.Regenerate();


                    //if(hostsInfo.GeneralView.OnSheetEditableInGUI && hostsInfo.GeneralView.OnSheet) {

                    //    hostsInfo.GeneralView.ViewPlacer.PlaceGeneralViewport();
                    //}

                    //if(hostsInfo.GeneralViewPerpendicular.OnSheetEditableInGUI && hostsInfo.GeneralViewPerpendicular.OnSheet) {

                    //    hostsInfo.GeneralViewPerpendicular.ViewPlacer.PlaceGeneralPerpendicularViewport();
                    //}

                    //if(hostsInfo.TransverseViewFirst.OnSheetEditableInGUI && hostsInfo.TransverseViewFirst.OnSheet) {

                    //    hostsInfo.TransverseViewFirst.ViewPlacer.PlaceTransverseFirstViewPorts();
                    //}

                    //if(hostsInfo.TransverseViewSecond.OnSheetEditableInGUI && hostsInfo.TransverseViewSecond.OnSheet) {

                    //    hostsInfo.TransverseViewSecond.ViewPlacer.PlaceTransverseSecondViewPorts();
                    //}

                    //if(hostsInfo.TransverseViewThird.OnSheetEditableInGUI && hostsInfo.TransverseViewThird.OnSheet) {

                    //    hostsInfo.TransverseViewThird.ViewPlacer.PlaceTransverseThirdViewPorts();
                    //}

                    //if(hostsInfo.RebarSchedule.OnSheetEditableInGUI && hostsInfo.RebarSchedule.OnSheet) {

                    //    hostsInfo.RebarSchedule.ViewPlacer.PlaceRebarSchedule();
                    //}

                    //if(hostsInfo.MaterialSchedule.OnSheetEditableInGUI && hostsInfo.MaterialSchedule.OnSheet) {

                    //    hostsInfo.MaterialSchedule.ViewPlacer.PlaceMaterialSchedule();
                    //}

                    //if(hostsInfo.SystemPartsSchedule.OnSheetEditableInGUI && hostsInfo.SystemPartsSchedule.OnSheet) {

                    //    hostsInfo.SystemPartsSchedule.ViewPlacer.PlaceSystemPartsSchedule();
                    //}

                    //if(hostsInfo.IFCPartsSchedule.OnSheetEditableInGUI && hostsInfo.IFCPartsSchedule.OnSheet) {

                    //    hostsInfo.IFCPartsSchedule.ViewPlacer.PlaceIFCPartsSchedule();
                    //}
                }


                transaction.Commit();
            }



            //SomeMagicFunc();
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

            ParamsForScheduleFilters.Add(new ScheduleFilterParamHelper("Введите название", "Введите название"));
        }

        /// <summary>
        /// Удаляет выбранное имя параметра фильтра спецификаций в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private void DeleteScheduleFilterParam(object p) {

            List <ScheduleFilterParamHelper> forDel= new List <ScheduleFilterParamHelper>();

            foreach(ScheduleFilterParamHelper param in ParamsForScheduleFilters) {
                if(param.IsCheck) {
                    forDel.Add(param);
                }
            }

            foreach(ScheduleFilterParamHelper param in forDel) {
                ParamsForScheduleFilters.Remove(param);
            }
        }

        /// <summary>
        /// Определяет можно ли выбранное имя параметра фильтра спецификаций в настройках плагина
        /// True, если выбрана штриховка в списке штриховок в настройках плагина
        /// </summary>
        /// <param name="p"></param>
        private bool CanChangeScheduleFilterParam(object p) {
            
            foreach(ScheduleFilterParamHelper param in ParamsForScheduleFilters) {
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