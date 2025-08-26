using System.Linq;

using Autodesk.Revit.DB;

namespace RevitListOfSchedules.Models;
internal class ScheduleElement {
    private readonly RevitRepository _revitRepository;
    private readonly string _defaultScheduleName;
    private readonly string _scheduleName;

    public ScheduleElement(RevitRepository revitRepository, string defaultScheduleName, string scheduleName) {
        _revitRepository = revitRepository;
        _defaultScheduleName = defaultScheduleName;
        _scheduleName = scheduleName;
        ConfigureSchedule();
    }

    private void ConfigureSchedule() {
        var newSchedule = DuplicateDefaultSchedule();
        var defenition = newSchedule.Definition;
        var filter = defenition.GetFilters().FirstOrDefault();
        filter?.SetValue(newSchedule.Name);
        defenition.SetFilter(0, filter);
        ConfigureScheduleHeader(newSchedule);
    }

    private ViewSchedule DuplicateDefaultSchedule() {
        var defaultSchedule = _revitRepository.GetSchedule(_defaultScheduleName);
        var newScheduleId = defaultSchedule.Duplicate(ViewDuplicateOption.Duplicate);
        var newSchedule = _revitRepository.Document.GetElement(newScheduleId) as ViewSchedule;
        newSchedule.Name = $"{_defaultScheduleName}_{_scheduleName}";
        return newSchedule;
    }

    private void ConfigureScheduleHeader(ViewSchedule schedule) {
        var tableData = schedule.GetTableData();
        var appearanceSection = tableData.GetSectionData(SectionType.Header);
        appearanceSection.ClearCell(0, 0);
        appearanceSection.SetCellText(0, 0, $"{ParamFactory.ScheduleName} {_scheduleName}");
    }
}
