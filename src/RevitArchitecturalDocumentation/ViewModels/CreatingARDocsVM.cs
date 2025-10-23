using System.Collections.ObjectModel;
using System.Windows.Input;

using Autodesk.Revit.DB;

using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Models.Exceptions;
using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.Models.Services;
using RevitArchitecturalDocumentation.ViewModels.Components;
using RevitArchitecturalDocumentation.Views;

namespace RevitArchitecturalDocumentation.ViewModels;
internal class CreatingARDocsVM : BaseViewModel {
    private readonly PluginConfig _pluginConfig;
    private readonly RevitRepository _revitRepository;
    private readonly IWindowService _windowService;

    private bool _createViewsFromSelected = false;
    private string _errorText;
    private CreatingARDocsV _pCOnASPDocsView;
    private ObservableCollection<TreeReportNode> _report = [];
    private ObservableCollection<ViewPlan> _selectedViews = [];
    private ObservableCollection<ViewHelper> _selectedViewHelpers = [];


    public CreatingARDocsVM(PluginConfig pluginConfig, RevitRepository revitRepository, MainOptions mainOptions, 
                            IWindowService windowService) {
        _pluginConfig = pluginConfig;
        _revitRepository = revitRepository;
        _windowService = windowService;

        TaskInformationVM = new TaskInfoListVM(pluginConfig, revitRepository, this);
        SheetOptsVM = new SheetOptionsVM(pluginConfig, revitRepository, mainOptions.SheetOpts);
        ViewOptsVM = new ViewOptionsVM(pluginConfig, revitRepository, mainOptions.ViewOpts);
        SpecOptsVM = new SpecOptionsVM(pluginConfig, revitRepository, mainOptions.SpecOpts);


        LoadViewCommand = RelayCommand.Create<CreatingARDocsV>(LoadView);
        AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);
    }

    public ICommand LoadViewCommand { get; }
    public ICommand AcceptViewCommand { get; }

    public TaskInfoListVM TaskInformationVM { get; }
    public SheetOptionsVM SheetOptsVM { get; }
    public ViewOptionsVM ViewOptsVM { get; }
    public SpecOptionsVM SpecOptsVM { get; }


    public ObservableCollection<ViewPlan> SelectedViews {
        get => _selectedViews;
        set => RaiseAndSetIfChanged(ref _selectedViews, value);
    }

    public ObservableCollection<ViewHelper> SelectedViewHelpers {
        get => _selectedViewHelpers;
        set => RaiseAndSetIfChanged(ref _selectedViewHelpers, value);
    }

    public bool CreateViewsFromSelected {
        get => _createViewsFromSelected;
        set => RaiseAndSetIfChanged(ref _createViewsFromSelected, value);
    }

    public CreatingARDocsV PCOnASPDocsView {
        get => _pCOnASPDocsView;
        set => RaiseAndSetIfChanged(ref _pCOnASPDocsView, value);
    }

    public ObservableCollection<TreeReportNode> TreeReport {
        get => _report;
        set => RaiseAndSetIfChanged(ref _report, value);
    }

    public string ErrorText {
        get => _errorText;
        set => RaiseAndSetIfChanged(ref _errorText, value);
    }


    /// <summary>
    /// Метод, отрабатывающий при загрузке окна
    /// </summary>
    private void LoadView(CreatingARDocsV creatingARDocsV) {
        PCOnASPDocsView = creatingARDocsV;
        LoadConfig();

        if(SelectedViews.Count == 0) {
            SelectedViews = _revitRepository.GetSelectedViewPlans();
            SelectedViewHelpers = [];
            foreach(var viewPlan in SelectedViews) {
                var viewHelper = new ViewHelper(viewPlan);
                SelectedViewHelpers.Add(viewHelper);
                try {
                    viewHelper.NameHelper.AnalyzeNGetLevelNumber();
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

        foreach(var viewHelper in SelectedViewHelpers) {
            try {
                viewHelper.NameHelper.AnalyzeNGetLevelNumber();

            } catch(ViewNameException ex) {
                ErrorText = ex.Message;
                return false;
            }
        }

        foreach(var task in TaskInformationVM.TasksForWork) {
            // Проверяем и получаем данные по каждому заданию
            try {
                task.CheckTasksForErrors();

            } catch(TaskException ex) {
                ErrorText = ex.Message;
                return false;
            }
            // Отдельно проверяем и получаем данные по спецификациям в задании
            foreach(var specHelper in task.ListSpecHelpers) {
                // LevelNumberFormat заполняется после последней проверки при получении имени, поэтому, если он заполнен, значит все ок
                if(specHelper.NameHelper.LevelNumberFormat.Length == 0) {
                    try {
                        // Анализируем и получаем номер одновременно, т.к. чтобы проанализировать номер уровня
                        // нужно получить другую информацию, что по факту равно загрузке при получении уровня
                        specHelper.NameHelper.AnalyzeNGetLevelNumber();
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

        CreateViewsFromSelected = settings.CreateViewsFromSelected;
    }


    /// <summary>
    /// Сохраняет параметры плагина для следующего запуска
    /// </summary>
    private void SaveConfig() {

        var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                      ?? _pluginConfig.AddSettings(_revitRepository.Document);

        settings.CreateViewsFromSelected = CreateViewsFromSelected;

        _pluginConfig.SaveProjectConfig();
    }


    /// <summary>
    /// Подготавливает и передает информацию в древовидный отчет об исходных данных заданий
    /// </summary>
    private TreeReportNode GetInitialDataForReport() {

        var rep = new TreeReportNode(null) { Name = "Исходные данные:" };

        if(CreateViewsFromSelected) {
            rep.AddNodeWithName($"Создание видов будет производиться на основе выбранных видов. Перебираем выбранные виды и поочередно применяем задания:");
        } else {
            rep.AddNodeWithName($"Создание видов будет производиться с нуля. Перебираем уровни проекта и поочередно применяем задания:");
        }
        rep.AddNodeWithName($"Выбрано видов до запуска плагина: {SelectedViews.Count}");
        rep.AddNodeWithName($"Выбран тип вида: {ViewOptsVM.SelectedViewFamilyType.Name}");
        rep.AddNodeWithName($"Выбран тип видового экрана: {ViewOptsVM.SelectedViewportType.Name}");
        rep.AddNodeWithName($"Выбран тип рамки листа: {SheetOptsVM.SelectedTitleBlock.Name}");
        rep.AddNodeWithName($"Выбрано поле параметра фильтрации спецификации: {SpecOptsVM.SelectedFilterNameForSpecs}");

        foreach(var task in TaskInformationVM.TasksForWork) {
            var taskRep = new TreeReportNode(rep) { Name = $"Номер задания: {task.TaskNumber}" };
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

        foreach(var item in TreeReport) {
            // Ищем указанные подстроки по всем узлам сверху вниз
            // Если находим у какого то дочернего элемента указанную подстроку,
            // то прописываем ее всем узлам снизу вверху (начиная с родителя узла, в котором ее нашли)
            item.RewriteByChildNamesRecursively("❗ ");
            item.RewriteByChildNamesRecursively("  ~  ");

            // В случае, когда заданий несколько возможна ситуация, когда один узел не важный, а другой важный, т.к. содержит информацию о создании чего-либо
            // В этом случае в узле самого верхнего уровня нужно удалить соответствующую строку, чтобы его не скрывала фильтрация
            item.RewriteByChildNames("Задание", "  ~  ");
        }
        var reportViewModel = new TreeReportVM(TreeReport, "  ~  ");
        _windowService.Show<TreeReportV, TreeReportVM>(reportViewModel);
    }


    /// <summary>
    /// Основной метод, выполняющий работу по созданию листов, видов, спек и выносу спек и видов на листы
    /// </summary>
    private void DoWork() {
        TreeReport.Add(GetInitialDataForReport());
        var mainOptions = new MainOptions(SheetOptsVM.GetSheetOption(), 
                                          ViewOptsVM.GetViewOption(), 
                                          SpecOptsVM.GetSpecOption());
        if(CreateViewsFromSelected) {
            var docsFromSelectedViewsVM = new DocsFromSelectedViews(this, _revitRepository, TreeReport,
                                                                    TaskInformationVM.TasksForWork, mainOptions);
            docsFromSelectedViewsVM.CreateDocs();
        } else {
            var docsFromScratchVM = new DocsFromScratch(this, _revitRepository, TreeReport,
                                                        TaskInformationVM.TasksForWork, mainOptions);
            docsFromScratchVM.CreateDocs();
        }
        OpenReportWindow();
    }
}
