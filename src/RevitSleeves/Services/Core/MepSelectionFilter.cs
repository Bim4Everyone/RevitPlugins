using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

using RevitSleeves.Models.Config;

namespace RevitSleeves.Services.Core;
internal class MepSelectionFilter : ISelectionFilter {
    private readonly SleevePlacementSettingsConfig _config;

    public MepSelectionFilter(SleevePlacementSettingsConfig config) {
        _config = config ?? throw new ArgumentNullException(nameof(config));
    }


    public bool AllowElement(Element elem) {
        if(elem is null || elem.Category is null) { return false; }

        return elem.Category.GetBuiltInCategory() == _config.PipeSettings.Category;
    }

    public bool AllowReference(Reference reference, XYZ position) {
        return false;
    }
}
