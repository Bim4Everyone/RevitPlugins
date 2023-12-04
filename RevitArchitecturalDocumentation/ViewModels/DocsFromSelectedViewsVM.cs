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
    internal class DocsFromSelectedViewsVM {

        public DocsFromSelectedViewsVM(PCOnASPDocsVM pCOnASPDocsVM, RevitRepository revitRepository, StringBuilder report) {
            MVM = pCOnASPDocsVM;
            Repository = revitRepository;
            Report = report;
        }

        public PCOnASPDocsVM MVM { get; set; }
        public RevitRepository Repository { get; set; }
        public StringBuilder Report { get; set; }


        /// <summary>
        /// Анализируем каждую задачу, проверяя выбор пользователя и получая данные в числовом формате
        /// </summary>
        private void AnalizeTasks() {

            Report.AppendLine("Анализируем задачи:");

            foreach(TaskInfo task in MVM.TasksForWork) {
                Report.AppendLine($"    Задача {MVM.TasksForWork.IndexOf(task) + 1}");
                task.AnalizeTask();
            }
        }


        /// <summary>
        /// В зависимости от выбора пользователя метод создает листы, виды (создает путем копирования выбранных видов), спеки и выносит виды и спеки на листы
        /// </summary>
        public void CreateDocs() {

            AnalizeTasks();

            if(MVM.TasksForWork.FirstOrDefault(o => o.CanWorkWithIt) is null) {

                Report.AppendLine("❗   Все задания имеют ошибки. Выполнение скрипта остановлено!");
                TaskDialog.Show("Документатор АР. Отчет", Report.ToString());
                return;
            }

            Report.AppendLine($"Приступаю к выполнению задания. Всего задач: {MVM.TasksForWork.Count}");
            using(Transaction transaction = Repository.Document.StartTransaction("Документатор АР")) {

                TaskDialog.Show("Отчет", "Создание на основе выбранных видов");
                TaskDialog.Show("Число выбранных видов", MVM.SelectedViews.Count.ToString());

                foreach(ViewPlan view in MVM.SelectedViews) {

                    string numberOfLevel = MVM.RegexForView.Match(view.Name.ToLower()).Groups[1].Value;

                    if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                        continue;
                    }

                    string viewNamePartWithSectionPart = string.Empty;

                    if(view.Name.ToLower().Contains("_часть ")) {
                        viewNamePartWithSectionPart = "_часть ";
                        viewNamePartWithSectionPart += MVM.RegexForBuildingSectionPart.Match(view.Name.ToLower()).Groups[1].Value;
                    }

                    int c = 0;
                    foreach(TaskInfo task in MVM.TasksForWork) {

                        c++;
                        Report.AppendLine($"    Вид \"{view.Name}\", задание номер {c}");

                        // Пропускаем те задания, в которых есть ошибки
                        if(!task.CanWorkWithIt) {
                            Report.AppendLine($"❗               В задании имеются ошибки, работа по нему выполнена не будет!");
                            continue;
                        }

                        if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {

                            continue;
                        }

                        SheetHelper sheetHelper = null;
                        if(MVM.WorkWithSheets) {

                            string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                MVM.SheetNamePrefix,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                numberOfLevel);

                            sheetHelper = new SheetHelper(Repository, Report);
                            sheetHelper.GetOrCreateSheet(newSheetName, MVM.SelectedTitleBlock, "Ширина", "Высота", 150, 110);
                        }


                        ViewHelper viewHelper = null;
                        if(MVM.WorkWithViews) {

                            string newViewName = string.Format("{0}{1} этаж К{2}_С{3}{4}{5}",
                                MVM.ViewNamePrefix,
                                numberOfLevel,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                viewNamePartWithSectionPart,
                                task.ViewNameSuffix);

                            viewHelper = new ViewHelper(Report, Repository);
                            viewHelper.GetView(newViewName, task.SelectedVisibilityScope, viewForDublicate: view);

                            if(sheetHelper.Sheet != null
                                && viewHelper.View != null
                                && Viewport.CanAddViewToSheet(Repository.Document, sheetHelper.Sheet.Id, viewHelper.View.Id)) {

                                viewHelper.PlaceViewportOnSheet(sheetHelper.Sheet, MVM.SelectedViewportType);
                            }
                        }

                        if(MVM.WorkWithSpecs) {

                            foreach(SpecHelper specHelper in task.ListSpecHelpers) {

                                SpecHelper newSpecHelper = specHelper.GetOrDublicateNSetSpec(MVM.SelectedFilterNameForSpecs, numberOfLevelAsInt);

                                // Располагаем созданные спеки на листе в позициях как у спек, с которых производилось копирование
                                // В случае если лист и размещаемая на нем спека не null и на листе еще нет вид.экрана этой спеки
                                if(sheetHelper.Sheet != null
                                    && newSpecHelper.Specification != null
                                    && !sheetHelper.HasSpecWithName(newSpecHelper.Specification.Name)) {

                                    ScheduleSheetInstance newScheduleSheetInstance = newSpecHelper.PlaceSpec(sheetHelper);
                                }
                            }
                        }
                    }

                    TaskDialog.Show("Документатор АР. Отчет", Report.ToString());
                    transaction.Commit();
                }
            }
        }
    }
}
