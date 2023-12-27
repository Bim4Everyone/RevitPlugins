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
using RevitArchitecturalDocumentation.Models.Options;
using RevitArchitecturalDocumentation.Views;

using static System.Net.Mime.MediaTypeNames;
using static Microsoft.WindowsAPICodePack.Shell.PropertySystem.SystemProperties.System;

using Parameter = Autodesk.Revit.DB.Parameter;
using View = Autodesk.Revit.DB.View;

namespace RevitArchitecturalDocumentation.ViewModels {
    internal class DocsFromSelectedViewsVM {

        public DocsFromSelectedViewsVM(CreatingARDocsVM pCOnASPDocsVM, RevitRepository revitRepository, ObservableCollection<TreeReportNode> report, 
            SheetOptions sheetOptions, ViewOptions viewOptions) {
            MVM = pCOnASPDocsVM;
            Repository = revitRepository;
            Report = report;
            SheetOpts = sheetOptions;
            ViewOpts = viewOptions;
        }

        public CreatingARDocsVM MVM { get; set; }
        public RevitRepository Repository { get; set; }
        public ObservableCollection<TreeReportNode> Report { get; set; }
        public SheetOptions SheetOpts { get; set; }



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
                        viewNamePartWithSectionPart += MVM.RegexForBuildingSectionPart.Match(viewHelper.View.Name.ToLower()).Groups[1].Value;
                    }

                    foreach(TaskInfo task in MVM.TasksForWork) {

                        TreeReportNode taskRep = new TreeReportNode(selectedViewRep) { Name = $"Задание номер: \"{task.TaskNumber}\" - " +
                            $"уровни ({task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}), {task.SelectedVisibilityScope.Name}" };

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
                            sheetHelper.GetOrCreateSheet(newSheetName, SheetOpts.SelectedTitleBlock, "Ширина", "Высота", 150, 110);
                            taskRep.Nodes.Add(sheetRep);
                        }


                        if(MVM.WorkWithViews) {

                            string newViewName = string.Format("{0}{1} этаж К{2}_С{3}{4}{5}",
                                MVM.ViewNamePrefix,
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

                                newViewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, MVM.SelectedViewportType);
                            }
                            taskRep.Nodes.Add(viewRep);
                        }

                        if(MVM.WorkWithSpecs) {

                            foreach(SpecHelper specHelper in task.ListSpecHelpers) {
                                TreeReportNode specRep = new TreeReportNode(taskRep) { Name = $"Работа со спецификацией \"{specHelper.Specification.Name}\"" };
                                specHelper.Report = specRep;

                                SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(MVM.SelectedFilterNameForSpecs, numberOfLevelAsInt);

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
