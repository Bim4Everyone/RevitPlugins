using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;
internal class ScheduleElement {
    private readonly RevitRepository _revitRepository;
    private readonly string _scheduleName;

    public ScheduleElement(RevitRepository revitRepository, string scheduleName) {
        _revitRepository = revitRepository;
        _scheduleName = scheduleName;
    }

    public void ConfigureSchedule() {
        var newSchedule = DuplicateDefaultSchedule();
        var definition = newSchedule.Definition;
        var filter = definition.GetFilters().FirstOrDefault();
        filter?.SetValue(newSchedule.Name);
        definition.SetFilter(0, filter);
        ConfigureScheduleHeader(newSchedule);
    }

    private ViewSchedule DuplicateDefaultSchedule() {
        var defaultSchedule = _revitRepository.GetSchedule(ParamFactory.DefaultScheduleName);
        var newScheduleId = defaultSchedule.Duplicate(ViewDuplicateOption.Duplicate);
        var newSchedule = _revitRepository.Document.GetElement(newScheduleId) as ViewSchedule;
        newSchedule.Name = _scheduleName;
        return newSchedule;
    }

    private void ConfigureScheduleHeader(ViewSchedule schedule) {
        var tableData = schedule.GetTableData();
        var appearanceSection = tableData.GetSectionData(SectionType.Header);
        appearanceSection.ClearCell(0, 0);
        appearanceSection.SetCellText(0, 0, ParamFactory.ScheduleName);
    }
}
