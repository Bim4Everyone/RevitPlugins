using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;

using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using MS.WindowsAPICodePack.Internal;

using RevitPylonDocumentation.Models;
using View = Autodesk.Revit.DB.View;

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
        private string _sheetGroupingTemp = "_Группа видов 1";
        private string _sheetSizeTemp = "А";
        private string _sheetCoefficientTemp = "х";

        private string _generalViewPrefixTemp = "";
        private string _generalViewSuffixTemp = "";
        private string _transverseViewFirstPrefixTemp = "";
        private string _transverseViewFirstSuffixTemp = "_Сеч.1-1";
        private string _transverseViewSecondPrefixTemp = "";
        private string _transverseViewSecondSuffixTemp = "_Сеч.2-2";
        private string _transverseViewThirdPrefixTemp = "";
        private string _transverseViewThirdSuffixTemp = "_Сеч.3-3";

        private string _rebarSchedulePrefixTemp = "Пилон ";
        private string _rebarScheduleSuffixTemp = "";
        private string _materialSchedulePrefixTemp = "!СМ_Пилон ";
        private string _materialScheduleSuffixTemp = "";
        private string _partsSchedulePrefixTemp = "!ВД_IFC_";
        private string _partsScheduleSuffixTemp = "";
        
        public static string DEF_TITLEBLOCK_NAME = "Создать типы по комплектам";

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
            
            GetTitleBlocks();
            GetLegends();

            CreateSheetsCommand = new RelayCommand(CreateSheets, CanCreateSheets);
            ApplySettingsCommands = new RelayCommand(ApplySettings, CanApplySettings);
        }



        public ICommand ApplySettingsCommands { get; }
        public ICommand CreateSheetsCommand { get; }



        // Рабочие наборы
        /// <summary>
        /// Список всех комплектов документации (по ум. обр_ФОП_Раздел проекта)
        /// </summary>
        public ObservableCollection<string> ProjectSections { get; set; } = new ObservableCollection<string>();

        /// <summary>
        /// Выбранный пользователем комплект документации
        /// </summary>
        public string SelectedProjectSection {
            get => _selectedProjectSection;
            set {
                _selectedProjectSection = value;
                // Запуск обновления списка доступных марок пилонов
                GetHostMarks();                                                                                     
            }
        }


        // Вспомогательные для документации
        /// <summary>
        /// Рамки листов, имеющиеся в проекте
        /// </summary>
        public ObservableCollection<FamilySymbol> TitleBlocks { get; set; } = new ObservableCollection<FamilySymbol>();
        /// <summary>
        /// Выбранная пользователем рамка листа
        /// </summary>
        public FamilySymbol SelectedTitleBlocks { get; set; }
        /// <summary>
        /// Легенды, имеющиеся в проекте
        /// </summary>
        public ObservableCollection<View> Legends { get; set; } = new ObservableCollection<View>();
        /// <summary>
        /// Выбранная пользователем легенда
        /// </summary>
        public static View SelectedLegend { get; set; }


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
                GetHostMarks();
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


        public string SHEET_GROUPING { get; set; } = "_Группа видов 1";
        public string SHEET_GROUPING_TEMP {
            get => _sheetGroupingTemp;
            set {
                _sheetGroupingTemp = value;
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


        public string PARTS_SCHEDULE_PREFIX { get; set; } = "!ВД_IFC_ ";
        public string PARTS_SCHEDULE_PREFIX_TEMP {
            get => _partsSchedulePrefixTemp;
            set {
                _partsSchedulePrefixTemp = value;
                _edited = true;
            }
        }

        public string PARTS_SCHEDULE_SUFFIX { get; set; } = "";
        public string PARTS_SCHEDULE_SUFFIX_TEMP {
            get => _partsScheduleSuffixTemp;
            set {
                _partsScheduleSuffixTemp = value;
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




        // Получаем названия Комплектов документации по пилонам
        private void GetRebarProjectSections() 
        {
            // Пользователь может перезадать параметр раздела, поэтому сначала чистим
            ProjectSections.Clear();
            ErrorText = string.Empty;
            // Пилоны могут быть выполнены категорией Стены или Несущие колонны
            List<BuiltInCategory> pylonCategories = new List<BuiltInCategory>() { BuiltInCategory.OST_Walls, BuiltInCategory.OST_StructuralColumns };

            foreach(var cat in pylonCategories) {

                var elems = new FilteredElementCollector(_revitRepository.Document)
                                    .OfCategory(cat)
                                    .WhereElementIsNotElementType()
                                    .ToElements();


                foreach(var item in elems) {
                    FamilyInstance elem = item as FamilyInstance;

                    if(elem is null || !elem.Name.Contains("Пилон")) {
                        continue;
                    }


                    // Запрашиваем Раздел проекта
                    Autodesk.Revit.DB.Parameter projectSectionParameter = elem.LookupParameter(PROJECT_SECTION);
                    if(projectSectionParameter == null) {
                        ErrorText = "Параметр раздела не найден у элементов Стен или Несущих колонн";
                        return;
                    }
                    string projectSection = projectSectionParameter.AsString();

                    if(projectSection is null) {
                        continue;
                    }
                    // Заполнение словаря Комплект документации - Пилон
                    if(!hostData.ContainsKey(projectSection)) {
                        List<FamilyInstance> hostList = new List<FamilyInstance>() { elem };

                        hostData.Add(projectSection, hostList);
                    } else {
                        hostData[projectSection].Add(elem);
                    }


                    // Заполнение списка разделов проекта
                    if(!ProjectSections.Contains(projectSection)) {
                        ProjectSections.Add(projectSection);
                    }
                }
            }
            // Сортируем
            ProjectSections = new ObservableCollection<string>(ProjectSections.OrderBy(i => i));
        }


        // Метод для авто обновления списка марок пилонов при выборе рабочего набора
        private void GetHostMarks() 
        {
            HostMarks.Clear();
            ErrorText= string.Empty;

            // Перебираем хосты выбранного раздела
            foreach(FamilyInstance host in hostData[SelectedProjectSection]) 
            {
                Autodesk.Revit.DB.Parameter hostMarkParameter = host.LookupParameter(MARK);
                if(hostMarkParameter == null) {
                    ErrorText = "Параметр марки не найден у элементов Стен или Несущих колонн";
                    return;
                }

                string hostMark = hostMarkParameter.AsString();
                if(hostMark is null) {
                    continue;
                }

                if(!hostMark.Contains(HostMarkForSearch)) {
                    continue;
                }


                // Заполнение списка марок основ
                if(!HostMarks.Contains(hostMark)) {
                    HostMarks.Add(hostMark);
                }
            }
        }


        private void GetTitleBlocks() {
            var titleBlocksInProject = new FilteredElementCollector(_revitRepository.Document)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsElementType()
                .ToElements();


            // Перевод и отбор листов с пилонами
            foreach(var item in titleBlocksInProject) {
                FamilySymbol titleBlock = item as FamilySymbol;
                if(titleBlock == null) {
                    continue;
                }

                TitleBlocks.Add(titleBlock);

                if(titleBlock.Name == DEF_TITLEBLOCK_NAME) {
                    SelectedTitleBlocks = titleBlock;
                }
            }
        }


        private void GetLegends() {
            var legendsInProject = new FilteredElementCollector(_revitRepository.Document)
                .OfClass(typeof(View))
                .WhereElementIsNotElementType()
                .ToElements();


            // Перевод и отбор легенд
            foreach(var item in legendsInProject) {
                View view = item as View;
                if(view == null) {
                    continue;
                }

                if(view.ViewType == ViewType.Legend) {
                    Legends.Add(view);

                    if(view.Name.Contains("илон")) {
                        SelectedLegend = view;
                    }
                }
            }
        }



        private void ApplySettings(object p) {
            // Необходимо будет написать метод проверки имен параметров - есть ли такие параметры у нужных категорий
            
            // Получаем заново список заполненных разделов проекта
            GetRebarProjectSections();

            // Устанавливаем флаг, что применили параметры и перезаписываем параметры
            PROJECT_SECTION = _projectSectionTemp;
            MARK = _markTemp;
            SHEET_GROUPING= _sheetGroupingTemp;
            SHEET_SIZE = _sheetSizeTemp;
            SHEET_COEFFICIENT = _sheetCoefficientTemp;
            GENERAL_VIEW_PREFIX = _generalViewPrefixTemp;
            GENERAL_VIEW_SUFFIX = _generalViewSuffixTemp;
            TRANSVERSE_VIEW_FIRST_PREFIX = _transverseViewFirstPrefixTemp;
            TRANSVERSE_VIEW_FIRST_SUFFIX = _transverseViewFirstSuffixTemp;
            TRANSVERSE_VIEW_SECOND_PREFIX = _transverseViewSecondPrefixTemp;
            TRANSVERSE_VIEW_SECOND_SUFFIX = _transverseViewSecondSuffixTemp;
            TRANSVERSE_VIEW_THIRD_PREFIX = _transverseViewThirdPrefixTemp;
            TRANSVERSE_VIEW_THIRD_SUFFIX = _transverseViewThirdSuffixTemp;
            REBAR_SCHEDULE_PREFIX = _rebarSchedulePrefixTemp;
            REBAR_SCHEDULE_SUFFIX = _rebarScheduleSuffixTemp;
            MATERIAL_SCHEDULE_PREFIX = _materialSchedulePrefixTemp;
            MATERIAL_SCHEDULE_SUFFIX = _materialScheduleSuffixTemp;
            PARTS_SCHEDULE_PREFIX = _partsSchedulePrefixTemp;
            PARTS_SCHEDULE_SUFFIX = _partsScheduleSuffixTemp;


            _edited = false;
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


                Autodesk.Revit.DB.Parameter viewSheetGroupingParameter = viewSheet.LookupParameter(SHEET_GROUPING);
                if(viewSheetGroupingParameter == null) {
                    #region Отчет
                    Report = "\tПараметр \"" + SHEET_GROUPING + "\" не заполнен, т.к. не был найден у листа";
                    #endregion
                } else {
                    viewSheetGroupingParameter.Set(SelectedProjectSection);
                    #region Отчет
                    Report = "\tГруппировка по параметру \"" + SHEET_GROUPING + "\": " + SelectedProjectSection;
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
                missingPylonSheetsInfo[sheetKeyName].PlaceGeneralViewport();
                missingPylonSheetsInfo[sheetKeyName].PlaceTransverseViewPorts();

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
            string partsSchedule = PARTS_SCHEDULE_PREFIX + PARTS_SCHEDULE_SUFFIX;

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
                    existingPylonSheetsInfo.Add(sheetKeyName, new PylonSheetInfo(this, sheetKeyName) {
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
                    missingPylonSheetsInfo.Add(hostMark, new PylonSheetInfo(this, hostMark));
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

    }
}