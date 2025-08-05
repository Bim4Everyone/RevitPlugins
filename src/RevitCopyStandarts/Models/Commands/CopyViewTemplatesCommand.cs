using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyViewTemplatesCommand : CopyStandartsCommand {
    public CopyViewTemplatesCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination, localizationService) {
    }

    public override string Name => _localizationService.GetLocalizedString("CopyViewTemplatesCommandName");

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector()
            .OfCategory(BuiltInCategory.OST_Views)
            .WhereElementIsNotElementType();
    }

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return elements.Cast<View>().Where(item => item.IsTemplate);
    }
}
