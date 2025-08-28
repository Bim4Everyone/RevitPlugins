using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitListOfSchedules.Models;
internal class ScheduleElement {
    private const int _firstLastColumnWidthMm = 30;
    private const int _middleColumnWidthMm = 110;
    private readonly RevitRepository _revitRepository;
    private readonly FamilySymbol _familySymbol;
    private readonly FamilyInstance _familyInstance;

    public ScheduleElement(
        RevitRepository revitRepository,
        FamilySymbol familySymbol,
        FamilyInstance familyInstance) {
        _revitRepository = revitRepository;
        _familySymbol = familySymbol;
        _familyInstance = familyInstance;
        CreateSchedule();
    }

    public void CreateSchedule() {
        var schedule = ViewSchedule.CreateNoteBlock(_revitRepository.Document, _familySymbol.Family.Id);
        schedule.Name = _familySymbol.Name;
        ConfigureScheduleColumns(schedule);
        ConfigureScheduleHeader(schedule);
    }

    private void ConfigureScheduleColumns(ViewSchedule schedule) {
        var definition = schedule.Definition;

        var noteField1 = definition.AddField(
            ScheduleFieldType.Instance,
            _familyInstance.GetParam(ParamFactory.FamilyParamNumber).Id);

        var noteField2 = definition.AddField(
            ScheduleFieldType.Instance,
            _familyInstance.GetParam(ParamFactory.FamilyParamName).Id);

        var noteField3 = definition.AddField(
            ScheduleFieldType.Instance,
            _familyInstance.GetParam(ParamFactory.FamilyParamRevision).Id);

        noteField1.GridColumnWidth = ConvertToInternalUnits(_firstLastColumnWidthMm);
        noteField2.GridColumnWidth = ConvertToInternalUnits(_middleColumnWidthMm);
        noteField3.GridColumnWidth = ConvertToInternalUnits(_firstLastColumnWidthMm);
    }

    private void ConfigureScheduleHeader(ViewSchedule schedule) {
        var tableData = schedule.GetTableData();
        var appearanceSection = tableData.GetSectionData(SectionType.Header);

        appearanceSection.ClearCell(0, 0);
        appearanceSection.SetCellText(0, 0, ParamFactory.ScheduleName);
    }

    private double ConvertToInternalUnits(double millimeters) {
        return UnitUtils.ConvertToInternalUnits(millimeters, UnitTypeId.Millimeters);
    }
}
