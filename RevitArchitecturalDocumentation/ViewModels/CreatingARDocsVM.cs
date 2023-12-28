using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;
using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels.Components;
using RevitArchitecturalDocumentation.Views;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class CreatingARDocsVM : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private bool _createViewsFromSelected = false;
        private string _errorText;
        private CreatingARDocsV _pCOnASPDocsView;
        private ObservableCollection<TreeReportNode> _report = new ObservableCollection<TreeReportNode>();
        private TaskInfo _selectedTask;
        private List<Element> _visibilityScopes;
        private List<Level> _levels;
        private ObservableCollection<ViewPlan> _selectedViews = new ObservableCollection<ViewPlan>();
        private ObservableCollection<ViewHelper> _selectedViewHelpers = new ObservableCollection<ViewHelper>();
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();

        private Regex _regexForBuildingPart = new Regex(@"К(.*?)_");
        private Regex _regexForBuildingSection = new Regex(@"С(.*?)$");
        private Regex _regexForBuildingSectionPart = new Regex(@"часть (.*?)$");
        private Regex _regexForLevel = new Regex(@"^(.*?) ");
        private Regex _regexForView = new Regex(@"_(.*?) этаж");


        public CreatingARDocsVM(PluginConfig pluginConfig, RevitRepository revitRepository,
            SheetOptions sheetOptions, ViewOptions viewOptions, SpecOptions specOptions) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;

            SheetOptsVM = new SheetOptionsVM(pluginConfig, revitRepository, sheetOptions);
            ViewOptsVM = new ViewOptionsVM(pluginConfig, revitRepository, viewOptions);
            SpecOptsVM = new SpecOptionsVM(pluginConfig, revitRepository, specOptions);



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


        public SheetOptionsVM SheetOptsVM { get; }
        public ViewOptionsVM ViewOptsVM { get; }
        public SpecOptionsVM SpecOptsVM { get; }



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


        public bool CreateViewsFromSelected {
            get => _createViewsFromSelected;
            set => this.RaiseAndSetIfChanged(ref _createViewsFromSelected, value);
        }

        public CreatingARDocsV PCOnASPDocsView {
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

            PCOnASPDocsView = obj as CreatingARDocsV;
            LoadConfig();

            VisibilityScopes = _revitRepository.VisibilityScopes;
            Levels = _revitRepository.Levels;


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

            if(SelectedViews.Count == 0 && CreateViewsFromSelected) {
                ErrorText = "Не выбрано видов, на основе которых создавать документацию";
                return false;
            }

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

            if(SheetOptsVM.WorkWithSheets && SheetOptsVM.SelectedTitleBlock is null) {
                ErrorText = "Не выбран тип рамки листа";
                return false;
            }

            if(ViewOptsVM.WorkWithViews && ViewOptsVM.SelectedViewFamilyType is null) {
                ErrorText = "Не выбран тип вида";
                return false;
            }

            if(ViewOptsVM.WorkWithViews && ViewOptsVM.SelectedViewportType is null) {
                ErrorText = "Не выбран тип видового экрана";
                return false;
            }

            if(SpecOptsVM.WorkWithSpecs && SpecOptsVM.FilterNamesFromSpecs.Count > 0 && SpecOptsVM.SelectedFilterNameForSpecs is null) {
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

            SheetOptsVM.LoadConfig();
            ViewOptsVM.LoadConfig();
            SpecOptsVM.LoadConfig();

            CreateViewsFromSelected = settings.CreateViewsFromSelected;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);

            SheetOptsVM.SaveConfig();
            ViewOptsVM.SaveConfig();
            SpecOptsVM.SaveConfig();

            settings.CreateViewsFromSelected = CreateViewsFromSelected;

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

            SpecOptsVM.FilterNamesFromSpecs.Clear();
            foreach(TaskInfo task in TasksForWork) {

                foreach(SpecHelper spec in task.ListSpecHelpers) {
                    if(SpecOptsVM.FilterNamesFromSpecs.Count == 0) {
                        SpecOptsVM.FilterNamesFromSpecs.AddRange(spec.GetFilterNames());
                    } else {
                        SpecOptsVM.FilterNamesFromSpecs = SpecOptsVM.FilterNamesFromSpecs.Intersect(spec.GetFilterNames()).ToList();
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
            rep.AddNodeWithName($"Выбран тип вида: {ViewOptsVM.SelectedViewFamilyTypeName}");
            rep.AddNodeWithName($"Выбран тип видового экрана: {ViewOptsVM.SelectedViewportTypeName}");
            rep.AddNodeWithName($"Выбран тип рамки листа: {SheetOptsVM.SelectedTitleBlockName}");
            rep.AddNodeWithName($"Выбрано поле параметра фильтрации спецификации: {SpecOptsVM.SelectedFilterNameForSpecs}");

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
                // Ищем указанные подстроки по всем узлам сверху вниз
                // Если находим у какого то дочернего элемента указанную подстроку,
                // то прописываем ее всем узлам снизу вверху (начиная с родителя узла, в котором ее нашли)
                item.RewriteByChildNamesRecursively("❗ ");
                item.RewriteByChildNamesRecursively("  ~  ");

                // В случае, когда заданий несколько возможна ситуация, когда один узел не важный, а другой важный, т.к. содержит информацию о создании чего-либо
                // В этом случае в узле самого верхнего уровня нужно удалить соответствующую строку, чтобы его не скрывала фильтрация
                item.RewriteByChildNames("Задание", "  ~  ");
            }

            TreeReportV window = new TreeReportV {
                // Передаем VM и указываем, что фильтрацию для сепарации на важные/не важные узлы будем выполнять через "  ~  "
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

                DocsFromSelectedViewsVM docsFromSelectedViewsVM = new DocsFromSelectedViewsVM(this, _revitRepository, TreeReport,
                    SheetOptsVM.GetSheetOption(), ViewOptsVM.GetViewOption(), SpecOptsVM.GetSpecOption());
                docsFromSelectedViewsVM.CreateDocs();
            } else {

                DocsFromScratchVM docsFromScratchVM = new DocsFromScratchVM(this, _revitRepository, TreeReport,
                    SheetOptsVM.GetSheetOption(), ViewOptsVM.GetViewOption(), SpecOptsVM.GetSpecOption());
                docsFromScratchVM.CreateDocs();
            }

            OpenReportWindow();
        }
    }
}
