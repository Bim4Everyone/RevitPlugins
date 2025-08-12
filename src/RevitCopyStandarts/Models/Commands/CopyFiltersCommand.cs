using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Копирует фильтры в файл View -> Filters
/// </summary>
internal class CopyFiltersCommand : CopyStandartsCommand {
    public CopyFiltersCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyFiltersCommandName");

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(FilterElement));
    }
}
