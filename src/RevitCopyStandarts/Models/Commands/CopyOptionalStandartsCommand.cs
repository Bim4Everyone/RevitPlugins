using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.SimpleServices;

namespace RevitCopyStandarts.Models.Commands;

internal class CopyOptionalStandartsCommand : CopyStandartsCommand {
    public CopyOptionalStandartsCommand(Document source, Document destination, ILocalizationService localizationService)
        : base(source, destination, localizationService) {
    }

    public string BuiltInCategoryName { get; set; }

    protected override FilteredElementCollector GetFilteredElementCollector() {
        var revitApi = AppDomain.CurrentDomain.GetAssemblies()
            .FirstOrDefault(item => item.GetName().Name.Equals("RevitAPI"));
        return base.GetFilteredElementCollector()
            .OfClass(Type.GetType(BuiltInCategoryName + "," + revitApi?.GetName(), true, true));
    }
}
