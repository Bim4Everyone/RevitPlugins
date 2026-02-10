using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSetCoordParams.Models.Enums;

namespace RevitSetCoordParams.Models.Settings;

internal class ConfigSettings {
    public ElementsProviderType ElementsProvider { get; set; }
    public PositionProviderType PositionProvider { get; set; }
    public string SourceFile { get; set; }
    public List<string> TypeModels { get; set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<BuiltInCategory> Categories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }

    public void ApplyDefaultValues(RevitRepository revitRepository) {
        ElementsProvider = ElementsProviderType.AllElementsProvider;
        PositionProvider = PositionProviderType.CenterPositionProvider;
        SourceFile = RevitConstants.CoordFilePartName;
        var doc = revitRepository.FindDocumentsByName(SourceFile);
        TypeModels = revitRepository.GetSourceElementsValues(doc).ToList();
        ParamMaps = RevitConstants.GetDefaultParamMaps();
        Categories = RevitConstants.GetDefaultBuiltInCategories();
        MaxDiameterSearchSphereMm = RevitConstants.MaxDiameterSearchSphereMm;
        StepDiameterSearchSphereMm = RevitConstants.StepDiameterSearchSphereMm;
        Search = RevitConstants.Search;
    }
}
