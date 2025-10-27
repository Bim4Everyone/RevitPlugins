using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels;
using RevitArchitecturalDocumentation.ViewModels.Components;

namespace RevitArchitecturalDocumentation.Models;
internal class DocsFromSelectedViews {
    private readonly ILocalizationService _localizationService;
    private readonly string _paramNameWidth = "Ширина";
    private readonly string _paramNameHeight = "Высота";

    public DocsFromSelectedViews(CreatingARDocsVM pCOnASPDocsVM, RevitRepository revitRepository, ObservableCollection<TreeReportNode> report,
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
    /// В зависимости от выбора пользователя метод создает листы, виды (создает путем копирования выбранных видов), спеки и выносит виды и спеки на листы
    /// </summary>
    public void CreateDocs() {

        using var transaction = Repository.Document.StartTransaction(_localizationService.GetLocalizedString("CreatingARDocsV.Title"));

        foreach(var viewHelper in MVM.SelectedViewHelpers) {
            int numberOfLevelAsInt = viewHelper.NameHelper.LevelNumber;
            string numberOfLevelAsStr = viewHelper.NameHelper.LevelNumberAsStr;

            var selectedViewRep = new TreeReportNode(null) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithSelectedView")} \"{viewHelper.View.Name}\"" };
            selectedViewRep.AddNodeWithName($"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.LevelNumberBySelectedView")} \"{numberOfLevelAsInt}\"");

            string viewNamePartWithSectionPart = string.Empty;

            if(viewHelper.View.Name.ToLower().Contains(_localizationService.GetLocalizedString("CreatingARDocsV.Report.Part"))) {
                viewNamePartWithSectionPart = _localizationService.GetLocalizedString("CreatingARDocsV.Report.Part");
                viewNamePartWithSectionPart += Repository.RegexForBuildingSectionPart.Match(viewHelper.View.Name.ToLower()).Groups[1].Value;
            }

            foreach(var task in TasksForWork) {

                var taskRep = new TreeReportNode(selectedViewRep) {
                    Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.TaskNumber")} \"{task.TaskNumber}\" - " +
                    $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.Levels")} ({task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}), {task.SelectedVisibilityScope.Name}"
                };

                if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                    taskRep.AddNodeWithName($"  ~  {_localizationService.GetLocalizedString("CreatingARDocsV.Report.Level")} \"{numberOfLevelAsInt}\" " +
                        $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.NotInRange")} " +
                        $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                    selectedViewRep.Nodes.Add(taskRep);
                    continue;
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
                        numberOfLevelAsStr);

                    var sheetRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithSheet")} \"{newSheetName}\"" };
                    sheetHelper = new SheetHelper(Repository, sheetRep);
                    SheetOpts.SelectedTitleBlock = SheetOpts.SelectedTitleBlock ?? Repository.TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(SheetOpts.SelectedTitleBlockName));

                    sheetHelper.GetOrCreateSheet(newSheetName, SheetOpts.SelectedTitleBlock, _paramNameWidth, _paramNameHeight, 150, 110);
                    taskRep.Nodes.Add(sheetRep);
                }


                if(ViewOpts.WorkWithViews) {

                    string newViewName = string.Format("{0}{1} {2} К{3}_С{4}{5}{6}",
                        ViewOpts.ViewNamePrefix,
                        numberOfLevelAsStr,
                        _localizationService.GetLocalizedString("CreatingARDocsVM.Report.Floor"),
                        task.NumberOfBuildingPartAsInt,
                        task.NumberOfBuildingSectionAsInt,
                        viewNamePartWithSectionPart,
                        task.ViewNameSuffix);

                    var viewRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithView")} \"{newViewName}\"" };

                    var newViewHelper = new ViewHelper(Repository, viewRep);
                    newViewHelper.GetView(newViewName, task.SelectedVisibilityScope, viewForDuplicate: viewHelper.View);

                    if(sheetHelper.Sheet != null
                        && newViewHelper.View != null
                        && Viewport.CanAddViewToSheet(Repository.Document, sheetHelper.Sheet.Id, newViewHelper.View.Id)) {
                        ViewOpts.SelectedViewportType = ViewOpts.SelectedViewportType ?? Repository.ViewportTypes?.FirstOrDefault(a => a.Name.Equals(ViewOpts.SelectedViewportTypeName));
                        newViewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, ViewOpts.SelectedViewportType);
                    }
                    taskRep.Nodes.Add(viewRep);
                }

                if(SpecOpts.WorkWithSpecs) {

                    foreach(var specHelper in task.ListSpecHelpers) {
                        var specRep = new TreeReportNode(taskRep) { Name = $"{_localizationService.GetLocalizedString("CreatingARDocsV.Report.WorkWithSpec")} \"{specHelper.Specification.Name}\"" };
                        specHelper.Report = specRep;

                        var newSpecHelper = specHelper.GetOrDuplicateNSetSpec(SpecOpts.SelectedFilterNameForSpecs, numberOfLevelAsInt);

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
                selectedViewRep.Nodes.Add(taskRep);
            }
            Report.Add(selectedViewRep);
        }
        transaction.Commit();
    }
}
