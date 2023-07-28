using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.KeySchedules;
using dosymep.Bim4Everyone.ProjectParams;
using dosymep.Bim4Everyone.Schedules;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Bim4Everyone.Templates;

namespace RevitRooms.Models {
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
#if REVIT_2021_OR_LESS
            _projectParameters.SetupRevitParams(_uiApplication.ActiveUIDocument.Document,
                ProjectParamsConfig.Instance.IsRoomNumberFix,
                ProjectParamsConfig.Instance.NumberingOrder,
                SharedParamsConfig.Instance.RoomArea,
                SharedParamsConfig.Instance.RoomsCount,
                SharedParamsConfig.Instance.RoomAreaWithRatio,
                SharedParamsConfig.Instance.ApartmentAreaRatio,
                SharedParamsConfig.Instance.ApartmentAreaNoBalcony,
                SharedParamsConfig.Instance.ApartmentLivingArea,
                SharedParamsConfig.Instance.ApartmentArea,
                SharedParamsConfig.Instance.ApartmentFullArea,
                SharedParamsConfig.Instance.ApartmentNumber,
                SharedParamsConfig.Instance.ApartmentNumberExtra,
                SharedParamsConfig.Instance.Level,
                SharedParamsConfig.Instance.ApartmentGroupName,
                SharedParamsConfig.Instance.RoomGroupShortName,
                SharedParamsConfig.Instance.RoomAreaRatio,
                SharedParamsConfig.Instance.FireCompartmentShortName,
                SharedParamsConfig.Instance.RoomSectionShortName,
                SharedParamsConfig.Instance.RoomTypeGroupShortName,
                SharedParamsConfig.Instance.ApartmentAreaSpec,
                SharedParamsConfig.Instance.ApartmentAreaMinSpec,
                SharedParamsConfig.Instance.ApartmentAreaMaxSpec,
                ProjectParamsConfig.Instance.IsRoomMainLevel,
                SharedParamsConfig.Instance.RoomMultilevelGroup);
            //SharedParamsConfig.Instance.ApartmentAreaRatioFix,
            //SharedParamsConfig.Instance.ApartmentAreaNoBalconyFix,
            //SharedParamsConfig.Instance.ApartmentLivingAreaFix,
            //SharedParamsConfig.Instance.ApartmentAreaFix,
            //SharedParamsConfig.Instance.RoomAreaFix,
            //SharedParamsConfig.Instance.RoomAreaWithRatioFix);
#else
            _projectParameters.SetupRevitParams(_uiApplication.ActiveUIDocument.Document,
                ProjectParamsConfig.Instance.IsRoomNumberFix,
                ProjectParamsConfig.Instance.NumberingOrder,
                SharedParamsConfig.Instance.RoomArea,
                SharedParamsConfig.Instance.RoomsCount,
                SharedParamsConfig.Instance.RoomAreaWithRatio,
                SharedParamsConfig.Instance.ApartmentAreaRatio,
                SharedParamsConfig.Instance.ApartmentAreaNoBalcony,
                SharedParamsConfig.Instance.ApartmentLivingArea,
                SharedParamsConfig.Instance.ApartmentArea,
                SharedParamsConfig.Instance.ApartmentFullArea,
                SharedParamsConfig.Instance.ApartmentNumber,
                SharedParamsConfig.Instance.ApartmentNumberExtra,
                SharedParamsConfig.Instance.Level,
                ProjectParamsConfig.Instance.IsRoomMainLevel,
                SharedParamsConfig.Instance.RoomMultilevelGroup);
#endif

            return this;
        }

        public CheckProjectParams CopyKeySchedules() {
            var viewSchedules = new FilteredElementCollector(_uiApplication.ActiveUIDocument.Document)
                .WhereElementIsNotElementType()
                .OfCategory(BuiltInCategory.OST_Schedules)
                .ToElements();

            bool result = GetKeyScheduleRules().Select(item => item.ScheduleName).Except(viewSchedules.Select(item => item.Name)).Any();
            if(result) {
                CheckActiveView(GetScheduleRules());
            }

            // Последовательность обязательна
            // Копирование ключевых спецификаций должно происходить раньше
            // потому что в обычных спецификация есть зависимость на ключевые параметры ключевых спецификаций
            _projectParameters.SetupSchedules(_uiApplication.ActiveUIDocument.Document, false, GetKeyScheduleRules());
            _projectParameters.SetupSchedules(_uiApplication.ActiveUIDocument.Document, result, GetScheduleRules());

            return this;
        }

        private static IEnumerable<KeyScheduleRule> GetKeyScheduleRules() {
            yield return KeySchedulesConfig.Instance.FireCompartment;
            yield return KeySchedulesConfig.Instance.RoomsGroups;
            yield return KeySchedulesConfig.Instance.RoomsNames;
            yield return KeySchedulesConfig.Instance.RoomsSections;
            yield return KeySchedulesConfig.Instance.RoomsTypeGroup;
        }

        private static IEnumerable<RevitScheduleRule> GetScheduleRules() {
            yield return SchedulesConfig.Instance.RoomsCheck;
            yield return SchedulesConfig.Instance.RoomsCheckAreas;
            yield return SchedulesConfig.Instance.RoomsCheckGroup;
            yield return SchedulesConfig.Instance.RoomsCheckHeights;
            yield return SchedulesConfig.Instance.RoomsCheckInvalidArea;
            yield return SchedulesConfig.Instance.RoomsCheckName;
            yield return SchedulesConfig.Instance.RoomsCheckParams;
            yield return SchedulesConfig.Instance.RoomsCheckRemoves;
            yield return SchedulesConfig.Instance.RoomsCheckSection;
            yield return SchedulesConfig.Instance.RoomsCheckDeviationAreas;
            yield return SchedulesConfig.Instance.RoomsCheckTypes;
            yield return SchedulesConfig.Instance.RoomsCheckGroupTypes;
            yield return SchedulesConfig.Instance.RoomsMultiLevel;
        }

        public CheckProjectParams ReplaceKeySchedules(IEnumerable<KeyScheduleRule> keyScheduleRules) {
            CheckActiveView(keyScheduleRules.Union(GetScheduleRules()));

            // Последовательность обязательна
            // Копирование ключевых спецификаций должно происходить раньше
            // потому что в обычных спецификация есть зависимость на ключевые параметры ключевых спецификаций
            _projectParameters.SetupSchedules(_uiApplication.ActiveUIDocument.Document, true, keyScheduleRules);
            _projectParameters.SetupSchedules(_uiApplication.ActiveUIDocument.Document, true, GetScheduleRules());
            
            return this;
        }

        private void CheckActiveView(IEnumerable<RevitScheduleRule> scheduleRules) {
            var openedView = scheduleRules.FirstOrDefault(item => item.ScheduleName.Equals(_uiApplication.ActiveUIDocument.ActiveView.Name));
            if(openedView != null) {
                throw new InvalidOperationException($"Для копирования спецификации закройте спецификацию \"{openedView.ScheduleName}\".");
            }
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
                    MainContent = " - " 
                        + string.Join(Environment.NewLine + " - ", brokenKeySchedules.Select(item => item.TestingSchedule.Name))
                        + Environment.NewLine 
                        + Environment.NewLine 
                        + "Проверьте название ключевого параметра и количество столбцов спецификации."
                        + Environment.NewLine
                        + Environment.NewLine
                        + "ВНИМАНИЕ! При замене ключевой спецификации будут удалены соответствующие ключевые значения у помещений и ключевой параметр из спецификаций!"
                };

                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink1, "Заменить на спецификации из шаблона?");
                taskDialog.AddCommandLink(TaskDialogCommandLinkId.CommandLink2, "Выход без изменений спецификаций");
                if(taskDialog.Show() == TaskDialogResult.CommandLink1) {
                    return ReplaceKeySchedules(brokenKeySchedules.Select(item => item.KeyScheduleRule));
                }

                return this;
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

            var keyScheduleRules = GetKeyScheduleRules();
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
