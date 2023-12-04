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

        private bool _workWithSheets = true;
        private bool _workWithViews = true;
        private bool _workWithSpecs = true;
        private bool _createViewsFromSelected = false;
        private string _selectedViewFamilyTypeName;
        private string _selectedViewportTypeName;
        private string _selectedTitleBlockName;
        private string _viewNamePrefix = string.Empty;
        private string _selectedFilterNameForSpecs = string.Empty;
        private string _sheetNamePrefix = string.Empty;
        private string _errorText;
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
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();

        private StringBuilder _report = new StringBuilder();
        private Regex _regexForBuildingPart = new Regex(@"К(.*?)_");
        private Regex _regexForBuildingSection = new Regex(@"С(.*?)$");
        private Regex _regexForBuildingSectionPart = new Regex(@"часть (.*?)$");
        private Regex _regexForLevel = new Regex(@"^(.*?) ");
        private Regex _regexForView = new Regex(@"_(.*?) этаж");


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

            if(SelectedViews.Count == 0) {
                SelectedViews = _revitRepository.GetSelectedViewPlans();
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

            foreach(TaskInfo task in TasksForWork) {
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

            TasksForWork.Add(new TaskInfo(RegexForBuildingPart, RegexForBuildingSection, Report));
        }

        /// <summary>
        /// Удаляет выбранную в интерфейсе задачу из списка. 
        /// </summary>
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
                task.ListSpecHelpers.Clear();

                ISelectionFilter selectFilter = new ScheduleSelectionFilter();
                IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                                .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");

                foreach(Reference reference in references) {

                    ScheduleSheetInstance scheduleSheetInstance = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                    if(scheduleSheetInstance is null) {
                        continue;
                    }

                    SpecHelper specHelper = new SpecHelper(_revitRepository, scheduleSheetInstance, Report);
                    task.ListSpecHelpers.Add(specHelper);
                    try {
                        specHelper.NameHelper.AnilizeNGetNameInfo();

                    } catch(ViewNameException ex) {
                        ErrorText = ex.Message;
                    }

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
        /// Основной метод, выполняющий работу по созданию листов, видов, спек и выносу спек и видов на листы
        /// </summary>
        private void DoWork() {

            if(CreateViewsFromSelected) {

                DocsFromSelectedViewsVM docsFromSelectedViewsVM = new DocsFromSelectedViewsVM(this, _revitRepository, Report);
                docsFromSelectedViewsVM.CreateDocs();
            } else {

                DocsFromScratchVM docsFromScratchVM = new DocsFromScratchVM(this, _revitRepository, Report);
                docsFromScratchVM.CreateDocs();
            }
        }
    }
}