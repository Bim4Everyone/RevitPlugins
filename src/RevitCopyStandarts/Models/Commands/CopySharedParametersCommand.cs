using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

public class CopySharedParametersCommand : CopyStandartsCommand {
    public CopySharedParametersCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector().OfClass(typeof(SharedParameterElement));
    }

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return base.FilterElements(elements)
            .Where(item => !_target.IsExistsParam(item.Name));
    }
}
