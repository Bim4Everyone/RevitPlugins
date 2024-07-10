using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels;

namespace RevitArchitecturalDocumentation.Models {
    internal class DocsFromSelectedViews {

        public DocsFromSelectedViews(CreatingARDocsVM pCOnASPDocsVM, RevitRepository revitRepository, ObservableCollection<TreeReportNode> report,
            ObservableCollection<TaskInfo> tasksForWork, MainOptions mainOptions) {
            MVM = pCOnASPDocsVM;
            Repository = revitRepository;
            Report = report;
            TasksForWork = tasksForWork;
            SheetOpts = mainOptions.SheetOpts;
            ViewOpts = mainOptions.ViewOpts;
            SpecOpts = mainOptions.SpecOpts;
        }

        public CreatingARDocsVM MVM { get; set; }
        public RevitRepository Repository { get; set; }
        public ObservableCollection<TreeReportNode> Report { get; set; }
        public SheetOptions SheetOpts { get; set; }
        public ViewOptions ViewOpts { get; set; }
        public SpecOptions SpecOpts { get; set; }
        public ObservableCollection<TaskInfo> TasksForWork { get; set; }



        /// <summary>
        /// В зависимости от выбора пользователя метод создает листы, виды (создает путем копирования выбранных видов), спеки и выносит виды и спеки на листы
        /// </summary>
        public void CreateDocs() {

            using(Transaction transaction = Repository.Document.StartTransaction("Документатор АР")) {

                foreach(ViewHelper viewHelper in MVM.SelectedViewHelpers) {
                    int numberOfLevelAsInt = viewHelper.NameHelper.LevelNumber;
                    string numberOfLevelAsStr = viewHelper.NameHelper.LevelNumberAsStr;

                    TreeReportNode selectedViewRep = new TreeReportNode(null) { Name = $"Работаем с выбранным видом: \"{viewHelper.View.Name}\"" };
                    selectedViewRep.AddNodeWithName($"Номер этажа в соответствии с именем выбранного вида: \"{numberOfLevelAsInt}\"");

                    string viewNamePartWithSectionPart = string.Empty;

                    if(viewHelper.View.Name.ToLower().Contains("_часть ")) {
                        viewNamePartWithSectionPart = "_часть ";
                        viewNamePartWithSectionPart += Repository.RegexForBuildingSectionPart.Match(viewHelper.View.Name.ToLower()).Groups[1].Value;
                    }

                    foreach(TaskInfo task in TasksForWork) {

                        TreeReportNode taskRep = new TreeReportNode(selectedViewRep) {
                            Name = $"Задание номер: \"{task.TaskNumber}\" - " +
                            $"уровни ({task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}), {task.SelectedVisibilityScope.Name}"
                        };

                        if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                            taskRep.AddNodeWithName($"  ~  Уровень вида \"{numberOfLevelAsInt}\" не подходит под искомый диапазон: " +
                                $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                            selectedViewRep.Nodes.Add(taskRep);
                            continue;
                        }

                        SheetHelper sheetHelper = null;
                        if(SheetOpts.WorkWithSheets) {

                            string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                SheetOpts.SheetNamePrefix,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                numberOfLevelAsStr);

                            TreeReportNode sheetRep = new TreeReportNode(taskRep) { Name = $"Работа с листом \"{newSheetName}\"" };
                            sheetHelper = new SheetHelper(Repository, sheetRep);
                            SheetOpts.SelectedTitleBlock = SheetOpts.SelectedTitleBlock ?? Repository.TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(SheetOpts.SelectedTitleBlockName));
                            sheetHelper.GetOrCreateSheet(newSheetName, SheetOpts.SelectedTitleBlock, "Ширина", "Высота", 150, 110);
                            taskRep.Nodes.Add(sheetRep);
                        }


                        if(ViewOpts.WorkWithViews) {

                            string newViewName = string.Format("{0}{1} этаж К{2}_С{3}{4}{5}",
                                ViewOpts.ViewNamePrefix,
                                numberOfLevelAsStr,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                viewNamePartWithSectionPart,
                                task.ViewNameSuffix);

                            TreeReportNode viewRep = new TreeReportNode(taskRep) { Name = $"Работа с видом \"{newViewName}\"" };

                            ViewHelper newViewHelper = new ViewHelper(Repository, viewRep);
                            newViewHelper.GetView(newViewName, task.SelectedVisibilityScope, viewForDublicate: viewHelper.View);

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
                                TreeReportNode specRep = new TreeReportNode(taskRep) { Name = $"Работа со спецификацией \"{specHelper.Specification.Name}\"" };
                                specHelper.Report = specRep;

                                SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(SpecOpts.SelectedFilterNameForSpecs, numberOfLevelAsInt);

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
    }
}
