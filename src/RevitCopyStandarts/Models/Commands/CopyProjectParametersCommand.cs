using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyProjectParametersCommand : CopyStandartsCommand {
    public CopyProjectParametersCommand(Document source, Document target, ILocalizationService localizationService)
        : base(source, target, localizationService) {
    }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        return base.GetFilteredElementCollector().OfClass(typeof(ParameterElement));
    }

    protected override IEnumerable<Element> FilterElements(IEnumerable<Element> elements) {
        return base.FilterElements(elements).OfType<ParameterElement>().Where(item => item.IsProjectParam());
    }
}
