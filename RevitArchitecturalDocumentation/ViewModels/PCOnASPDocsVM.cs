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

using Autodesk.AdvanceSteel.StructuralAnalysis;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using Microsoft.WindowsAPICodePack.ShellExtensions;

using Ninject.Planning.Targets;

using pyRevitLabs.Json.Linq;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;
using RevitArchitecturalDocumentation.Views;

using static System.Net.Mime.MediaTypeNames;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class PCOnASPDocsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private bool _workWithSheets = true;
        private bool _workWithViews = true;
        private bool _workWithSpecs = true;
        private bool _createViewsFromSelected = false;
        private string _selectedViewFamilyTypeName;
        private string _selectedViewportTypeName;
        private string _selectedTitleBlockName;
        private string _viewNamePrefix = string.Empty;
        private string _selectedFilterNameForSpecs;
        private string _sheetNamePrefix = string.Empty;
        private string _errorText;
        private PCOnASPDocsV _pCOnASPDocsView;
        private ObservableCollection<TreeReportNode> _report = new ObservableCollection<TreeReportNode>();
        private ViewFamilyType _selectedViewFamilyType;
        private ElementType _selectedViewportType;
        private FamilySymbol _selectedTitleBlock;
        private TaskInfo _selectedTask;
        private List<Element> _visibilityScopes;
        private List<Level> _levels;
        private List<ViewFamilyType> _viewFamilyTypes;
        private List<ElementType> _viewportTypes;
        private List<FamilySymbol> _titleBlocksInProject;
        private List<string> _filterNamesFromSpecs = new List<string>();
        private ObservableCollection<ViewPlan> _selectedViews = new ObservableCollection<ViewPlan>();
        private ObservableCollection<ViewHelper> _selectedViewHelpers = new ObservableCollection<ViewHelper>();
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();

        //private StringBuilder _report = new StringBuilder();
        private Regex _regexForBuildingPart = new Regex(@"К(.*?)_");
        private Regex _regexForBuildingSection = new Regex(@"С(.*?)$");
        private Regex _regexForBuildingSectionPart = new Regex(@"часть (.*?)$");
        private Regex _regexForLevel = new Regex(@"^(.*?) ");
        private Regex _regexForView = new Regex(@"_(.*?) этаж");


        public PCOnASPDocsVM(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            LoadViewCommand = new RelayCommand(LoadView);
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



        //public StringBuilder Report {
        //    get => _report;
        //    set => this.RaiseAndSetIfChanged(ref _report, value);
        //}
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

        public ObservableCollection<ViewHelper> SelectedViewHelpers {
            get => _selectedViewHelpers;
            set => this.RaiseAndSetIfChanged(ref _selectedViewHelpers, value);
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

        public PCOnASPDocsV PCOnASPDocsView {
            get => _pCOnASPDocsView;
            set => this.RaiseAndSetIfChanged(ref _pCOnASPDocsView, value);
        }

        public ObservableCollection<TreeReportNode> TreeReport {
            get => _report;
            set => this.RaiseAndSetIfChanged(ref _report, value);
        }

        public string ErrorText {
            get => _errorText;
            set => this.RaiseAndSetIfChanged(ref _errorText, value);
        }


        /// <summary>
        /// Метод, отрабатывающий при загрузке окна
        /// </summary>
        private void LoadView(object obj) {

            PCOnASPDocsView = obj as PCOnASPDocsV;
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
                TasksForWork.Add(new TaskInfo(RegexForBuildingPart, RegexForBuildingSection, 1));
            }

            if(SelectedViews.Count == 0) {
                SelectedViews = _revitRepository.GetSelectedViewPlans();
                SelectedViewHelpers = new ObservableCollection<ViewHelper>();
                foreach(ViewPlan viewPlan in SelectedViews) {
                    ViewHelper viewHelper = new ViewHelper(viewPlan);
                    SelectedViewHelpers.Add(viewHelper);
                    try {
                        viewHelper.NameHelper.AnilizeNGetLevelNumber();
                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                    }
                }
            }
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

            foreach(ViewHelper viewHelper in SelectedViewHelpers) {
                try {
                    viewHelper.NameHelper.AnilizeNGetLevelNumber();

                } catch(ViewNameException ex) {
                    ErrorText = ex.Message;
                    return false;
                }
            }

            foreach(TaskInfo task in TasksForWork) {
                // Проверяем и получаем данные по каждому заданию
                try {
                    task.СheckTasksForErrors();

                } catch(TaskException ex) {
                    ErrorText = ex.Message;
                    return false;
                }
                // Отдельно проверяем и получаем данные по спецификациям в задании
                foreach(SpecHelper specHelper in task.ListSpecHelpers) {

                    // LevelNumberFormat заполняется после последней проверки при получении имени, поэтому, если он заполнен, значит все ок
                    if(specHelper.NameHelper.LevelNumberFormat.Length == 0) {
                        try {
                            // Анализируем и получаем номер одновременно, т.к. чтобы проанализировать номер уровня
                            // нужно получить другую информацию, что по факту равно загрузке при получении уровня
                            specHelper.NameHelper.AnilizeNGetLevelNumber();

                        } catch(ViewNameException ex) {
                            ErrorText = ex.Message;
                            return false;
                        }
                    }
                }
            }

            if(SelectedViews.Count == 0 && CreateViewsFromSelected) {
                ErrorText = "Не выбрано видов, на основе которых создавать документацию";
                return false;
            }

            if(WorkWithSheets && SelectedTitleBlock is null) {
                ErrorText = "Не выбран тип рамки листа";
                return false;
            }

            if(WorkWithViews && SelectedViewFamilyType is null) {
                ErrorText = "Не выбран тип вида";
                return false;
            }

            if(WorkWithViews && SelectedViewportType is null) {
                ErrorText = "Не выбран тип видового экрана";
                return false;
            }

            if(WorkWithSpecs && FilterNamesFromSpecs.Count > 0 && SelectedFilterNameForSpecs is null) {
                ErrorText = "Не выбрано имя поля фильтра спецификации";
                return false;
            }

            ErrorText = "";
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


        /// <summary>
        /// Добавляет задачу в список. 
        /// Задача содержит информацию о начальном и конечном уровне, с которыми нужно работать; выбранную область видимости и спеки
        /// </summary>
        private void AddTask() {

            TasksForWork.Add(new TaskInfo(RegexForBuildingPart, RegexForBuildingSection, TasksForWork.Count + 1));
        }

        /// <summary>
        /// Удаляет выбранную в интерфейсе задачу из списка. 
        /// </summary>
        private void DeleteTask() {

            if(TasksForWork.Count > 0) {
                TasksForWork.RemoveAt(TasksForWork.Count - 1);
            }
            GetFilterNames();
        }


        /// <summary>
        /// После скрытия окна позволяет выбрать видовые экраны спек в Revit
        /// </summary>
        private void SelectSpecs(object obj) {

            PCOnASPDocsView.Hide();

            TaskInfo task = obj as TaskInfo;
            if(task != null) {
                task.ListSpecHelpers.Clear();

                ISelectionFilter selectFilter = new ScheduleSelectionFilter();
                IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");

                foreach(Reference reference in references) {

                    ScheduleSheetInstance scheduleSheetInstance = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                    if(scheduleSheetInstance is null) {
                        continue;
                    }

                    //SpecHelper specHelper = new SpecHelper(_revitRepository, scheduleSheetInstance, TreeReport);
                    SpecHelper specHelper = new SpecHelper(_revitRepository, scheduleSheetInstance);
                    task.ListSpecHelpers.Add(specHelper);
                    try {
                        specHelper.NameHelper.AnilizeNGetNameInfo();

                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                    }

                }
                GetFilterNames();
            }
            PCOnASPDocsView.ShowDialog();
        }


        /// <summary>
        /// Метод перебирает все выбранные спеки во всех заданиях и собирает список параметров фильтрации. принадлежащий всем одновременно
        /// </summary>
        private void GetFilterNames() {

            FilterNamesFromSpecs.Clear();
            foreach(TaskInfo task in TasksForWork) {

                foreach(SpecHelper spec in task.ListSpecHelpers) {
                    if(FilterNamesFromSpecs.Count == 0) {
                        FilterNamesFromSpecs.AddRange(spec.GetFilterNames());
                    } else {
                        FilterNamesFromSpecs = FilterNamesFromSpecs.Intersect(spec.GetFilterNames()).ToList();
                    }
                }
            }
        }


        /// <summary>
        /// Подготавливает и передает информацию в древовидный отчет об исходных данных заданий
        /// </summary>
        private TreeReportNode GetInitialDataForReport() {

            TreeReportNode rep = new TreeReportNode(null) { Name = "Исходные данные:" };

            if(CreateViewsFromSelected) {
                rep.AddNodeWithName($"Создание видов будет производиться на основе выбранных видов. Перебираем выбранные виды и поочередно применяем задания:");
            } else {
                rep.AddNodeWithName($"Создание видов будет производиться с нуля. Перебираем уровни проекта и поочередно применяем задания:");
            }
            rep.AddNodeWithName($"Выбрано видов до запуска плагина: {SelectedViews.Count}");
            rep.AddNodeWithName($"Выбран тип вида: {SelectedViewFamilyTypeName}");
            rep.AddNodeWithName($"Выбран тип видового экрана: {SelectedViewportTypeName}");
            rep.AddNodeWithName($"Выбран тип рамки листа: {SelectedTitleBlockName}");
            rep.AddNodeWithName($"Выбрано поле параметра фильтрации спецификации: {SelectedFilterNameForSpecs}");

            foreach(TaskInfo task in TasksForWork) {
                TreeReportNode taskRep = new TreeReportNode(rep) { Name = $"Номер задания: {task.TaskNumber}" };
                taskRep.AddNodeWithName($"Начальный уровень: {task.StartLevelNumberAsInt}");
                taskRep.AddNodeWithName($"Конечный уровень: {task.EndLevelNumberAsInt}");
                taskRep.AddNodeWithName($"Область видимости: {task.SelectedVisibilityScope.Name}");
                taskRep.AddNodeWithName($"Номер корпуса области видимости: {task.NumberOfBuildingPartAsInt}");
                taskRep.AddNodeWithName($"Номер секции области видимости: {task.NumberOfBuildingSectionAsInt}");

                rep.Nodes.Add(taskRep);
            }

            return rep;
        }


        /// <summary>
        /// Метод перебирает все выбранные спеки во всех заданиях и собирает список параметров фильтрации. принадлежащий всем одновременно
        /// </summary>
        private void OpenReportWindow() {

            foreach(TreeReportNode item in TreeReport) {
                item.FindInChildName("❗ ");
                item.FindInChildName("  ~  ");
            }

            TreeReportV window = new TreeReportV {
                DataContext = new TreeReportVM(TreeReport, "  ~  ")
            };
            window.Show();
        }


        /// <summary>
        /// Основной метод, выполняющий работу по созданию листов, видов, спек и выносу спек и видов на листы
        /// </summary>
        private void DoWork() {

            TreeReport.Add(GetInitialDataForReport());

            if(CreateViewsFromSelected) {

                DocsFromSelectedViewsVM docsFromSelectedViewsVM = new DocsFromSelectedViewsVM(this, _revitRepository, TreeReport);
                docsFromSelectedViewsVM.CreateDocs();
            } else {

                DocsFromScratchVM docsFromScratchVM = new DocsFromScratchVM(this, _revitRepository, TreeReport);
                docsFromScratchVM.CreateDocs();
            }

            OpenReportWindow();
        }
    }
}