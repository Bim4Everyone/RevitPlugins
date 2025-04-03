using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitListOfSchedules.Models {
    internal class ScheduleElement {

        private const int _firstLastCollumn = 30;
        private const int _firstSecondCollumn = 110;
        private readonly ILocalizationService _localizationService;
        private readonly RevitRepository _revitRepository;
        private readonly FamilySymbol _famSymbol;
        private readonly FamilyInstance _famInstance;


        public ScheduleElement(
            ILocalizationService localizationService,
            RevitRepository revitRepository,
            FamilySymbol famSymbol,
            FamilyInstance famInstance) {
            _revitRepository = revitRepository;
            _localizationService = localizationService;
            _famSymbol = famSymbol;
            _famInstance = famInstance;
            CreateSchedule();
        }

        private void CreateSchedule() {

            string transactionNamePlace = _localizationService.GetLocalizedString("ScheduleElement.TransactionName");
            using(Transaction t = _revitRepository.Document.StartTransaction(transactionNamePlace)) {

                ViewSchedule vewSchedule = ViewSchedule.CreateNoteBlock(_revitRepository.Document, _famSymbol.Family.Id);

                vewSchedule.Name = _famSymbol.Name;

                TableData tableData = vewSchedule.GetTableData();
                TableSectionData appearanceSection = tableData.GetSectionData(SectionType.Header);

                ScheduleDefinition scheduleDef = vewSchedule.Definition;

                appearanceSection.ClearCell(0, 0);
                appearanceSection.SetCellText(0, 0, ParamFactory.ScheduleName);

                ScheduleField noteField1 = vewSchedule.Definition
                    .AddField(ScheduleFieldType.Instance, _famInstance.GetParam(ParamFactory.FamilyParamNumber).Id);
                ScheduleField noteField2 = vewSchedule.Definition
                    .AddField(ScheduleFieldType.Instance, _famInstance.GetParam(ParamFactory.FamilyParamName).Id);
                ScheduleField noteField3 = vewSchedule.Definition
                    .AddField(ScheduleFieldType.Instance, _famInstance.GetParam(ParamFactory.FamilyParamRevision).Id);

                double firstLastCollumn = UnitUtils.ConvertToInternalUnits(_firstLastCollumn, UnitTypeId.Millimeters);
                double secondCollumn = UnitUtils.ConvertToInternalUnits(_firstSecondCollumn, UnitTypeId.Millimeters);

                noteField1.GridColumnWidth = firstLastCollumn;
                noteField2.GridColumnWidth = secondCollumn;
                noteField3.GridColumnWidth = firstLastCollumn;

                t.Commit();
            }
        }
    }
}
