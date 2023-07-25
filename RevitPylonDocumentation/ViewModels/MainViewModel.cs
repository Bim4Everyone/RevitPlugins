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

        private bool _needWorkWithGeneralView = false;
        private bool _needWorkWithGeneralPerpendicularView = false;
        private bool _needWorkWithTransverseViewFirst = false;
        private bool _needWorkWithTransverseViewSecond = false;
        private bool _needWorkWithTransverseViewThird = false;
        private bool _needWorkWithRebarSchedule = false;
        private bool _needWorkWithMaterialSchedule = false;
        private bool _needWorkWithSystemPartsSchedule = false;
        private bool _needWorkWithIFCPartsSchedule = false;
        private bool _needWorkWithLegend = false;

        private List<PylonSheetInfo> _selectedHostsInfo = new List<PylonSheetInfo>();


        public UserSelectionSettingsHelper UserSelectionSettings { get; set; }

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            UserSelectionSettings = new UserSelectionSettingsHelper();

            GetRebarProjectSections();

            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;

            TitleBlocks = _revitRepository.TitleBlocksInProject;
            SelectedTitleBlocks = TitleBlocks
                .FirstOrDefault(titleBlock => titleBlock.Name == DEF_TITLEBLOCK_NAME);

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
            TestCommand = new RelayCommand(Test);

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
        public string MATERIAL_SCHEDULE_DISP2 { get; set; } = "СМ_Пилоны";
        public string MATERIAL_SCHEDULE_DISP2_TEMP {
            get => _materialScheduleDisp2Temp;
            set {
                this.RaiseAndSetIfChanged(ref _materialScheduleDisp2Temp, value);
                _edited = true;
            }
        }
        public string SYSTEM_PARTS_SCHEDULE_DISP2 { get; set; } = "ВД_СИС_Пилоны";
        public string SYSTEM_PARTS_SCHEDULE_DISP2_TEMP {
            get => _systemPartsScheduleDisp2Temp;
            set {
                this.RaiseAndSetIfChanged(ref _systemPartsScheduleDisp2Temp, value);
                _edited = true;
            }
        }
        public string IFC_PARTS_SCHEDULE_DISP2 { get; set; } = "ВД_IFC_Пилоны";
        public string IFC_PARTS_SCHEDULE_DISP2_TEMP {
            get => _IFCPartsScheduleDisp2Temp;
            set {
                this.RaiseAndSetIfChanged(ref _IFCPartsScheduleDisp2Temp, value);
                _edited = true;
            }
        }

        public string TYPICAL_PYLON_FILTER_PARAMETER { get; set; } = "обр_ФОП_Фильтрация 1";
        public string TYPICAL_PYLON_FILTER_PARAMETER_TEMP {
            get => _typicalPylonFilterParameterTemp;
            set {
                this.RaiseAndSetIfChanged(ref _typicalPylonFilterParameterTemp, value);
                _edited = true;
            }
        }


        public string TYPICAL_PYLON_FILTER_VALUE { get; set; } = "на 1 шт.";
        public string TYPICAL_PYLON_FILTER_VALUE_TEMP {
            get => _typicalPylonFilterValueTemp;
            set {
                this.RaiseAndSetIfChanged(ref _typicalPylonFilterValueTemp, value);
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
                this.RaiseAndSetIfChanged(ref _paramsForScheduleFilters, value);
                _edited = true;
            }
        }
        #endregion

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }




        private void LoadView() {
            LoadConfig();
        }

        private void AcceptView() {

            SaveConfig();

            TaskDialog.Show("TEMPLATE_NAME", TRANSVERSE_VIEW_TEMPLATE_NAME);
            TaskDialog.Show("_TEMP", TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP);

            Test(null);
        }


        private void LoadConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document);

            UserSelectionSettings.NeedWorkWithGeneralView = setting.NeedWorkWithGeneralView;
            NeedWorkWithGeneralPerpendicularView = setting.NeedWorkWithGeneralPerpendicularView;
            NeedWorkWithTransverseViewFirst = setting.NeedWorkWithTransverseViewFirst;
            NeedWorkWithTransverseViewSecond = setting.NeedWorkWithTransverseViewSecond;
            NeedWorkWithTransverseViewThird = setting.NeedWorkWithTransverseViewThird;
            NeedWorkWithRebarSchedule = setting.NeedWorkWithRebarSchedule;
            NeedWorkWithMaterialSchedule = setting.NeedWorkWithMaterialSchedule;
            NeedWorkWithSystemPartsSchedule = setting.NeedWorkWithSystemPartsSchedule;
            NeedWorkWithIFCPartsSchedule = setting.NeedWorkWithIFCPartsSchedule;
            NeedWorkWithLegend = setting.NeedWorkWithLegend;

            PROJECT_SECTION = setting.PROJECT_SECTION;
            PROJECT_SECTION_TEMP = setting.PROJECT_SECTION;
            MARK = setting.MARK;
            MARK_TEMP = setting.MARK;
            DISPATCHER_GROUPING_FIRST = setting.DISPATCHER_GROUPING_FIRST;
            DISPATCHER_GROUPING_FIRST_TEMP = setting.DISPATCHER_GROUPING_FIRST;
            DISPATCHER_GROUPING_SECOND = setting.DISPATCHER_GROUPING_SECOND;
            DISPATCHER_GROUPING_SECOND_TEMP = setting.DISPATCHER_GROUPING_SECOND;

            SHEET_SIZE = setting.SHEET_SIZE;
            SHEET_SIZE_TEMP = setting.SHEET_SIZE;
            SHEET_COEFFICIENT = setting.SHEET_COEFFICIENT;
            SHEET_COEFFICIENT_TEMP = setting.SHEET_COEFFICIENT;
            SHEET_PREFIX = setting.SHEET_PREFIX;
            SHEET_PREFIX_TEMP = setting.SHEET_PREFIX;
            SHEET_SUFFIX = setting.SHEET_SUFFIX;
            SHEET_SUFFIX_TEMP = setting.SHEET_SUFFIX;

            GENERAL_VIEW_PREFIX = setting.GENERAL_VIEW_PREFIX;
            GENERAL_VIEW_PREFIX_TEMP = setting.GENERAL_VIEW_PREFIX;
            GENERAL_VIEW_SUFFIX = setting.GENERAL_VIEW_SUFFIX;
            GENERAL_VIEW_SUFFIX_TEMP = setting.GENERAL_VIEW_SUFFIX;
            GENERAL_VIEW_PERPENDICULAR_PREFIX = setting.GENERAL_VIEW_PERPENDICULAR_PREFIX;
            GENERAL_VIEW_PERPENDICULAR_PREFIX_TEMP = setting.GENERAL_VIEW_PERPENDICULAR_PREFIX;
            GENERAL_VIEW_PERPENDICULAR_SUFFIX = setting.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            GENERAL_VIEW_PERPENDICULAR_SUFFIX_TEMP = setting.GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            GENERAL_VIEW_TEMPLATE_NAME = setting.GENERAL_VIEW_TEMPLATE_NAME;
            GENERAL_VIEW_TEMPLATE_NAME_TEMP = setting.GENERAL_VIEW_TEMPLATE_NAME;
            FindGeneralViewTemplate();

            GENERAL_VIEW_X_OFFSET = setting.GENERAL_VIEW_X_OFFSET;
            GENERAL_VIEW_X_OFFSET_TEMP = setting.GENERAL_VIEW_X_OFFSET;
            GENERAL_VIEW_Y_TOP_OFFSET = setting.GENERAL_VIEW_Y_TOP_OFFSET;
            GENERAL_VIEW_Y_TOP_OFFSET_TEMP = setting.GENERAL_VIEW_Y_TOP_OFFSET;
            GENERAL_VIEW_Y_BOTTOM_OFFSET = setting.GENERAL_VIEW_Y_BOTTOM_OFFSET;
            GENERAL_VIEW_Y_BOTTOM_OFFSET_TEMP = setting.GENERAL_VIEW_Y_BOTTOM_OFFSET;

            TRANSVERSE_VIEW_FIRST_PREFIX = setting.TRANSVERSE_VIEW_FIRST_PREFIX;
            TRANSVERSE_VIEW_FIRST_PREFIX_TEMP = setting.TRANSVERSE_VIEW_FIRST_PREFIX;
            TRANSVERSE_VIEW_FIRST_SUFFIX = setting.TRANSVERSE_VIEW_FIRST_SUFFIX;
            TRANSVERSE_VIEW_FIRST_SUFFIX_TEMP = setting.TRANSVERSE_VIEW_FIRST_SUFFIX;
            TRANSVERSE_VIEW_SECOND_PREFIX = setting.TRANSVERSE_VIEW_SECOND_PREFIX;
            TRANSVERSE_VIEW_SECOND_PREFIX_TEMP = setting.TRANSVERSE_VIEW_SECOND_PREFIX;
            TRANSVERSE_VIEW_SECOND_SUFFIX = setting.TRANSVERSE_VIEW_SECOND_SUFFIX;
            TRANSVERSE_VIEW_SECOND_SUFFIX_TEMP = setting.TRANSVERSE_VIEW_SECOND_SUFFIX;
            TRANSVERSE_VIEW_THIRD_PREFIX = setting.TRANSVERSE_VIEW_THIRD_PREFIX;
            TRANSVERSE_VIEW_THIRD_PREFIX_TEMP = setting.TRANSVERSE_VIEW_THIRD_PREFIX;
            TRANSVERSE_VIEW_THIRD_SUFFIX = setting.TRANSVERSE_VIEW_THIRD_SUFFIX;
            TRANSVERSE_VIEW_THIRD_SUFFIX_TEMP = setting.TRANSVERSE_VIEW_THIRD_SUFFIX;
            TRANSVERSE_VIEW_TEMPLATE_NAME = setting.TRANSVERSE_VIEW_TEMPLATE_NAME;
            TRANSVERSE_VIEW_TEMPLATE_NAME_TEMP = setting.TRANSVERSE_VIEW_TEMPLATE_NAME;
            FindTransverseViewTemplate();

            TRANSVERSE_VIEW_X_OFFSET = setting.TRANSVERSE_VIEW_X_OFFSET;
            TRANSVERSE_VIEW_X_OFFSET_TEMP = setting.TRANSVERSE_VIEW_X_OFFSET;
            TRANSVERSE_VIEW_Y_OFFSET = setting.TRANSVERSE_VIEW_Y_OFFSET;
            TRANSVERSE_VIEW_Y_OFFSET_TEMP = setting.TRANSVERSE_VIEW_Y_OFFSET;


            REBAR_SCHEDULE_PREFIX = setting.REBAR_SCHEDULE_PREFIX;
            REBAR_SCHEDULE_PREFIX_TEMP = setting.REBAR_SCHEDULE_PREFIX;
            REBAR_SCHEDULE_SUFFIX = setting.REBAR_SCHEDULE_SUFFIX;
            REBAR_SCHEDULE_SUFFIX_TEMP = setting.REBAR_SCHEDULE_SUFFIX;

            MATERIAL_SCHEDULE_PREFIX = setting.MATERIAL_SCHEDULE_PREFIX;
            MATERIAL_SCHEDULE_PREFIX_TEMP = setting.MATERIAL_SCHEDULE_PREFIX;
            MATERIAL_SCHEDULE_SUFFIX = setting.MATERIAL_SCHEDULE_SUFFIX;
            MATERIAL_SCHEDULE_SUFFIX_TEMP = setting.MATERIAL_SCHEDULE_SUFFIX;

            SYSTEM_PARTS_SCHEDULE_PREFIX = setting.SYSTEM_PARTS_SCHEDULE_PREFIX;
            SYSTEM_PARTS_SCHEDULE_PREFIX_TEMP = setting.SYSTEM_PARTS_SCHEDULE_PREFIX;
            SYSTEM_PARTS_SCHEDULE_SUFFIX = setting.SYSTEM_PARTS_SCHEDULE_SUFFIX;
            SYSTEM_PARTS_SCHEDULE_SUFFIX_TEMP = setting.SYSTEM_PARTS_SCHEDULE_SUFFIX;

            IFC_PARTS_SCHEDULE_PREFIX = setting.IFC_PARTS_SCHEDULE_PREFIX;
            IFC_PARTS_SCHEDULE_PREFIX_TEMP = setting.IFC_PARTS_SCHEDULE_PREFIX;
            IFC_PARTS_SCHEDULE_SUFFIX = setting.IFC_PARTS_SCHEDULE_SUFFIX;
            IFC_PARTS_SCHEDULE_SUFFIX_TEMP = setting.IFC_PARTS_SCHEDULE_SUFFIX;

            REBAR_SCHEDULE_NAME = setting.REBAR_SCHEDULE_NAME;
            REBAR_SCHEDULE_NAME_TEMP = setting.REBAR_SCHEDULE_NAME;
            MATERIAL_SCHEDULE_NAME = setting.MATERIAL_SCHEDULE_NAME;
            MATERIAL_SCHEDULE_NAME_TEMP = setting.MATERIAL_SCHEDULE_NAME;
            SYSTEM_PARTS_SCHEDULE_NAME = setting.SYSTEM_PARTS_SCHEDULE_NAME;
            SYSTEM_PARTS_SCHEDULE_NAME_TEMP = setting.SYSTEM_PARTS_SCHEDULE_NAME;
            IFC_PARTS_SCHEDULE_NAME = setting.IFC_PARTS_SCHEDULE_NAME;
            IFC_PARTS_SCHEDULE_NAME_TEMP = setting.IFC_PARTS_SCHEDULE_NAME;

            REBAR_SCHEDULE_DISP1 = setting.REBAR_SCHEDULE_DISP1;
            REBAR_SCHEDULE_DISP1_TEMP = setting.REBAR_SCHEDULE_DISP1;
            MATERIAL_SCHEDULE_DISP1 = setting.MATERIAL_SCHEDULE_DISP1;
            MATERIAL_SCHEDULE_DISP1_TEMP = setting.MATERIAL_SCHEDULE_DISP1;
            SYSTEM_PARTS_SCHEDULE_DISP1 = setting.SYSTEM_PARTS_SCHEDULE_DISP1;
            SYSTEM_PARTS_SCHEDULE_DISP1_TEMP = setting.SYSTEM_PARTS_SCHEDULE_DISP1;
            IFC_PARTS_SCHEDULE_DISP1 = setting.IFC_PARTS_SCHEDULE_DISP1;
            IFC_PARTS_SCHEDULE_DISP1_TEMP = setting.IFC_PARTS_SCHEDULE_DISP1;
            REBAR_SCHEDULE_DISP2 = setting.REBAR_SCHEDULE_DISP2;
            REBAR_SCHEDULE_DISP2_TEMP = setting.REBAR_SCHEDULE_DISP2;
            MATERIAL_SCHEDULE_DISP2 = setting.MATERIAL_SCHEDULE_DISP2;
            MATERIAL_SCHEDULE_DISP2_TEMP = setting.MATERIAL_SCHEDULE_DISP2;
            SYSTEM_PARTS_SCHEDULE_DISP2 = setting.SYSTEM_PARTS_SCHEDULE_DISP2;
            SYSTEM_PARTS_SCHEDULE_DISP2_TEMP = setting.SYSTEM_PARTS_SCHEDULE_DISP2;
            IFC_PARTS_SCHEDULE_DISP2 = setting.IFC_PARTS_SCHEDULE_DISP2;
            IFC_PARTS_SCHEDULE_DISP2_TEMP = setting.IFC_PARTS_SCHEDULE_DISP2;

            TYPICAL_PYLON_FILTER_PARAMETER = setting.TYPICAL_PYLON_FILTER_PARAMETER;
            TYPICAL_PYLON_FILTER_PARAMETER_TEMP = setting.TYPICAL_PYLON_FILTER_PARAMETER;
            TYPICAL_PYLON_FILTER_VALUE = setting.TYPICAL_PYLON_FILTER_VALUE;
            TYPICAL_PYLON_FILTER_VALUE_TEMP = setting.TYPICAL_PYLON_FILTER_VALUE;
        }

        private void SaveConfig() {
            var setting = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            setting.NeedWorkWithGeneralView = UserSelectionSettings.NeedWorkWithGeneralView;
            setting.NeedWorkWithGeneralPerpendicularView = NeedWorkWithGeneralPerpendicularView;
            setting.NeedWorkWithTransverseViewFirst = NeedWorkWithTransverseViewFirst;
            setting.NeedWorkWithTransverseViewSecond = NeedWorkWithTransverseViewSecond;
            setting.NeedWorkWithTransverseViewThird = NeedWorkWithTransverseViewThird;
            setting.NeedWorkWithRebarSchedule = NeedWorkWithRebarSchedule;
            setting.NeedWorkWithMaterialSchedule = NeedWorkWithMaterialSchedule;
            setting.NeedWorkWithSystemPartsSchedule = NeedWorkWithSystemPartsSchedule;
            setting.NeedWorkWithIFCPartsSchedule = NeedWorkWithIFCPartsSchedule;
            setting.NeedWorkWithLegend = NeedWorkWithLegend;


            setting.PROJECT_SECTION = PROJECT_SECTION;
            setting.MARK = MARK;
            setting.DISPATCHER_GROUPING_FIRST = DISPATCHER_GROUPING_FIRST;
            setting.DISPATCHER_GROUPING_SECOND = DISPATCHER_GROUPING_SECOND;

            setting.SHEET_SIZE = SHEET_SIZE;
            setting.SHEET_COEFFICIENT = SHEET_COEFFICIENT;
            setting.SHEET_PREFIX = SHEET_PREFIX;
            setting.SHEET_SUFFIX = SHEET_SUFFIX;

            setting.GENERAL_VIEW_PREFIX = GENERAL_VIEW_PREFIX;
            setting.GENERAL_VIEW_SUFFIX = GENERAL_VIEW_SUFFIX;
            setting.GENERAL_VIEW_PERPENDICULAR_PREFIX = GENERAL_VIEW_PERPENDICULAR_PREFIX;
            setting.GENERAL_VIEW_PERPENDICULAR_SUFFIX = GENERAL_VIEW_PERPENDICULAR_SUFFIX;
            setting.GENERAL_VIEW_TEMPLATE_NAME = GENERAL_VIEW_TEMPLATE_NAME;
            setting.GENERAL_VIEW_X_OFFSET = GENERAL_VIEW_X_OFFSET;
            setting.GENERAL_VIEW_Y_TOP_OFFSET = GENERAL_VIEW_Y_TOP_OFFSET;
            setting.GENERAL_VIEW_Y_BOTTOM_OFFSET = GENERAL_VIEW_Y_BOTTOM_OFFSET;

            setting.TRANSVERSE_VIEW_FIRST_PREFIX = TRANSVERSE_VIEW_FIRST_PREFIX;
            setting.TRANSVERSE_VIEW_FIRST_SUFFIX = TRANSVERSE_VIEW_FIRST_SUFFIX;
            setting.TRANSVERSE_VIEW_SECOND_PREFIX = TRANSVERSE_VIEW_SECOND_PREFIX;
            setting.TRANSVERSE_VIEW_SECOND_SUFFIX = TRANSVERSE_VIEW_SECOND_SUFFIX;
            setting.TRANSVERSE_VIEW_THIRD_PREFIX = TRANSVERSE_VIEW_THIRD_PREFIX;
            setting.TRANSVERSE_VIEW_THIRD_SUFFIX = TRANSVERSE_VIEW_THIRD_SUFFIX;
            setting.TRANSVERSE_VIEW_TEMPLATE_NAME = TRANSVERSE_VIEW_TEMPLATE_NAME;

            setting.TRANSVERSE_VIEW_X_OFFSET = TRANSVERSE_VIEW_X_OFFSET;
            setting.TRANSVERSE_VIEW_Y_OFFSET = TRANSVERSE_VIEW_Y_OFFSET;


            setting.REBAR_SCHEDULE_PREFIX = REBAR_SCHEDULE_PREFIX;
            setting.REBAR_SCHEDULE_SUFFIX = REBAR_SCHEDULE_SUFFIX;

            setting.MATERIAL_SCHEDULE_PREFIX = MATERIAL_SCHEDULE_PREFIX;
            setting.MATERIAL_SCHEDULE_SUFFIX = MATERIAL_SCHEDULE_SUFFIX;

            setting.SYSTEM_PARTS_SCHEDULE_PREFIX = SYSTEM_PARTS_SCHEDULE_PREFIX;
            setting.SYSTEM_PARTS_SCHEDULE_SUFFIX = SYSTEM_PARTS_SCHEDULE_SUFFIX;

            setting.IFC_PARTS_SCHEDULE_PREFIX = IFC_PARTS_SCHEDULE_PREFIX;
            setting.IFC_PARTS_SCHEDULE_SUFFIX = IFC_PARTS_SCHEDULE_SUFFIX;

            setting.REBAR_SCHEDULE_NAME = REBAR_SCHEDULE_NAME;
            setting.MATERIAL_SCHEDULE_NAME = MATERIAL_SCHEDULE_NAME;
            setting.SYSTEM_PARTS_SCHEDULE_NAME = SYSTEM_PARTS_SCHEDULE_NAME;
            setting.IFC_PARTS_SCHEDULE_NAME = IFC_PARTS_SCHEDULE_NAME;

            setting.REBAR_SCHEDULE_DISP1 = REBAR_SCHEDULE_DISP1;
            setting.MATERIAL_SCHEDULE_DISP1 = MATERIAL_SCHEDULE_DISP1;
            setting.SYSTEM_PARTS_SCHEDULE_DISP1 = SYSTEM_PARTS_SCHEDULE_DISP1;
            setting.IFC_PARTS_SCHEDULE_DISP1 = IFC_PARTS_SCHEDULE_DISP1;
            setting.REBAR_SCHEDULE_DISP2 = REBAR_SCHEDULE_DISP2;
            setting.MATERIAL_SCHEDULE_DISP2 = MATERIAL_SCHEDULE_DISP2;
            setting.SYSTEM_PARTS_SCHEDULE_DISP2 = SYSTEM_PARTS_SCHEDULE_DISP2;
            setting.IFC_PARTS_SCHEDULE_DISP2 = IFC_PARTS_SCHEDULE_DISP2;

            setting.TYPICAL_PYLON_FILTER_PARAMETER = TYPICAL_PYLON_FILTER_PARAMETER;
            setting.TYPICAL_PYLON_FILTER_VALUE = TYPICAL_PYLON_FILTER_VALUE;


            _pluginConfig.SaveProjectConfig();
        }







        /// <summary>
        /// Ищет эталонные спецификации по указанным именам. На основе эталонных спек создаются спеки для пилонов путем копирования
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

            MATERIAL_SCHEDULE_PREFIX = _materialSchedulePrefixTemp;
            MATERIAL_SCHEDULE_SUFFIX = _materialScheduleSuffixTemp;

            SYSTEM_PARTS_SCHEDULE_PREFIX = _systemPartsSchedulePrefixTemp;
            SYSTEM_PARTS_SCHEDULE_SUFFIX = _systemPartsScheduleSuffixTemp;

            IFC_PARTS_SCHEDULE_PREFIX = _IFCPartsSchedulePrefixTemp;
            IFC_PARTS_SCHEDULE_SUFFIX = _IFCPartsScheduleSuffixTemp;

            REBAR_SCHEDULE_NAME = _rebarScheduleNameTemp;
            MATERIAL_SCHEDULE_NAME = _materialScheduleNameTemp;
            SYSTEM_PARTS_SCHEDULE_NAME = _systemPartsScheduleNameTemp;
            IFC_PARTS_SCHEDULE_NAME = _IFCPartsScheduleNameTemp;

            REBAR_SCHEDULE_DISP1 = _rebarScheduleDisp1Temp;
            MATERIAL_SCHEDULE_DISP1 = _materialScheduleDisp1Temp;
            SYSTEM_PARTS_SCHEDULE_DISP1 = _systemPartsScheduleDisp1Temp;
            IFC_PARTS_SCHEDULE_DISP1 = _IFCPartsScheduleDisp1Temp;

            REBAR_SCHEDULE_DISP2 = _rebarScheduleDisp2Temp;
            MATERIAL_SCHEDULE_DISP2 = _materialScheduleDisp2Temp;
            SYSTEM_PARTS_SCHEDULE_DISP2 = _systemPartsScheduleDisp2Temp;
            IFC_PARTS_SCHEDULE_DISP2 = _IFCPartsScheduleDisp2Temp;

            TYPICAL_PYLON_FILTER_PARAMETER = _typicalPylonFilterParameterTemp;
            TYPICAL_PYLON_FILTER_VALUE = _typicalPylonFilterValueTemp;


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


        private void Test(object p) {

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
                    
                    if(UserSelectionSettings.NeedWorkWithGeneralView) {
                        
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

                    if(NeedWorkWithGeneralPerpendicularView) {

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

                    if(NeedWorkWithTransverseViewFirst) {

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

                    if(NeedWorkWithTransverseViewSecond) {

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

                    if(NeedWorkWithTransverseViewThird) {

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

                    if(NeedWorkWithRebarSchedule) {

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

                    if(NeedWorkWithMaterialSchedule) {

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

                    if(NeedWorkWithSystemPartsSchedule) {

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

                    if(NeedWorkWithIFCPartsSchedule) {

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


                    if(UserSelectionSettings.NeedWorkWithGeneralView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralView.ViewportElement is null) {

                            hostsInfo.GeneralView.ViewSectionPlacer.PlaceGeneralViewport();
                        }
                    }
                    if(NeedWorkWithGeneralPerpendicularView) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.GeneralViewPerpendicular.ViewportElement is null) {

                            hostsInfo.GeneralViewPerpendicular.ViewSectionPlacer.PlaceGeneralPerpendicularViewport();
                        }
                    }
                    if(NeedWorkWithTransverseViewFirst) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewFirst.ViewportElement is null) {

                            hostsInfo.TransverseViewFirst.ViewSectionPlacer.PlaceTransverseFirstViewPorts();
                        }
                    }
                    if(NeedWorkWithTransverseViewSecond) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewSecond.ViewportElement is null) {

                            hostsInfo.TransverseViewSecond.ViewSectionPlacer.PlaceTransverseSecondViewPorts();
                        }
                    }
                    if(NeedWorkWithTransverseViewThird) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.TransverseViewThird.ViewportElement is null) {

                            hostsInfo.TransverseViewThird.ViewSectionPlacer.PlaceTransverseThirdViewPorts();
                        }
                    }
                    if(NeedWorkWithRebarSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.RebarSchedule.ViewportElement is null) {

                            hostsInfo.RebarSchedule.ViewSchedulePlacer.PlaceRebarSchedule();
                        }
                    }
                    if(NeedWorkWithMaterialSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.MaterialSchedule.ViewportElement is null) {

                            hostsInfo.MaterialSchedule.ViewSchedulePlacer.PlaceMaterialSchedule();
                        }
                    }
                    if(NeedWorkWithSystemPartsSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.SystemPartsSchedule.ViewportElement is null) {

                            hostsInfo.SystemPartsSchedule.ViewSchedulePlacer.PlaceSystemPartsSchedule();
                        }
                    }
                    if(NeedWorkWithIFCPartsSchedule) {

                        // Если видовой экран на листе не найден, то размещаем
                        if(hostsInfo.IFCPartsSchedule.ViewportElement is null) {

                            hostsInfo.IFCPartsSchedule.ViewSchedulePlacer.PlaceIFCPartsSchedule();
                        }
                    }
                    if(NeedWorkWithLegend) {

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
                .FirstOrDefault(view => view.Name.Equals(GENERAL_VIEW_TEMPLATE_NAME));
        }
        public void FindTransverseViewTemplate() {
            SelectedTransverseViewTemplate = ViewTemplatesInPj
                .FirstOrDefault(view => view.Name.Equals(TRANSVERSE_VIEW_TEMPLATE_NAME));
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