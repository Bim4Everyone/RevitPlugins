using System;
using System.Linq;
using System.Text;


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


namespace RevitArchitecturalDocumentation.ViewModels {
    internal class DocsFromScratchVM : BaseViewModel {

        public DocsFromScratchVM(PCOnASPDocsVM pCOnASPDocsVM, RevitRepository revitRepository, StringBuilder report) {
            MVM = pCOnASPDocsVM;
            Repository = revitRepository;
            Report = report;
        }

        public PCOnASPDocsVM MVM { get; set; }
        public RevitRepository Repository { get; set; }
        public StringBuilder Report { get; set; }


        /// <summary>
        /// В зависимости от выбора пользователя метод создает листы, виды (создает с нуля, а не копированием выбранных видов), спеки и выносит виды и спеки на листы
        /// </summary>
        public void CreateDocs() {

            Report.AppendLine($"Исходные данные в заданиях:");
            foreach(TaskInfo task in MVM.TasksForWork) {
                Report.AppendLine($"Номер задания: {task.TaskNumber}");
                Report.AppendLine($"        Начальный уровень: {task.StartLevelNumberAsInt}");
                Report.AppendLine($"        Конечный уровень: {task.EndLevelNumberAsInt}");
                Report.AppendLine($"        Область видимости: {task.SelectedVisibilityScope.Name}");
                Report.AppendLine($"        Номер корпуса области видимости: {task.NumberOfBuildingPartAsInt}");
                Report.AppendLine($"        Номер секции области видимости: {task.NumberOfBuildingSectionAsInt}");
            }


            Report.AppendLine($"Приступаю к выполнению задания. Всего задач: {MVM.TasksForWork.Count}");
            using(Transaction transaction = Repository.Document.StartTransaction("Документатор АР")) {

                TaskDialog.Show("Отчет", "Создание с нуля");

                Report.AppendLine($"Плагин перебирает уровни и на каждом пытается выполнить задания по очереди.");
                foreach(Level level in MVM.Levels) {

                    Report.AppendLine($"Уровень \"{level.Name}\"");

                    string numberOfLevel = MVM.RegexForLevel.Match(level.Name).Groups[1].Value;
                    if(!int.TryParse(numberOfLevel, out int numberOfLevelAsInt)) {
                        Report.AppendLine($"❗       Не удалось определить номер уровня {level.Name}!");
                        continue;
                    }
                    Report.AppendLine($"    Уровень номер: {numberOfLevelAsInt}");

                    int c = 0;
                    foreach(TaskInfo task in MVM.TasksForWork) {

                        c++;
                        Report.AppendLine($"    Задание номер {c}");

                        string strForLevelSearch = "К" + task.NumberOfBuildingPartAsInt.ToString() + "_";
                        if(!level.Name.Contains(strForLevelSearch)) {
                            Report.AppendLine($"        Уровень не относится к нужному корпусу, т.к. не содержит: \"{strForLevelSearch}\":");
                            continue;
                        } else {
                            Report.AppendLine($"        Уровень относится к нужному корпусу, т.к. содержит: \"{strForLevelSearch}\":");
                        }

                        if(numberOfLevelAsInt < task.StartLevelNumberAsInt || numberOfLevelAsInt > task.EndLevelNumberAsInt) {
                            Report.AppendLine($"        Уровень: {level.Name} не подходит под диапазон {task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}:");
                            continue;
                        }
                        Report.AppendLine($"        Уровень: {level.Name} подходит под диапазон {task.StartLevelNumberAsInt} - {task.EndLevelNumberAsInt}:");


                        SheetHelper sheetHelper = null;
                        if(MVM.WorkWithSheets) {

                            string newSheetName = string.Format("{0}корпус {1}_секция {2}_этаж {3}",
                                MVM.SheetNamePrefix,
                                task.NumberOfBuildingPartAsInt,
                                task.NumberOfBuildingSectionAsInt,
                                numberOfLevel);

                            sheetHelper = new SheetHelper(Repository, Report);
                            sheetHelper.GetOrCreateSheet(newSheetName, MVM.SelectedTitleBlock);
                        }

                        ViewHelper viewHelper = null;
                        if(MVM.WorkWithViews) {

                            string newViewName = string.Format("{0}{1} этаж К{2}{3}",
                                MVM.ViewNamePrefix,
                                numberOfLevel,
                                task.NumberOfBuildingPartAsInt,
                                task.ViewNameSuffix);

                            viewHelper = new ViewHelper(Report, Repository);
                            viewHelper.GetView(newViewName, task.SelectedVisibilityScope, MVM.SelectedViewFamilyType, level);

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

                                    newSpecHelper.PlaceSpec(sheetHelper);
                                }
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

