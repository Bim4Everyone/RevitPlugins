using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels;
using RevitArchitecturalDocumentation.ViewModels.Components;


namespace RevitArchitecturalDocumentation.Models;
internal class DocsFromScratch {
    private readonly ILocalizationService _localizationService;

    public DocsFromScratch(CreatingARDocsVM pCOnASPDocsVM, RevitRepository revitRepository, ObservableCollection<TreeReportNode> report,
        ObservableCollection<TaskInfoVM> tasksForWork, MainOptions mainOptions, ILocalizationService localizationService) {
        MVM = pCOnASPDocsVM;
        Repository = revitRepository;
        Report = report;
        TasksForWork = tasksForWork;
        SheetOpts = mainOptions.SheetOpts;
        ViewOpts = mainOptions.ViewOpts;
        SpecOpts = mainOptions.SpecOpts;
        _localizationService = localizationService;
    }

    public CreatingARDocsVM MVM { get; set; }
    public RevitRepository Repository { get; set; }
    public ObservableCollection<TreeReportNode> Report { get; set; }
    public SheetOptions SheetOpts { get; set; }
    public ViewOptions ViewOpts { get; set; }
    public SpecOptions SpecOpts { get; set; }


    public ObservableCollection<TaskInfoVM> TasksForWork { get; set; }



    /// <summary>
    /// В зависимости от выбора пользователя метод создает листы, виды (создает с нуля, а не копированием выбранных видов), спеки и выносит виды и спеки на листы
    /// </summary>
    public void CreateDocs() {

        using var transaction = Repository.Document.StartTransaction(_localizationService.GetLocalizedString("CreatingARDocsV.Title"));

        foreach(var level in Repository.Levels) {

            var levelRep = new TreeReportNode(null) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithLevel")} \"{level.Name}\"" };

            string numberOfLevel = Repository.RegexForLevel.Match(level.Name).Groups[1].Value;
            if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                levelRep.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.LevelNumberNotDetected")} {level.Name}!");
                continue;
            }
            levelRep.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.LevelNumber")} \"{numberOfLevelAsInt}\"");

            foreach(var task in TasksForWork) {

                var taskRep = new TreeReportNode(levelRep) {
                    Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.TaskNumber")} \"{task.TaskNumber}\" - " +
                    $"уровни ({task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}), {task.SelectedVisibilityScope.Name}"
                };

                string strForLevelSearch = "К" + task.NumberOfBuildingPartAsInt.ToString() + "_";
                if(!level.Name.Contains(strForLevelSearch)) {
                    taskRep.AddNodeWithName($"  ~  {_localizationService.GetLocalizedString("CreatingARDocsV.Report.LevelNotMatch")} \"{strForLevelSearch}\"");
                    levelRep.Nodes.Add(taskRep);
                    continue;
                } else {
                    taskRep.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.LevelMatch")} \"{strForLevelSearch}\"");
                }

                if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                    taskRep.AddNodeWithName($"  ~  {_localizationService.GetLocalizedString("CreatingARDocsV.Report.Level")} \"{numberOfLevelAsInt}\" " +
                        $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.NotInRange")}  " +
                                 $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                    levelRep.Nodes.Add(taskRep);
                    continue;
                } else {
                    taskRep.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.Level")} \"{numberOfLevelAsInt}\" " +
                        $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.InRange")} " +
                                $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                }


                SheetHelper sheetHelper = null;
                if(SheetOpts.WorkWithSheets) {

                    string newSheetName = string.Format("{0}{1} {2}_{3} {4}_{5} {6}",
                        SheetOpts.SheetNamePrefix,
                        _localizationService.GetLocalizedString("CreatingARDocsV.Report.Building"),
                        task.NumberOfBuildingPartAsInt,
                        _localizationService.GetLocalizedString("CreatingARDocsV.Report.Section"),
                        task.NumberOfBuildingSectionAsInt,
                        _localizationService.GetLocalizedString("CreatingARDocsV.Report.Floor"),
                        numberOfLevel);

                    var sheetRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithSheet")} \"{newSheetName}\"" };

                    sheetHelper = new SheetHelper(Repository, _localizationService, sheetRep);
                    SheetOpts.SelectedTitleBlock = SheetOpts.SelectedTitleBlock ?? Repository.TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(SheetOpts.SelectedTitleBlockName));
                    sheetHelper.GetOrCreateSheet(newSheetName, SheetOpts.SelectedTitleBlock);
                    taskRep.Nodes.Add(sheetRep);
                }

                if(ViewOpts.WorkWithViews) {

                    string newViewName = string.Format("{0}{1} {2} К{3}{4}",
                        ViewOpts.ViewNamePrefix,
                        numberOfLevel,
                        _localizationService.GetLocalizedString("CreatingARDocsV.Report.Floor"),
                        task.NumberOfBuildingPartAsInt,
                        task.ViewNameSuffix);

                    var viewRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithView")} \"{newViewName}\"" };

                    var newViewHelper = new ViewHelper(Repository, _localizationService, viewRep);
                    _ = newViewHelper.GetView(newViewName, task.SelectedVisibilityScope, ViewOpts.SelectedViewFamilyType, level);

                    if(sheetHelper.Sheet != null
                        && newViewHelper.View != null
                        && Viewport.CanAddViewToSheet(Repository.Document, sheetHelper.Sheet.Id, newViewHelper.View.Id)) {
                        ViewOpts.SelectedViewportType = ViewOpts.SelectedViewportType ?? Repository.ViewportTypes?.FirstOrDefault(a => a.Name.Equals(ViewOpts.SelectedViewportTypeName));
                        newViewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, ViewOpts.SelectedViewportType);
                    }
                    taskRep.Nodes.Add(viewRep);
                }

                if(SpecOpts.WorkWithSpecs) {

                    foreach(SpecHelper specHelper in task.ListSpecHelpers) {
                        var specRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithSpec")} \"{specHelper.Specification.Name}\"" };
                        specHelper.Report = specRep;

                        SpecHelper newSpecHelper = specHelper.GetOrDuplicateNSetSpec(SpecOpts.SelectedFilterNameForSpecs, numberOfLevelAsInt);

                        // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование
                        // В случае если лист и размещаемая на нем спека не null и на листе еще нет вид.экрана этой спеки
                        if(sheetHelper.Sheet != null
                            && newSpecHelper.Specification != null
                            && !sheetHelper.HasSpecWithName(newSpecHelper.Specification.Name)) {

                            newSpecHelper.PlaceSpec(sheetHelper);
                        }
                        taskRep.Nodes.Add(specRep);
                    }
                }
                levelRep.Nodes.Add(taskRep);
            }
            Report.Add(levelRep);
        }
        transaction.Commit();
    }
}

