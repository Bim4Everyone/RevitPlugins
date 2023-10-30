using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Media3D;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;
using RevitArchitecturalDocumentation.Views;

using static System.Net.Mime.MediaTypeNames;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class MainViewModel : BaseViewModel {
        private readonly PluginConfig _pluginConfig;
        private readonly RevitRepository _revitRepository;

        private List<Element> _visibilityScopes;
        private List<Level> _levels;
        private List<ViewFamilyType> _viewFamilyTypes;
        private List<FamilySymbol> _titleBlocksInProject;
        private ViewFamilyType _selectedViewFamilyType;
        private FamilySymbol _selectedTitleBlock;
        private ObservableCollection<TaskInfo> _tasksForWork = new ObservableCollection<TaskInfo>();
        private TaskInfo _selectedTask;
        private string _viewNamePrefix = string.Empty;
        private string _selectedFilterNameForSpecs = string.Empty;
        private string _sheetNamePrefix = string.Empty;
        private List<SpecHelper> _scheduleSheetInstances = new List<SpecHelper>();
        private List<string> _filterNamesFromSpecs = new List<string>();
        private bool _workWithSpecs = false;

        private string _errorText;

        public MainViewModel(PluginConfig pluginConfig, RevitRepository revitRepository) {
            _pluginConfig = pluginConfig;
            _revitRepository = revitRepository;


            VisibilityScopes = _revitRepository.VisibilityScopes;
            Levels = _revitRepository.Levels;
            ViewFamilyTypes = _revitRepository.ViewFamilyTypes;
            TitleBlocksInProject = _revitRepository.TitleBlocksInProject;

            LoadViewCommand = RelayCommand.Create(LoadView);
            AcceptViewCommand = RelayCommand.Create(AcceptView, CanAcceptView);

            AddTaskCommand = RelayCommand.Create(AddTask);
            DeleteTaskCommand = RelayCommand.Create(DeleteTask);
            SelectSpecCommand = RelayCommand.Create(SelectSpec);


            TestCommand = RelayCommand.Create(Test);
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }
        public ICommand SelectSpecCommand { get; }


        public ICommand TestCommand { get; }



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

        public List<FamilySymbol> TitleBlocksInProject {
            get => _titleBlocksInProject;
            set => this.RaiseAndSetIfChanged(ref _titleBlocksInProject, value);
        }

        public FamilySymbol SelectedTitleBlock {
            get => _selectedTitleBlock;
            set => this.RaiseAndSetIfChanged(ref _selectedTitleBlock, value);
        }

        public string ViewNamePrefix {
            get => _viewNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _viewNamePrefix, value);
        }

        public string SheetNamePrefix {
            get => _sheetNamePrefix;
            set => this.RaiseAndSetIfChanged(ref _sheetNamePrefix, value);
        }

        public bool WorkWithSpecs {
            get => _workWithSpecs;
            set => this.RaiseAndSetIfChanged(ref _workWithSpecs, value);
        }
        public List<SpecHelper> ScheduleSheetInstances {
            get => _scheduleSheetInstances;
            set => this.RaiseAndSetIfChanged(ref _scheduleSheetInstances, value);
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

            if(SelectedViewFamilyType is null) {
                return false;
            }

            if(SelectedTitleBlock is null) {
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

            SheetNamePrefix = settings.SheetNamePrefix;
            ViewNamePrefix = settings.ViewNamePrefix;
        }


        /// <summary>
        /// Сохраняет параметры плагина для следующего запуска
        /// </summary>
        private void SaveConfig() {

            var settings = _pluginConfig.GetSettings(_revitRepository.Document)
                          ?? _pluginConfig.AddSettings(_revitRepository.Document);


            settings.SheetNamePrefix = SheetNamePrefix;
            settings.ViewNamePrefix = ViewNamePrefix;

            _pluginConfig.SaveProjectConfig();
        }



        private void AddTask() {

            TasksForWork.Add(new TaskInfo());
        }

        private void DeleteTask() {

            if(SelectedTask != null) {
                TasksForWork.Remove(SelectedTask);
            }
        }


        private void SelectSpec() {

            ScheduleSheetInstances.Clear();

            ISelectionFilter selectFilter = new ScheduleSelectionFilter();
            IList<Reference> references = _revitRepository.ActiveUIDocument.Selection
                            .PickObjects(ObjectType.Element, selectFilter, "Выберите спецификации на листе");
            
            foreach(Reference reference in references) {

                ScheduleSheetInstance elem = _revitRepository.Document.GetElement(reference) as ScheduleSheetInstance;
                if(elem is null) {
                    continue;
                }

                SpecHelper specHelper = new SpecHelper(elem);
                ScheduleSheetInstances.Add(specHelper);
                specHelper.GetInfo();
                FilterNamesFromSpecs.AddRange(specHelper.GetFilterNames());
            }


            MainWindow mainWindow = new MainWindow();
            mainWindow.DataContext = this;
            mainWindow.ShowDialog();
        }



        private void Test() {


            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор ")) {


                foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                    ViewSchedule elem = _revitRepository.Document.GetElement(id) as ViewSchedule;

                    elem.Duplicate(ViewDuplicateOption.WithDetailing);
                }

                transaction.Commit();
            }
        }



        private void DoWork() {

            StringBuilder report = new StringBuilder();

            // При работе с ДДУ листы пользователь должен выбрать заранее, т.к. селектор API не позволяет выбирать элементы из диспетчера
            List<View> views = new List<View>();
            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                View view = _revitRepository.Document.GetElement(id) as View;
                if(view != null) {
                    views.Add(view);
                }
            }
            report.AppendLine($"Выбрано видов до запуска плагина: {views.Count}");


            var regexForBuildingPart = new Regex(@"К(.*?)_");
            var regexForBuildingSection = new Regex(@"С(.*?)$");
            var regexForBuildingSectionPart = new Regex(@"часть (.*?)$");

            var regexForLevel = new Regex(@"^(.*?) ");

            var regexForView = new Regex(@"псо_(.*?) этаж");

            var regexForSpecs = new Regex(@"_(.*?) этаж");

            report.AppendLine($"Приступаю к выполнению задания. Всего задач: {TasksForWork.Count}");
            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор АР")) {
                int c = 0;
                foreach(TaskInfo task in TasksForWork) {
                    c++;
                    report.AppendLine($"    Задача {c}");

                    int startLevelNumberAsInt;
                    if(!int.TryParse(task.StartLevelNumber, out startLevelNumberAsInt)) {
                        report.AppendLine($"❗   Начальный уровень в задаче {c} некорректен!");
                        continue;
                    }
                    report.AppendLine($"    Начальный уровень: {startLevelNumberAsInt}");


                    int endLevelNumberAsInt;
                    if(!int.TryParse(task.EndLevelNumber, out endLevelNumberAsInt)) {
                        report.AppendLine($"❗   Конечный уровень в задаче: {c}  некорректен!");
                        continue;
                    }
                    report.AppendLine($"    Конечный уровень: {endLevelNumberAsInt}");


                    if(task.SelectedVisibilityScope is null) {
                        report.AppendLine($"❗   Не выбрана область видимости в задаче: {c}");
                        continue;
                    }
                    report.AppendLine($"    Работа с областью видимости: {task.SelectedVisibilityScope.Name}");


                    string numberOfBuildingPart = regexForBuildingPart.Match(task.SelectedVisibilityScope.Name).Groups[1].Value;
                    int numberOfBuildingPartAsInt;
                    if(!int.TryParse(numberOfBuildingPart, out numberOfBuildingPartAsInt)) {
                        report.AppendLine($"❗   Не удалось определить корпус у области видимости: {task.SelectedVisibilityScope.Name}!");
                        continue;
                    }
                    report.AppendLine($"    Номер корпуса: {numberOfBuildingPartAsInt}");


                    string numberOfBuildingSection = regexForBuildingSection.Match(task.SelectedVisibilityScope.Name).Groups[1].Value;
                    int numberOfBuildingSectionAsInt;
                    if(!int.TryParse(numberOfBuildingPart, out numberOfBuildingSectionAsInt)) {
                        report.AppendLine($"❗   Не удалось определить секцию у области видимости: {task.SelectedVisibilityScope.Name}!");

                        continue;
                    }
                    report.AppendLine($"    Номер секции: {numberOfBuildingSectionAsInt}");


                    string temp = string.Empty;

                    if(views.Count == 0) {


                        string strForLevelSearch = "К" + numberOfBuildingPart + "_";
                        report.AppendLine($"    Уровни, содержащие: \"{strForLevelSearch}\":");

                        foreach(Level level in Levels) {

                            if(level.Name.Contains(strForLevelSearch)) {

                                report.AppendLine($"        Уровень \"{level.Name}\"");

                                string numberOfLevel = regexForLevel.Match(level.Name).Groups[1].Value;
                                int numberOfLevelAsInt;
                                if(!int.TryParse(numberOfLevel, out numberOfLevelAsInt)) {
                                    report.AppendLine($"❗       Не удалось определить номер уровня {level.Name}!");
                                    continue;
                                }
                                report.AppendLine($"        Номер уровня: {numberOfLevelAsInt}");

                                if(numberOfLevelAsInt < startLevelNumberAsInt || numberOfLevelAsInt > endLevelNumberAsInt) {

                                    continue;
                                }
                                report.AppendLine($"        Уровень: {level.Name} подходит под диапазон {startLevelNumberAsInt} - {endLevelNumberAsInt}:");

                                // ДДУ_4 этаж К3
                                // СК24_дом 37.2_корпус 37.2.3_секция 3_этаж 4
                                try {
                                    ViewPlan newViewPlan;
                                    try {
                                        newViewPlan = ViewPlan.Create(_revitRepository.Document, SelectedViewFamilyType.Id, level.Id);
                                        report.AppendLine($"            Вид успешно создан!");
                                        newViewPlan.Name = string.Format("{0}{1} этаж К{2}", ViewNamePrefix, numberOfLevel, numberOfBuildingPart);
                                        report.AppendLine($"            Задано имя: {newViewPlan.Name}");
                                        newViewPlan.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(task.SelectedVisibilityScope.Id);
                                        report.AppendLine($"            Задана область видимости: {task.SelectedVisibilityScope.Name}");
                                    } catch(Exception) {
                                        report.AppendLine($"❗           Произошла ошибка при работе с видом!");
                                        continue;
                                    }

                                    ViewSheet newSheet;
                                    try {
                                        newSheet = ViewSheet.Create(_revitRepository.Document, SelectedTitleBlock.Id);
                                        report.AppendLine($"            Лист успешно создан!");
                                        newSheet.Name = string.Format("{0}корпус {1}_секция {2}_этаж {3}", SheetNamePrefix, numberOfBuildingPart, numberOfBuildingSection, numberOfLevel);
                                        report.AppendLine($"            Задано имя: {newSheet.Name}");

                                        _revitRepository.Document.Regenerate();
                                    } catch(Exception) {
                                        report.AppendLine($"❗           Произошла ошибка при создании вида!");
                                        continue;
                                    }
                                    

                                    if(Viewport.CanAddViewToSheet(_revitRepository.Document, newSheet.Id, newViewPlan.Id)) {


                                        // Размещаем план на листе
                                        Viewport viewPort = Viewport.Create(_revitRepository.Document, newSheet.Id, newViewPlan.Id, new XYZ(0, 0, 0));

                                        XYZ viewportCenter = viewPort.GetBoxCenter();
                                        Outline viewportOutline = viewPort.GetBoxOutline();
                                        double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
                                        double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;

                                        //if(viewportHalfWidth < viewportHalfHeight) {
                                        //    viewPort.get_Parameter(BuiltInParameter.VIEWPORT_ATTR_ORIENTATION_ON_SHEET).Set(1);

                                        //    viewPort.SetBoxCenter(new XYZ());
                                        //    viewportOutline = viewPort.GetBoxOutline();
                                        //    viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
                                        //    viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;
                                        //}


                                        // Ищем рамку листа
                                        FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, newSheet.Id)
                                            .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                            .WhereElementIsNotElementType()
                                            .FirstOrDefault() as FamilyInstance;

                                        if(titleBlock is null) { continue; }

                                        //titleBlock.LookupParameter("Ширина").Set(150 / 304.8);
                                        //titleBlock.LookupParameter("Высота").Set(viewportHalfHeight * 2 + 10 / 304.8);

                                        _revitRepository.Document.Regenerate();

                                        // Получение габаритов рамки листа
                                        BoundingBoxXYZ boundingBoxXYZ = titleBlock.get_BoundingBox(newSheet);
                                        double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
                                        double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

                                        double titleBlockMinY = boundingBoxXYZ.Min.Y;
                                        double titleBlockMinX = boundingBoxXYZ.Min.X;

                                        XYZ correctPosition = new XYZ(
                                            titleBlockMinX + viewportHalfWidth,
                                            titleBlockHeight / 2 + titleBlockMinY,
                                            0);


                                        viewPort.SetBoxCenter(correctPosition);
                                    }

                                    // надо позже сделать перечисление не через спеку на виде, а через оболочку, чтоб сразу хранить и спеку на виде,
                                    // и спеку, и префикс уровня
                                    if(WorkWithSpecs) {

                                        foreach(SpecHelper specHelper in ScheduleSheetInstances) {

                                            if(!specHelper.CanWorkWithIt) {
                                                //TaskDialog.Show("Report", "Возникли проблемы с " + specHelper.SpecSheetInstance.Name);
                                                return;
                                            }

                                            ViewSchedule newViewSchedule = _revitRepository.Document.GetElement(specHelper.Specification.Duplicate(ViewDuplicateOption.Duplicate)) as ViewSchedule;
                                            if(newViewSchedule is null) {
                                                continue;
                                            }

                                            //TaskDialog.Show("LevelNumber", specHelper.LevelNumber.ToString());
                                            //TaskDialog.Show("FirstPartOfSpecName", specHelper.FirstPartOfSpecName.ToString());
                                            //TaskDialog.Show("FormatOfLevelNumber", specHelper.FormatOfLevelNumber);
                                            //TaskDialog.Show("FormatOfLevelNumber", String.Format(specHelper.FormatOfLevelNumber, numberOfLevelAsInt));
                                            //TaskDialog.Show("LastPartOfSpecName", specHelper.SuffixOfLevelNumber.ToString());
                                            //TaskDialog.Show("LastPartOfSpecName", specHelper.LastPartOfSpecName.ToString());

                                            ScheduleSheetInstance newScheduleSheetInstance = ScheduleSheetInstance.Create(
                                                _revitRepository.Document,
                                                newSheet.Id,
                                                newViewSchedule.Id,
                                                specHelper.SpecSheetInstancePoint);

                                            newViewSchedule.Name = specHelper.FirstPartOfSpecName
                                                                    + String.Format(specHelper.FormatOfLevelNumber, numberOfLevelAsInt)
                                                                    + specHelper.SuffixOfLevelNumber
                                                                    + specHelper.LastPartOfSpecName;



                                            SpecHelper newSpec = new SpecHelper(newViewSchedule);

                                            newSpec.ChangeSpecFilters(SelectedFilterNameForSpecs, numberOfLevelAsInt);
                                        }
                                    }

                                } catch(DivideByZeroException) {
                                    temp += "Возникла проблема с " + task.SelectedVisibilityScope.Name + Environment.NewLine;
                                }
                            }
                        }
                    } else {

                        foreach(View view in views) {

                            string numberOfLevel = regexForView.Match(view.Name.ToLower()).Groups[1].Value;

                            int numberOfLevelAsInt;
                            if(!int.TryParse(numberOfLevel, out numberOfLevelAsInt)) {
                                temp += "Не удалось определить номер уровня у вида " + view.Name + Environment.NewLine;
                                continue;
                            }

                            if(numberOfLevelAsInt < startLevelNumberAsInt || numberOfLevelAsInt > endLevelNumberAsInt) {

                                continue;
                            }

                            string viewNamePartWithSectionPart = string.Empty;

                            if(view.Name.ToLower().Contains("_часть ")) {
                                viewNamePartWithSectionPart = "_часть ";
                                viewNamePartWithSectionPart += regexForBuildingSectionPart.Match(view.Name.ToLower()).Groups[1].Value;
                            }

                            try {
                                ElementId newViewPlanId = view.Duplicate(ViewDuplicateOption.WithDetailing);
                                ViewPlan newViewPlan = view.Document.GetElement(newViewPlanId) as ViewPlan;
                                
                                if(newViewPlan is null) {
                                    temp += "Возникла проблема при создании с " + task.SelectedVisibilityScope.Name + Environment.NewLine;
                                    continue;
                                }

                                newViewPlan.Name = string.Format("{0}{1} этаж К{2}_С{3}{4}", 
                                    ViewNamePrefix, numberOfLevel, numberOfBuildingPart, numberOfBuildingSection, viewNamePartWithSectionPart);
                                newViewPlan.get_Parameter(BuiltInParameter.VIEWER_VOLUME_OF_INTEREST_CROP).Set(task.SelectedVisibilityScope.Id);


                                ViewSheet newSheet = ViewSheet.Create(_revitRepository.Document, SelectedTitleBlock.Id);
                                newSheet.Name = string.Format("{0}корпус {1}_секция {2}_этаж {3}", SheetNamePrefix, numberOfBuildingPart, numberOfBuildingSection, numberOfLevel);

                                _revitRepository.Document.Regenerate();

                                if(Viewport.CanAddViewToSheet(_revitRepository.Document, newSheet.Id, newViewPlan.Id)) {

                                    // Размещаем план на листе
                                    Viewport viewPort = Viewport.Create(_revitRepository.Document, newSheet.Id, newViewPlan.Id, new XYZ(0, 0, 0));

                                    XYZ viewportCenter = viewPort.GetBoxCenter();
                                    Outline viewportOutline = viewPort.GetBoxOutline();
                                    double viewportHalfWidth = viewportOutline.MaximumPoint.X - viewportCenter.X;
                                    double viewportHalfHeight = viewportOutline.MaximumPoint.Y - viewportCenter.Y;



                                    // Ищем рамку листа
                                    FamilyInstance titleBlock = new FilteredElementCollector(_revitRepository.Document, newSheet.Id)
                                        .OfCategory(BuiltInCategory.OST_TitleBlocks)
                                        .WhereElementIsNotElementType()
                                        .FirstOrDefault() as FamilyInstance;

                                    if(titleBlock is null) { continue; }

                                    Parameter width = titleBlock.LookupParameter("Ширина");
                                    if(width != null) {
                                        width.Set(150 / 304.8);
                                    }

                                    Parameter height = titleBlock.LookupParameter("Высота");
                                    if(height != null) {
                                        height.Set(viewportHalfHeight * 2 + 10 / 304.8);
                                    }


                                    _revitRepository.Document.Regenerate();

                                    // Получение габаритов рамки листа
                                    BoundingBoxXYZ boundingBoxXYZ = titleBlock.get_BoundingBox(newSheet);
                                    double titleBlockWidth = boundingBoxXYZ.Max.X - boundingBoxXYZ.Min.X;
                                    double titleBlockHeight = boundingBoxXYZ.Max.Y - boundingBoxXYZ.Min.Y;

                                    double titleBlockMinY = boundingBoxXYZ.Min.Y;
                                    double titleBlockMinX = boundingBoxXYZ.Min.X;

                                    XYZ correctPosition = new XYZ(
                                        titleBlockMinX + viewportHalfWidth,
                                        titleBlockHeight / 2 + titleBlockMinY,
                                        0);

                                    viewPort.SetBoxCenter(correctPosition);
                                }

                            } catch(Exception) {
                                temp += "Возникла проблема с " + task.SelectedVisibilityScope.Name + Environment.NewLine;

                            }
                        }
                    }


                    if(temp.Length == 0) {
                        TaskDialog.Show("Отчет", "Ошибок с " + task.SelectedVisibilityScope.Name + " не было!");
                        TaskDialog.Show("Документатор АР. Отчет", report.ToString());
                        MessageBox.Show(report.ToString());
                    } else {
                        TaskDialog.Show("Ошибка!", temp);
                    }
                }

                transaction.Commit();
            }


        }
    }
}