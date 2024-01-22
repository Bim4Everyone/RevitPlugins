using System.Collections.ObjectModel;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.ViewModels;


namespace RevitArchitecturalDocumentation.Models {
    internal class DocsFromScratch {

        public DocsFromScratch(CreatingARDocsVM pCOnASPDocsVM, RevitRepository revitRepository, ObservableCollection<TreeReportNode> report,
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
        /// В зависимости от выбора пользователя метод создает листы, виды (создает с нуля, а не копированием выбранных видов), спеки и выносит виды и спеки на листы
        /// </summary>
        public void CreateDocs() {

            using(Transaction transaction = Repository.Document.StartTransaction("Документатор АР")) {

                foreach(Level level in Repository.Levels) {

                    TreeReportNode levelRep = new TreeReportNode(null) { Name = $"Работаем с уровнем: \"{level.Name}\"" };

                    string numberOfLevel = Repository.RegexForLevel.Match(level.Name).Groups[1].Value;
                    if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                        levelRep.AddNodeWithName($"❗ Не удалось определить номер уровня {level.Name}!");
                        continue;
                    }
                    levelRep.AddNodeWithName($"Номер уровня: \"{numberOfLevelAsInt}\"");

                    foreach(TaskInfo task in TasksForWork) {

                        TreeReportNode taskRep = new TreeReportNode(levelRep) {
                            Name = $"Задание номер: \"{task.TaskNumber}\" - " +
                            $"уровни ({task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}), {task.SelectedVisibilityScope.Name}"
                        };

                        string strForLevelSearch = "К" + task.NumberOfBuildingPartAsInt.ToString() + "_";
                        if(!level.Name.Contains(strForLevelSearch)) {
                            taskRep.AddNodeWithName($"  ~  Уровень не относится к нужному корпусу, т.к. не содержит: \"{strForLevelSearch}\"");
                            levelRep.Nodes.Add(taskRep);
                            continue;
                        } else {
                            taskRep.AddNodeWithName($"Уровень относится к нужному корпусу, т.к. содержит: \"{strForLevelSearch}\"");
                        }

                        if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                            taskRep.AddNodeWithName($"  ~  Уровень \"{numberOfLevelAsInt}\" не подходит под искомый диапазон: " +
                                         $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                            levelRep.Nodes.Add(taskRep);
                            continue;
                        } else {
                            taskRep.AddNodeWithName($"Уровень \"{numberOfLevelAsInt}\" подходит под искомый диапазон: " +
                                        $"{task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}");
                        }


                        SheetHelper sheetHelper = null;
                        if(SheetOpts.WorkWithSheets) {

                            string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                SheetOpts.SheetNamePrefix,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                numberOfLevel);

                            TreeReportNode sheetRep = new TreeReportNode(taskRep) { Name = $"Работа с листом \"{newSheetName}\"" };

                            sheetHelper = new SheetHelper(Repository, sheetRep);
                            SheetOpts.SelectedTitleBlock = SheetOpts.SelectedTitleBlock ?? Repository.TitleBlocksInProject?.FirstOrDefault(a => a.Name.Equals(SheetOpts.SelectedTitleBlockName));
                            sheetHelper.GetOrCreateSheet(newSheetName, SheetOpts.SelectedTitleBlock);
                            taskRep.Nodes.Add(sheetRep);
                        }

                        if(ViewOpts.WorkWithViews) {

                            string newViewName = string.Format("{0}{1} этаж К{2}{3}",
                                ViewOpts.ViewNamePrefix,
                                numberOfLevel,
                                task.NumberOfBuildingPartAsInt,
                                task.ViewNameSuffix);

                            TreeReportNode viewRep = new TreeReportNode(taskRep) { Name = $"Работа с видом \"{newViewName}\"" };

                            ViewHelper newViewHelper = new ViewHelper(Repository, viewRep);
                            newViewHelper.GetView(newViewName, task.SelectedVisibilityScope, ViewOpts.SelectedViewFamilyType, level);

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
                        levelRep.Nodes.Add(taskRep);
                    }
                    Report.Add(levelRep);
                }
                transaction.Commit();
            }
        }
    }
}

