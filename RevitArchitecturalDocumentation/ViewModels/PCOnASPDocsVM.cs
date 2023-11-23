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
    internal class PCOnASPDocsVM : BaseViewModel {
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
        private ObservableCollection<ViewPlan> _selectedViews = new ObservableCollection<ViewPlan>();
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();
        private TaskInfo _selectedTask;
        private string _viewNamePrefix = string.Empty;
        private string _selectedFilterNameForSpecs = string.Empty;
        private string _sheetNamePrefix = string.Empty;
        private List<string> _filterNamesFromSpecs = new List<string>();
        private bool _workWithSheets = true;
        private bool _workWithViews = true;
        private bool _workWithSpecs = true;
        private bool _createViewsFromSelected = false;

        private StringBuilder _report = new StringBuilder();
        private Regex _regexForBuildingPart = new Regex(@"К(.*?)_");
        private Regex _regexForBuildingSection = new Regex(@"С(.*?)$");
        private Regex _regexForBuildingSectionPart = new Regex(@"часть (.*?)$");
        private Regex _regexForLevel = new Regex(@"^(.*?) ");
        private Regex _regexForView = new Regex(@"псо_(.*?) этаж");

        private string _errorText;


        public PCOnASPDocsVM(PluginConfig pluginConfig, RevitRepository revitRepository) {
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

        public ObservableCollection<ViewPlan> SelectedViews {
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
                TasksForWork.Add(new TaskInfo(RegexForBuildingPart, RegexForBuildingSection, Report));
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

            if(WorkWithSpecs && SelectedFilterNameForSpecs is null) {
                return false;
            }

            foreach(TaskInfo task in TasksForWork) {

                if(task.SelectedVisibilityScope is null) {
                    return false;
                }
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
            SelectedFilterNameForSpecs = settings.SelectedFilterNameForSpecs;
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
            settings.SelectedFilterNameForSpecs = SelectedFilterNameForSpecs;


            _pluginConfig.SaveProjectConfig();
        }

        private void GetSelectedViews() {

            // При работе с ДДУ листы пользователь должен выбрать заранее, т.к. селектор API не позволяет выбирать элементы из диспетчера
            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                ViewPlan view = _revitRepository.Document.GetElement(id) as ViewPlan;
                if(view != null) {
                    SelectedViews.Add(view);
                }
            }
            Report.AppendLine($"Выбрано видов до запуска плагина: {SelectedViews.Count}");
        }

        private void AddTask() {

            TasksForWork.Add(new TaskInfo(RegexForBuildingPart, RegexForBuildingSection, Report));
        }

        private void DeleteTask() {

            if(SelectedTask != null) {
                TasksForWork.Remove(SelectedTask);
            }
            GetFilterNames();
        }


        /// <summary>
        /// После скрытия окна позволяет выбрать видовые экраны спек в Revit
        /// </summary>
        private void SelectSpecs(object obj) {

            TaskInfo task = obj as TaskInfo;
            if(task != null) {
                task.ScheduleSheetInstances.Clear();

                ISelectionFilter selectFilter = new ScheduleSelectionFilter();
                IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");

                foreach(Reference reference in references) {

                    ScheduleSheetInstance scheduleSheetInstance = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                    if(scheduleSheetInstance is null) {
                        continue;
                    }

                    SpecHelper specHelper = new SpecHelper(_revitRepository, scheduleSheetInstance, Report);
                    task.ScheduleSheetInstances.Add(specHelper);
                    specHelper.GetNameInfo();
                }
                GetFilterNames();
            }


            PCOnASPDocsV window = new PCOnASPDocsV {
                DataContext = this
            };
            window.ShowDialog();
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

                    foreach(ViewPlan view in SelectedViews) {

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

                            SheetHelper sheetHelper = null;
                            if(WorkWithSheets) {

                                string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                    SheetNamePrefix,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    numberOfLevel);

                                sheetHelper = new SheetHelper(_revitRepository, Report);
                                sheetHelper.GetOrCreateSheet(newSheetName, SelectedTitleBlock, "Ширина", "Высота", 150, 110);
                            }


                            ViewHelper viewHelper = null;
                            if(WorkWithViews) {

                                string newViewName = string.Format("{0}{1} этаж К{2}_С{3}{4}{5}",
                                    ViewNamePrefix,
                                    numberOfLevel,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    viewNamePartWithSectionPart,
                                    task.ViewNameSuffix);

                                viewHelper = new ViewHelper(Report, _revitRepository);
                                viewHelper.GetView(newViewName, task.SelectedVisibilityScope, viewForDublicate: view);

                                if(sheetHelper.Sheet != null 
                                    && viewHelper.View != null 
                                    && Viewport.CanAddViewToSheet(_revitRepository.Document, sheetHelper.Sheet.Id, viewHelper.View.Id)) {

                                    viewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, SelectedViewportType);
                                }
                            }

                            if(WorkWithSpecs) {

                                foreach(SpecHelper specHelper in task.ScheduleSheetInstances) {

                                    if(!specHelper.HasProblemWithLevelDetection) {
                                        Report.AppendLine($"❗               В задании спецификации имеются ошибки, создание спецификации отменено!");
                                        continue;
                                    }

                                    SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(SelectedFilterNameForSpecs, numberOfLevelAsInt);                                  

                                    // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование
                                    // В случае если лист и размещаемая на нем спека не null и на листе еще нет вид.экрана этой спеки
                                    if(sheetHelper.Sheet != null 
                                        && newSpecHelper.Specification != null
                                        && !sheetHelper.HasSpecWithName(newSpecHelper.Specification.Name)) {

                                        ScheduleSheetInstance newScheduleSheetInstance = newSpecHelper.PlaceSpec(sheetHelper);
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


                            SheetHelper sheetHelper = null;
                            if(WorkWithSheets) {

                                string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                    SheetNamePrefix,
                                    task.NumberOfBuildingPartAsInt,
                                    task.NumberOfBuildingSectionAsInt,
                                    numberOfLevel);

                                sheetHelper = new SheetHelper(_revitRepository, Report);
                                sheetHelper.GetOrCreateSheet(newSheetName, SelectedTitleBlock);
                            }

                            ViewHelper viewHelper = null;
                            if(WorkWithViews) {

                                string newViewName = string.Format("{0}{1} этаж К{2}{3}",
                                    ViewNamePrefix,
                                    numberOfLevel,
                                    task.NumberOfBuildingPartAsInt, 
                                    task.ViewNameSuffix);

                                viewHelper = new ViewHelper(Report, _revitRepository);
                                viewHelper.GetView(newViewName, task.SelectedVisibilityScope, SelectedViewFamilyType, level);

                                if(sheetHelper.Sheet != null 
                                    && viewHelper.View != null 
                                    && Viewport.CanAddViewToSheet(_revitRepository.Document, sheetHelper.Sheet.Id, viewHelper.View.Id)) {

                                    viewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, SelectedViewportType);
                                }
                            }


                            if(WorkWithSpecs) {

                                foreach(SpecHelper specHelper in task.ScheduleSheetInstances) {

                                    if(!specHelper.HasProblemWithLevelDetection) {
                                        Report.AppendLine($"❗               В задании спецификации имеются ошибки, создание спецификации отменено!");
                                        continue;
                                    }

                                    SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(SelectedFilterNameForSpecs, numberOfLevelAsInt);

                                    // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование
                                    // В случае если лист и размещаемая на нем спека не null и на листе еще нет вид.экрана этой спеки
                                    if(sheetHelper.Sheet != null 
                                        && newSpecHelper.Specification != null
                                        && !sheetHelper.HasSpecWithName(newSpecHelper.Specification.Name)) {

                                        newSpecHelper.PlaceSpec(sheetHelper);
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