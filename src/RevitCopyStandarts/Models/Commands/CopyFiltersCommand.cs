using Autodesk.Revit.DB;

namespace RevitCopyStandarts.Models.Commands;

/// <summary>
///     Копирует фильтры в файл View -> Filters
/// </summary>
internal class CopyFiltersCommand : CopyStandartsCommand {
    public CopyFiltersCommand(Document source, Document destination)
        : base(source, destination) {
    }

    public override string Name => "Фильтр";

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfClass(typeof(FilterElement));
    }
}
