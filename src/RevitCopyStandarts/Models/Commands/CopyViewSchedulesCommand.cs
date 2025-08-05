using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Копирует спецификации
/// </summary>
internal class CopyViewSchedulesCommand : CopyStandartsCommand {
    public CopyViewSchedulesCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination,localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyViewSchedulesCommandName");

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(ViewSchedule));
    }
}
