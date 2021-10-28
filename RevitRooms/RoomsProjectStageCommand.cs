using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep;
using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;

namespace RevitRooms {
    [Transaction(TransactionMode.Manual)]
    public class RoomsProjectStageCommand : IExternalCommand {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements) {
            AppDomain.CurrentDomain.AssemblyResolve += AppDomainExtensions.CurrentDomain_AssemblyResolve;
            try {
                var isChecked = new CheckProjectParams(commandData.Application)
                    .CopyProjectParams()
                    .CopyKeySchedules()
                    .CheckKeySchedules()
                    .GetIsChecked();

                if(!isChecked) {
                    TaskDialog.Show("Квартирография Стадии П.", "Заполните атрибуты у квартир.");
                    return Result.Succeeded;
                }



            } catch(Exception ex) {
#if D2020 || D2021 || D2022
                TaskDialog.Show("Квартирография Стадии П.", ex.ToString());
#else
                TaskDialog.Show("Квартирография Стадии П.", ex.Message);
#endif
            } finally {
                AppDomain.CurrentDomain.AssemblyResolve -= AppDomainExtensions.CurrentDomain_AssemblyResolve;
            }

            return Result.Succeeded;
        }
    }

    internal class CheckProjectParams {
        private readonly UIApplication _uiApplication;
        private readonly ProjectParameters _projectParameters;

        private bool _isChecked = true;

        public CheckProjectParams(UIApplication uiApplication) {
            _uiApplication = uiApplication;
            _projectParameters = ProjectParameters.Create(_uiApplication.Application);
        }

        public bool GetIsChecked() {
            return _isChecked;
        }

        public CheckProjectParams CopyProjectParams() {
            _projectParameters.SetupRevitParams(_uiApplication.ActiveUIDocument.Document,
                ProjectParamsConfig.Instance.IsRoomNumberFix,
                SharedParamsConfig.Instance.RoomArea,
                SharedParamsConfig.Instance.RoomAreaWithRatio,
                SharedParamsConfig.Instance.ApartmentAreaRatio,
                SharedParamsConfig.Instance.ApartmentAreaNoBalcony,
                SharedParamsConfig.Instance.ApartmentLivingArea,
                SharedParamsConfig.Instance.ApartmentArea,
                SharedParamsConfig.Instance.ApartmentFullArea,
                SharedParamsConfig.Instance.ApartmentNumber,
                SharedParamsConfig.Instance.ApartmentNumberExtra,
                SharedParamsConfig.Instance.Level);
                //SharedParamsConfig.Instance.ApartmentAreaRatioFix,
                //SharedParamsConfig.Instance.ApartmentAreaNoBalconyFix,
                //SharedParamsConfig.Instance.ApartmentLivingAreaFix,
                //SharedParamsConfig.Instance.ApartmentAreaFix,
                //SharedParamsConfig.Instance.RoomAreaFix,
                //SharedParamsConfig.Instance.RoomAreaWithRatioFix);

            return this;
        }

        public CheckProjectParams CopyKeySchedules() {
            var keyScheduleRules = KeySchedulesConfig.Instance.GetKeyScheduleRules();
            _projectParameters.SetupKeySchedules(_uiApplication.ActiveUIDocument.Document, false, keyScheduleRules);

            return this;
        }

        public CheckProjectParams ReplaceKeySchedules(IEnumerable<KeyScheduleRule> keyScheduleRules) {
            _projectParameters.SetupKeySchedules(_uiApplication.ActiveUIDocument.Document, true, keyScheduleRules);
            return this;
        }

        public CheckProjectParams CheckKeySchedules() {
            var keyScheduleTestings = GetKeyScheduleTestings();

            var brokenKeySchedules = new List<KeyScheduleTesting>();
            var notFilledKeySchedules = new List<KeyScheduleTesting>();
            foreach(var keyScheduleTesting in keyScheduleTestings) {
                bool brokenKeySchedule = keyScheduleTesting.IsEmptySchedule()
                    || keyScheduleTesting.IsNotCorrectKeyRevitParam()
                    || keyScheduleTesting.GetNotExistsRequiredParamsInSchedule().Any();

                if(brokenKeySchedule) {
                    brokenKeySchedules.Add(keyScheduleTesting);
                }

                bool notFilledKeySchedule = keyScheduleTesting.GetNotFilledParamsInSchedule().Any();
                if(notFilledKeySchedule) {
                    notFilledKeySchedules.Add(keyScheduleTesting);
                }
            }

            if(brokenKeySchedules.Count > 0) {
                _isChecked = false;
                var taskDialog = new TaskDialog("Квартирография Стадии П.") {
                    AllowCancellation = true,
                    MainInstruction = "Были найдены некорректные ключевые спецификации.",
                    MainContent = " - " + string.Join(Environment.NewLine + " - ", brokenKeySchedules.Select(item => item.TestingSchedule.Name))
                };

                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Заменить спецификации?");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Выход");
                if(taskDialog.Show() == TaskDialogResult.CommandLink1) {
                    return ReplaceKeySchedules(brokenKeySchedules.Select(item => item.KeyScheduleRule));
                }
            }

            if(notFilledKeySchedules.Count > 0) {
                _isChecked = false;
                var taskDialog = new TaskDialog("Квартирография Стадии П.") {
                    AllowCancellation = true,
                    MainInstruction = "Были найдены не заполненные ключевые спецификации:",
                    MainContent = " - " + string.Join(Environment.NewLine + " - ", notFilledKeySchedules.Select(item => item.TestingSchedule.Name)),
                    ExpandedContent = Environment.NewLine + string.Join(Environment.NewLine, notFilledKeySchedules.Select(item => FormatMessage(item)))
                };

                taskDialog.Show();
            }

            return this;
        }

        private string FormatMessage(KeyScheduleTesting keyScheduleTesting) {
            return $"{keyScheduleTesting.TestingSchedule.Name}{Environment.NewLine}\t- " + string.Join(Environment.NewLine + "\t- ", keyScheduleTesting.GetNotFilledParamsInSchedule().Select(item => item.Name));
        }

        private List<KeyScheduleTesting> GetKeyScheduleTestings() {
            var viewSchedules = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                .OfClass(typeof(ViewSchedule))
                .Cast<ViewSchedule>()
                .ToList();

            var keyScheduleRules = KeySchedulesConfig.Instance.GetKeyScheduleRules();
            return keyScheduleRules
                .Select(item => GetKeyScheduleTesting(item, viewSchedules))
                .Where(item => item != null)
                .ToList();
        }

        private KeyScheduleTesting GetKeyScheduleTesting(KeyScheduleRule keyScheduleRule, List<ViewSchedule> viewSchedules) {
            var viewSchedule = viewSchedules.Find(item => item.Name.Equals(keyScheduleRule.ScheduleName));
            if(viewSchedule == null) {
                return null;
            }

            return keyScheduleRule.CreateKeyScheduleTesting(viewSchedule);
        }
    }
}
