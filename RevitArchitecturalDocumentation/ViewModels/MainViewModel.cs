using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Ninject.Planning.Targets;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Views;

using static System.Net.Mime.MediaTypeNames;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private List<Element> _visibilityScopes;
        private List<Level> _levels;
        private List<ViewFamilyType> _viewFamilyTypes;
        private List<ElementType> _viewportTypes;
        private List<FamilySymbol> _titleBlocksInProject;
        private ViewFamilyType _selectedViewFamilyType;
        private string _selectedViewFamilyTypeName;
        private ElementType _selectedViewportType;
        private string _selectedViewportTypeName;
        private FamilySymbol _selectedTitleBlock;
        private string _selectedTitleBlockName;
        private List<View> _selectedViews = new List<View>();
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();
        private TaskInfo _selectedTask;
        private string _viewNamePrefix = string.Empty;
        private string _selectedFilterNameForSpecs = string.Empty;
        private string _sheetNamePrefix = string.Empty;
        private List<string> _filterNamesFromSpecs = new List<string>();
        private bool _workWithSheets = true;
        private bool _workWithViews = true;
        private bool _workWithSpecs = true;
        private bool _createViewsFromSelected = true;

        private StringBuilder _report = new StringBuilder();
        private Regex _regexForBuildingPart = new Regex(@"К(.*?)_");
        private Regex _regexForBuildingSection = new Regex(@"С(.*?)$");
        private Regex _regexForBuildingSectionPart = new Regex(@"часть (.*?)$");
        private Regex _regexForLevel = new Regex(@"^(.*?) ");
        private Regex _regexForView = new Regex(@"псо_(.*?) этаж");

        private string _errorText;


        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask);

            SelectSpecsCommand = new RelayCommand(SelectSpecs);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectSpecsCommand { get; }



        public StringBuilder Report {
            get => _report;
            set => this.RaiseAndSetIfChanged(ref _report, value);
        }
        public Regex RegexForBuildingPart {
            get => _regexForBuildingPart;
            set => this.RaiseAndSetIfChanged(ref _regexForBuildingPart, value);
        }
        public Regex RegexForBuildingSection {
            get => _regexForBuildingSection;
            set => this.RaiseAndSetIfChanged(ref _regexForBuildingSection, value);
        }
        public Regex RegexForBuildingSectionPart {
            get => _regexForBuildingSectionPart;
            set => this.RaiseAndSetIfChanged(ref _regexForBuildingSectionPart, value);
        }
        public Regex RegexForLevel {
            get => _regexForLevel;
            set => this.RaiseAndSetIfChanged(ref _regexForLevel, value);
        }
        public Regex RegexForView {
            get => _regexForView;
            set => this.RaiseAndSetIfChanged(ref _regexForView, value);
        }

        public List<View> SelectedViews {
            get => _selectedViews;
            set => this.RaiseAndSetIfChanged(ref _selectedViews, value);
        }

        public ObservableCollection<TaskInfo> TasksForWork {
            get => _tasksForWork;
            set => this.RaiseAndSetIfChanged(ref _tasksForWork, value);
        }

        public TaskInfo SelectedTask {
            get => _selectedTask;
            set => this.RaiseAndSetIfChanged(ref _selectedTask, value);
        }

        public List<Element> VisibilityScopes {
            get => _visibilityScopes;
            set => this.RaiseAndSetIfChanged(ref _visibilityScopes, value);
        }

        public List<Level> Levels {
            get => _levels;
            set => this.RaiseAndSetIfChanged(ref _levels, value);
        }

        public List<ViewFamilyType> ViewFamilyTypes {
            get => _viewFamilyTypes;
            set => this.RaiseAndSetIfChanged(ref _viewFamilyTypes, value);
        }

        public ViewFamilyType SelectedViewFamilyType {
            get => _selectedViewFamilyType;
            set => this.RaiseAndSetIfChanged(ref _selectedViewFamilyType, value);
        }

        public string SelectedViewFamilyTypeName {
            get => _selectedViewFamilyTypeName;
            set => this.RaiseAndSetIfChanged(ref _selectedViewFamilyTypeName, value);
        }

        public List<ElementType> ViewportTypes {
            get => _viewportTypes;
            set => this.RaiseAndSetIfChanged(ref _viewportTypes, value);
        }

        public ElementType SelectedViewportType {
            get => _selectedViewportType;
            set => this.RaiseAndSetIfChanged(ref _selectedViewportType, value);
        }

        public string SelectedViewportTypeName {
            get => _selectedViewportTypeName;
            set => this.RaiseAndSetIfChanged(ref _selectedViewportTypeName, value);
        }

        public bool CreateViewsFromSelected {
            get => _createViewsFromSelected;
            set => this.RaiseAndSetIfChanged(ref _createViewsFromSelected, value);
        }

        public List<FamilySymbol> TitleBlocksInProject {
            get => _titleBlocksInProject;
            set => this.RaiseAndSetIfChanged(ref _titleBlocksInProject, value);
        }

        public FamilySymbol SelectedTitleBlock {
            get => _selectedTitleBlock;
            set => this.RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
        }

        public string SelectedTitleBlockName {
            get => _selectedTitleBlockName;
            set => this.RaiseAndSetIfChanged(ref _selectedTitleBlockName, value);
        }

        public string ViewNamePrefix {
            get => _viewNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _viewNamePrefix, value);
        }

        public string SheetNamePrefix {
            get => _sheetNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _sheetNamePrefix, value);
        }

        public bool WorkWithSheets {
            get => _workWithSheets;
            set => this.RaiseAndSetIfChanged(ref _workWithSheets, value);
        }

        public bool WorkWithViews {
            get => _workWithViews;
            set => this.RaiseAndSetIfChanged(ref _workWithViews, value);
        }

        public bool WorkWithSpecs {
            get => _workWithSpecs;
            set => this.RaiseAndSetIfChanged(ref _workWithSpecs, value);
        }

        public List<string> FilterNamesFromSpecs {
            get => _filterNamesFromSpecs;
            set => this.RaiseAndSetIfChanged(ref _filterNamesFromSpecs, value);
        }

        public string SelectedFilterNameForSpecs {
            get => _selectedFilterNameForSpecs;
            set => this.RaiseAndSetIfChanged(ref _selectedFilterNameForSpecs, value);
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

            VisibilityScopes = _revitRepository.VisibilityScopes;
            Levels = _revitRepository.Levels;
            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
            ViewportTypes = _revitRepository.ViewportTypes;
            TitleBlocksInProject = _revitRepository.TitleBlocksInProject;

            SelectedViewFamilyType = ViewFamilyTypes.FirstOrDefault(a => a.Name.Equals(SelectedViewFamilyTypeName));
            SelectedViewportType = ViewportTypes.FirstOrDefault(a => a.Name.Equals(SelectedViewportTypeName));
            SelectedTitleBlock = TitleBlocksInProject.FirstOrDefault(a => a.Name.Equals(SelectedTitleBlockName));

            if(TasksForWork.Count == 0) {
                TasksForWork.Add(new TaskInfo(this));
            }

            GetSelectedViews();
        }

        /// <summary>
        /// Метод, отрабатывающий при нажатии кнопки "Ок"
        /// </summary>
        private void AcceptView() {

            SaveConfig();
            DoWork();
        }

        /// <summary>
        /// Определяет можно ли запустить работу плагина
        /// </summary>
        private bool CanAcceptView() {

            if(WorkWithViews && SelectedViewFamilyType is null) {
                return false;
            }

            if(WorkWithViews && SelectedViewportType is null) {
                return false;
            }

            if(WorkWithSheets && SelectedTitleBlock is null) {
                return false;
            }

            return true;
        }


        /// <summary>
        /// Подгружает параметры плагина с предыдущего запуска
        /// </summary>
        private void LoadConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document);

            if(settings is null) { return; }

            WorkWithSheets = settings.WorkWithSheets;
            WorkWithViews = settings.WorkWithViews;
            WorkWithSpecs = settings.WorkWithSpecs;

            CreateViewsFromSelected = settings.CreateViewsFromSelected;
            SheetNamePrefix = settings.SheetNamePrefix;
            ViewNamePrefix = settings.ViewNamePrefix;
            SelectedTitleBlockName = settings.SelectedTitleBlockName;
            SelectedViewFamilyTypeName = settings.SelectedViewFamilyTypeName;
            SelectedViewportTypeName = settings.SelectedViewportTypeName;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            settings.WorkWithSheets = WorkWithSheets;
            settings.WorkWithViews = WorkWithViews;
            settings.WorkWithSpecs = WorkWithSpecs;

            settings.CreateViewsFromSelected = CreateViewsFromSelected;
            settings.SheetNamePrefix = SheetNamePrefix;
            settings.ViewNamePrefix = ViewNamePrefix;

            settings.SelectedTitleBlockName = SelectedTitleBlock.Name;
            settings.SelectedViewFamilyTypeName = SelectedViewFamilyType.Name;
            settings.SelectedViewportTypeName = SelectedViewportType.Name;


            _pluginConfig.SaveProjectConfig();
        }

        private void GetSelectedViews() {

            // При работе с ДДУ листы пользователь должен выбрать заранее, т.к. селектор API не позволяет выбирать элементы из диспетчера
            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                View view = _revitRepository.Document.GetElement(id) as View;
                if(view != null) {
                    SelectedViews.Add(view);
                }
            }
            Report.AppendLine($"Выбрано видов до запуска плагина: {SelectedViews.Count}");
        }

        private void AddTask() {

            TasksForWork.Add(new TaskInfo(this));
        }

        private void DeleteTask() {

            if(SelectedTask != null) {
                TasksForWork.Remove(SelectedTask);
            }
            GetFilterNames();
        }



        private void SelectSpecs(object obj) {

            TaskInfo task = obj as TaskInfo;
            if(task != null) {
                task.ScheduleSheetInstances.Clear();

                ISelectionFilter selectFilter = new ScheduleSelectionFilter();
                IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");

                foreach(Reference reference in references) {

                    ScheduleSheetInstance elem = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                    if(elem is null) {
                        continue;
                    }

                    SpecHelper specHelper = new SpecHelper(this, elem);
                    task.ScheduleSheetInstances.Add(specHelper);
                    specHelper.GetInfo();
                }
                GetFilterNames();
            }


            MainWindow mainWindow = new MainWindow {
                DataContext = this
            };
            mainWindow.ShowDialog();
        }



        /// <summary>
        /// Метод перебирает все выбранные спеки во всех заданиях и собирает список параметров фильтрации. принадлежащий всем одновременно
        /// </summary>
        private void GetFilterNames() {

            FilterNamesFromSpecs.Clear();
            foreach(TaskInfo task in TasksForWork) {

                foreach(SpecHelper spec in task.ScheduleSheetInstances) {
                    if(FilterNamesFromSpecs.Count == 0) {
                        FilterNamesFromSpecs.AddRange(spec.GetFilterNames());
                    } else {
                        FilterNamesFromSpecs = FilterNamesFromSpecs.Intersect(spec.GetFilterNames()).ToList();
                    }
                }
            }
        }



        /// <summary>
        /// Анализируем каждую задачу, проверяя выбор пользователя и получая данные в числовом формате
        /// </summary>
        private void AnalizeTasks() {

            Report.AppendLine("Анализируем задачи:");

            foreach(TaskInfo task in TasksForWork) {
                Report.AppendLine($"    Задача {TasksForWork.IndexOf(task) + 1}");
                task.AnalizeTask();
            }
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает лист с указанным именем
        /// </summary>
        private ViewSheet GetOrCreateSheet(string newSheetName) {

            ViewSheet newSheet = _revitRepository.GetSheetByName(newSheetName);
            if(newSheet is null) {
                Report.AppendLine($"                Лист с именем {newSheetName} не найден в проекте, приступаем к созданию");
                try {
                    newSheet = ViewSheet.Create(_revitRepository.Document, SelectedTitleBlock.Id);
                    Report.AppendLine($"                Лист успешно создан!");
                    newSheet.Name = newSheetName;
                    Report.AppendLine($"                Задано имя: {newSheet.Name}");

                    // Ищем рамку листа
                    FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, newSheet.Id)
                        .OfCategory(BuiltInCategory.OST_TitleBlocks)
                        .WhereElementIsNotElementType()
                        .FirstOrDefault() as FamilyInstance;

                    if(titleBlock != null) {
                        Parameter widthParam = titleBlock.LookupParameter("Ширина");
                        Parameter heightParam = titleBlock.LookupParameter("Высота");

                        if(widthParam != null && heightParam != null) {
                            titleBlock.LookupParameter("Ширина").Set(150 / 304.8);
                            titleBlock.LookupParameter("Высота").Set(110 / 304.8);
                        }
                    }

                    _revitRepository.Document.Regenerate();
                } catch(Exception) {
                    Report.AppendLine($"❗               Произошла ошибка при создании листа!");
                }
            } else {
                Report.AppendLine($"                Лист с именем {newSheetName} успешно найден в проекте!");
            }
            return newSheet;
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает/дублирует вид с указанным именем
        /// </summary>
        private ViewPlan GetOrCreateView(TaskInfo task, string newViewName, Level level = null, View view = null) {

            ViewPlan newViewPlan = _revitRepository.GetViewByName(newViewName);
            if(newViewPlan is null) {
                Report.AppendLine($"                Вид с именем {newViewName} не найден в проекте, приступаем к созданию");
                try {
                    if(CreateViewsFromSelected) {
                        ElementId newViewPlanId = view.Duplicate(ViewDuplicateOption.WithDetailing);
                        newViewPlan = view.Document.GetElement(newViewPlanId) as ViewPlan;
                        if(newViewPlan is null) {
                            Report.AppendLine($"❗               Произошла ошибка при дублировании вида!");
                            return null;
                        }
                        Report.AppendLine($"                Вид успешно продублирован!");
                    } else {
                        newViewPlan = ViewPlan.Create(_revitRepository.Document, SelectedViewFamilyType.Id, level.Id);
                        Report.AppendLine($"                Вид успешно создан!");
                    }
                    newViewPlan.Name = newViewName;
                    Report.AppendLine($"                Задано имя: {newViewPlan.Name}");
                    newViewPlan.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(task.SelectedVisibilityScope.Id);
                    Report.AppendLine($"                Задана область видимости: {task.SelectedVisibilityScope.Name}");

                    newViewPlan.get_Parameter(BuiltInParameter.VIEWER_ANNOTATION_CROP_ACTIVE).Set(1);
                    Report.AppendLine($"                Задана образка аннотаций на виде");

                    ViewCropRegionShapeManager cropManager = newViewPlan.GetCropRegionShapeManager();
                    double dim = UnitUtilsHelper.ConvertToInternalValue(3);
                    cropManager.TopAnnotationCropOffset = dim;
                    cropManager.BottomAnnotationCropOffset = dim;
                    cropManager.LeftAnnotationCropOffset = dim;
                    cropManager.RightAnnotationCropOffset = dim;
                    Report.AppendLine($"                Задано минимальное смещение обрезки аннотаций");

                } catch(Exception) {
                    Report.AppendLine($"❗               Произошла ошибка при работе с видом!");
                }
            } else {
                Report.AppendLine($"                Вид с именем {newViewName} успешно найден в проекте!");
            }
            return newViewPlan;
        }


        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает спецификацию с указанным именем и задает ей фильрацию
        /// </summary>
        private ViewSchedule GetOrCreateSpec(SpecHelper specHelper, string newSpecName, int numberOfLevelAsInt) {

            ViewSchedule newViewSpec = _revitRepository.GetSpecByName(newSpecName);
            if(newViewSpec is null) {
                try {
                    Report.AppendLine($"                Спецификация с именем {newSpecName} не найдена в проекте, приступаем к созданию");
                    newViewSpec = _revitRepository.Document.GetElement(specHelper.Specification.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                    Report.AppendLine($"                Спецификация успешно создана!");
                    newViewSpec.Name = newSpecName;
                    Report.AppendLine($"                Задано имя: {newViewSpec.Name}");
                    SpecHelper newSpec = new SpecHelper(this, newViewSpec);
                    newSpec.ChangeSpecFilters(SelectedFilterNameForSpecs, numberOfLevelAsInt);
                    Report.AppendLine($"                Фильтрация задана успешно!");
                } catch(Exception) {
                    Report.AppendLine($"❗               Произошла ошибка при работе со спецификацией!");
                }
            } else {
                Report.AppendLine($"                Спецификация с именем {newSpecName} успешно найдена в проекте!");
            }

            return newViewSpec;
        }



        /// <summary>
        /// Метод находит в проекте, а если не нашел, то создает спецификацию с указанным именем и задает ей фильрацию
        /// </summary>
        private Viewport PlaceViewportOnSheet(ViewSheet viewSheet, ViewPlan viewPlan) {

            // Размещаем план на листе
            Viewport viewPort = Viewport.Create(_revitRepository.Document, viewSheet.Id, viewPlan.Id, new XYZ(0, 0, 0));
            if(viewPort is null) {
                Report.AppendLine($"❗       Не удалось создать вид на листе!");
                return null;
            }
            Report.AppendLine($"        Видовой экран успешно создан на листе!");

            if(SelectedViewportType != null) {
                viewPort.ChangeTypeId(SelectedViewportType.Id);
                Report.AppendLine($"        Видовому экрану задан тип {SelectedViewportType.Name}!");
            }

            XYZ viewportCenter = viewPort.GetBoxCenter();
            Outline viewportOutline = viewPort.GetBoxOutline();
            double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
            //double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

            // Ищем рамку листа
            FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, viewSheet.Id)
                .OfCategory(BuiltInCategory.OST_TitleBlocks)
                .WhereElementIsNotElementType()
                .FirstOrDefault() as FamilyInstance;

            if(titleBlock is null) {
                Report.AppendLine($"❗       Не удалось найти рамку листа, она нужна для правильного расположения вида на листе!");
                return null;
            }

            _revitRepository.Document.Regenerate();

            // Получение габаритов рамки листа
            BoundingBoxXYZ boundingBoxXYZ = titleBlock.get_BoundingBox(viewSheet);
            double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
            double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

            double titleBlockMinY = boundingBoxXYZ.Min.Y;
            double titleBlockMinX = boundingBoxXYZ.Min.X;

            XYZ correctPosition = new XYZ(
                titleBlockMinX + viewportHalfWidth,
                titleBlockHeight / 2 + titleBlockMinY,
                0);

            viewPort.SetBoxCenter(correctPosition);
            Report.AppendLine($"        Вид успешно спозиционирован на листе!");

#if REVIT_2022_OR_GREATER
            viewPort.LabelOffset = new XYZ(0.142591947719928, 0.318344950433976, 0);
            Report.AppendLine($"        Оглавление вида успешно спозиционировано на листе!");
#endif

            return viewPort;
        }



        /// <summary>
        /// Метод находит на листе, а если не нашел, то создает видовой экран спецификации
        /// </summary>
        private ScheduleSheetInstance PlaceSpecViewportOnSheet(SpecHelper specHelper, ViewSheet viewSheet, ViewSchedule viewSchedule) {

            ScheduleSheetInstance newScheduleSheetInstance = _revitRepository.GetSpecFromSheetByName(viewSheet, viewSchedule.Name);

            // Если спека не найдена на листе, то добавляем ее
            if(newScheduleSheetInstance is null) {
                newScheduleSheetInstance = ScheduleSheetInstance.Create(
                    _revitRepository.Document,
                    viewSheet.Id,
                    viewSchedule.Id,
                    specHelper.SpecSheetInstancePoint);

                Report.AppendLine($"        Спецификация успешно размещена на листе!");
            } else {
                Report.AppendLine($"        Спецификация уже была размещена на листе!");
            }

            return newScheduleSheetInstance;
        }


        private void DoWork() {

            AnalizeTasks();

            if(TasksForWork.FirstOrDefault(o => o.CanWorkWithIt) is null) {

                Report.AppendLine("❗   Все задания имеют ошибки. Выполнение скрипта остановлено!");
                TaskDialog.Show("Документатор АР. Отчет", Report.ToString());
                return;
            }

            Report.AppendLine($"Приступаю к выполнению задания. Всего задач: {TasksForWork.Count}");
            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор АР")) {

                if(CreateViewsFromSelected) {

                    TaskDialog.Show("fd", "создание с видов");
                    TaskDialog.Show("Число выбранных видов", SelectedViews.Count.ToString());

                    foreach(View view in SelectedViews) {

                        string numberOfLevel = RegexForView.Match(view.Name.ToLower()).Groups[1].Value;

                        if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                            continue;
                        }

                        string viewNamePartWithSectionPart = string.Empty;

                        if(view.Name.ToLower().Contains("_часть ")) {
                            viewNamePartWithSectionPart = "_часть ";
                            viewNamePartWithSectionPart += RegexForBuildingSectionPart.Match(view.Name.ToLower()).Groups[1].Value;
                        }

                        int c = 0;
                        foreach(TaskInfo task in TasksForWork) {

                            c++;
                            Report.AppendLine($"    Вид \"{view.Name}\", задание номер {c}");

                            // Пропускаем те задания, в которых есть ошибки
                            if(!task.CanWorkWithIt) {
                                Report.AppendLine($"❗               В задании имеются ошибки, работа по нему выполнена не будет!");
                                continue;
                            }

                            if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {

                                continue;
                            }

                            ViewSheet newSheet = null;
                            if(WorkWithSheets) {

                                string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                    SheetNamePrefix,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    numberOfLevel);

                                newSheet = GetOrCreateSheet(newSheetName);
                            }

                            ViewPlan newViewPlan = null;
                            if(WorkWithViews) {

                                string newViewName = string.Format("{0}{1} этаж К{2}_С{3}{4}{5}",
                                    ViewNamePrefix,
                                    numberOfLevel,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    viewNamePartWithSectionPart,
                                    task.ViewNameSuffix);

                                newViewPlan = GetOrCreateView(task, newViewName, null, view);

                                if(newSheet != null && newViewPlan != null && Viewport.CanAddViewToSheet(_revitRepository.Document, newSheet.Id, newViewPlan.Id)) {

                                    PlaceViewportOnSheet(newSheet, newViewPlan);
                                }
                            }

                            ViewSchedule newViewSchedule = null;
                            if(WorkWithSpecs) {

                                foreach(SpecHelper specHelper in task.ScheduleSheetInstances) {

                                    if(!specHelper.CanWorkWithIt) {
                                        Report.AppendLine($"❗               В задании спецификации имеются ошибки, создание спецификации отменено!");
                                        continue;
                                    }

                                    string newScheduleName = specHelper.FirstPartOfSpecName
                                                            + String.Format(specHelper.FormatOfLevelNumber, numberOfLevelAsInt)
                                                            + specHelper.SuffixOfLevelNumber
                                                            + specHelper.LastPartOfSpecName;

                                    newViewSchedule = GetOrCreateSpec(specHelper, newScheduleName, numberOfLevelAsInt);

                                    // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование
                                    if(newSheet != null && newViewSchedule != null && _revitRepository.GetSpecFromSheetByName(newSheet, newScheduleName) is null) {

                                        ScheduleSheetInstance newScheduleSheetInstance = ScheduleSheetInstance.Create(
                                            _revitRepository.Document,
                                            newSheet.Id,
                                            newViewSchedule.Id,
                                            specHelper.SpecSheetInstancePoint);

                                        if(newScheduleSheetInstance is null) {
                                            Report.AppendLine($"❗       Не удалось создать видовой экран спецификации на листе!");
                                        } else {
                                            Report.AppendLine($"        Видовой экран спецификации успешно создан на листе!");
                                        }
                                    }
                                }
                            }
                        }
                    }
                } else {

                    TaskDialog.Show("fd", "создание с нуля");

                    Report.AppendLine($"Плагин перебирает уровни и на каждом пытается выполнить задания по очереди.");
                    foreach(Level level in Levels) {

                        Report.AppendLine($"Уровень \"{level.Name}\"");

                        string numberOfLevel = RegexForLevel.Match(level.Name).Groups[1].Value;
                        if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                            Report.AppendLine($"❗       Не удалось определить номер уровня {level.Name}!");
                            continue;
                        }
                        Report.AppendLine($"    Уровень номер: {numberOfLevelAsInt}");

                        int c = 0;
                        foreach(TaskInfo task in TasksForWork) {

                            c++;
                            Report.AppendLine($"    Задание номер {c}");

                            // Пропускаем те задания, в которых есть ошибки
                            if(!task.CanWorkWithIt) {
                                Report.AppendLine($"❗               В задании имеются ошибки, работа по нему выполнена не будет!");
                                continue;
                            }

                            string strForLevelSearch = "К" + task.NumberOfBuildingPartAsInt.ToString() + "_";
                            if(!level.Name.Contains(strForLevelSearch)) {
                                Report.AppendLine($"        Уровень не относится к нужному корпусу, т.к. не содержит: \"{strForLevelSearch}\":");
                                continue;
                            } else {
                                Report.AppendLine($"        Уровень относится к нужному корпусу, т.к. содержит: \"{strForLevelSearch}\":");
                            }

                            if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                                Report.AppendLine($"        Уровень: {level.Name} не подходит под диапазон {task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}:");
                                continue;
                            }
                            Report.AppendLine($"        Уровень: {level.Name} подходит под диапазон {task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}:");


                            ViewSheet newSheet = null;
                            if(WorkWithSheets) {

                                string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                    SheetNamePrefix,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    numberOfLevel);

                                newSheet = GetOrCreateSheet(newSheetName);
                            }


                            ViewPlan newViewPlan = null;
                            if(WorkWithViews) {

                                string newViewName = string.Format("{0}{1} этаж К{2}{3}",
                                    ViewNamePrefix,
                                    numberOfLevel,
                                    task.NumberOfBuildingPartAsInt,
                                    task.ViewNameSuffix);

                                newViewPlan = GetOrCreateView(task, newViewName, level);

                                if(newSheet != null && newViewPlan != null && Viewport.CanAddViewToSheet(_revitRepository.Document, newSheet.Id, newViewPlan.Id)) {

                                    PlaceViewportOnSheet(newSheet, newViewPlan);
                                }
                            }


                            ViewSchedule newViewSchedule = null;
                            if(WorkWithSpecs) {

                                foreach(SpecHelper specHelper in task.ScheduleSheetInstances) {

                                    if(!specHelper.CanWorkWithIt) {
                                        Report.AppendLine($"❗               В задании спецификации имеются ошибки, создание спецификации отменено!");
                                        continue;
                                    }

                                    string newScheduleName = specHelper.FirstPartOfSpecName
                                                            + String.Format(specHelper.FormatOfLevelNumber, numberOfLevelAsInt)
                                                            + specHelper.SuffixOfLevelNumber
                                                            + specHelper.LastPartOfSpecName;

                                    newViewSchedule = GetOrCreateSpec(specHelper, newScheduleName, numberOfLevelAsInt);



                                    // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование, 
                                    // в случае если лист и спека существуют и видовой экран спеки не найден на листе
                                    if(newSheet != null && newViewSchedule != null) {

                                        PlaceSpecViewportOnSheet(specHelper, newSheet, newViewSchedule);
                                    }
                                }
                            }
                        }
                    }
                }

                TaskDialog.Show("Документатор АР. Отчет", Report.ToString());

                transaction.Commit();
            }
        }
    }
}