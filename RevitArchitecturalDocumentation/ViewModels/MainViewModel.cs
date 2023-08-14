using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;
using System.Xml.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Revit;
using dosymep.WPF.Commands;
using dosymep.WPF.ViewModels;

using RevitArchitecturalDocumentation.Models;


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
        private string _sheetNamePrefix = string.Empty;

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
        }

        public ICommand LoadViewCommand { get; }
        public ICommand AcceptViewCommand { get; }

        public ICommand AddTaskCommand { get; }
        public ICommand DeleteTaskCommand { get; }



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









        private void DoWork() {


            List<View> views = new List<View>();

            foreach(ElementId id in _revitRepository.ActiveUIDocument.Selection.GetElementIds()) {

                View view = _revitRepository.Document.GetElement(id) as View;
                if(view != null) {
                    views.Add(view);
                }
            }


            var regexForBuildingPart = new Regex(@"К(.*?)_");
            var regexForBuildingSection = new Regex(@"С(.*?)$");

            var regexForLevel = new Regex(@"^(.*?) ");

            var regexForView = new Regex(@"ПСО_(.*?) этаж");



            using(Transaction transaction = _revitRepository.Document.StartTransaction("Документатор АР")) {

                foreach(TaskInfo task in TasksForWork) {

                    if(task.SelectedVisibilityScope is null) {
                        TaskDialog.Show("Ошибка!", "Не выбрана область видимости в одной из строк");
                        continue;
                    }

                    int startLevelNumberAsInt;
                    if(!int.TryParse(task.StartLevelNumber, out startLevelNumberAsInt)) {
                        TaskDialog.Show("Ошибка!", "Начальный уровень у " + task.SelectedVisibilityScope.Name + " некорректен!");
                        continue;
                    }

                    int endLevelNumberAsInt;
                    if(!int.TryParse(task.EndLevelNumber, out endLevelNumberAsInt)) {
                        TaskDialog.Show("Ошибка!", "Начальный уровень у " + task.SelectedVisibilityScope.Name + " некорректен!");
                        continue;
                    }


                    string numberOfBuildingPart = regexForBuildingPart.Match(task.SelectedVisibilityScope.Name).Groups[1].Value;

                    int numberOfBuildingPartAsInt;
                    if(!int.TryParse(numberOfBuildingPart, out numberOfBuildingPartAsInt)) {
                        TaskDialog.Show("Ошибка!", "Не удалось определить корпус у области видимости " + task.SelectedVisibilityScope.Name);
                        continue;
                    }


                    string numberOfBuildingSection = regexForBuildingSection.Match(task.SelectedVisibilityScope.Name).Groups[1].Value;

                    int numberOfBuildingSectionAsInt;
                    if(!int.TryParse(numberOfBuildingPart, out numberOfBuildingSectionAsInt)) {
                        TaskDialog.Show("Ошибка!", "Не удалось определить секцию у области видимости " + task.SelectedVisibilityScope.Name);
                        continue;
                    }


                    string temp = string.Empty;

                    if(views.Count == 0) {

                        foreach(Level level in Levels) {

                            if(level.Name.Contains("К" + numberOfBuildingPart + "_")) {


                                string numberOfLevel = regexForLevel.Match(level.Name).Groups[1].Value;

                                int numberOfLevelAsInt;
                                if(!int.TryParse(numberOfLevel, out numberOfLevelAsInt)) {
                                    TaskDialog.Show("Ошибка!", "Не удалось определить номер уровня " + level.Name);
                                    continue;
                                }

                                if(numberOfLevelAsInt < startLevelNumberAsInt || numberOfLevelAsInt > endLevelNumberAsInt) {

                                    continue;
                                }

                                // ДДУ_4 этаж К3
                                // СК24_дом 37.2_корпус 37.2.3_секция 3_этаж 4

                                try {

                                    ViewPlan newViewPlan = ViewPlan.Create(_revitRepository.Document, SelectedViewFamilyType.Id, level.Id);
                                    newViewPlan.Name = string.Format("{0}{1} этаж К{2}", ViewNamePrefix, numberOfLevel, numberOfBuildingPart);
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

                                        titleBlock.LookupParameter("Ширина").Set(150 / 304.8);
                                        titleBlock.LookupParameter("Высота").Set(viewportHalfHeight * 2 + 10 / 304.8);

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


                                        //TaskDialog.Show("titleBlockWidth", titleBlockWidth.ToString());
                                        //TaskDialog.Show("viewportHalfWidth", viewportHalfWidth.ToString());


                                        viewPort.SetBoxCenter(correctPosition);
                                    }

                                } catch(Exception) {
                                    temp += "Возникла проблема с " + task.SelectedVisibilityScope.Name + Environment.NewLine;

                                }
                            }
                        }
                    } else {

                        foreach(View view in views) {

                            string numberOfLevel = regexForView.Match(view.Name).Groups[1].Value;

                            int numberOfLevelAsInt;
                            if(!int.TryParse(numberOfLevel, out numberOfLevelAsInt)) {
                                temp += "Не удалось определить номер уровня у вида " + view.Name + Environment.NewLine;
                                continue;
                            }

                            if(numberOfLevelAsInt < startLevelNumberAsInt || numberOfLevelAsInt > endLevelNumberAsInt) {

                                continue;
                            }

                            try {
                                ElementId newViewPlanId = view.Duplicate(ViewDuplicateOption.WithDetailing);
                                ViewPlan newViewPlan = view.Document.GetElement(newViewPlanId) as ViewPlan;
                                
                                if(newViewPlan is null) {
                                    temp += "Возникла проблема при создании с " + task.SelectedVisibilityScope.Name + Environment.NewLine;
                                    continue;
                                }

                                newViewPlan.Name = string.Format("{0}{1} этаж К{2}", ViewNamePrefix, numberOfLevel, numberOfBuildingPart);
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



                    TaskDialog.Show("Ошибка!", temp);
                }




                transaction.Commit();
            }


            




        }
    }
}