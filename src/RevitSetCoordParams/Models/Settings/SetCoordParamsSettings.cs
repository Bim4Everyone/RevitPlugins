using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSetCoordParams.Models.Settings;
internal class SetCoordParamsSettings {

    private readonly Document _document;

    public SetCoordParamsSettings(Document document, ConfigSettings configSettings) {
        _document = document;
        ConfigSettings = configSettings;
    }

    public ConfigSettings ConfigSettings { get; private set; }
    public List<ParamMap> ParamMaps { get; set; }
    public List<RevitCategory> RevitCategories { get; set; }
    public double MaxDiameterSearchSphereMm { get; set; }
    public double StepDiameterSearchSphereMm { get; set; }
    public bool Search { get; set; }


    public void LoadConfigSettings() {
        ParamMaps = ConfigSettings.ParamMaps;
        RevitCategories = ConfigSettings.Categories;
        MaxDiameterSearchSphereMm = ConfigSettings.MaxDiameterSearchSphereMm;
        StepDiameterSearchSphereMm = ConfigSettings.StepDiameterSearchSphereMm;
        Search = ConfigSettings.Search;
    }

    public void UpdateConfigSettings() {
        ConfigSettings.ParamMaps = ParamMaps;
        ConfigSettings.Categories = RevitCategories;
        ConfigSettings.MaxDiameterSearchSphereMm = MaxDiameterSearchSphereMm;
        ConfigSettings.StepDiameterSearchSphereMm = StepDiameterSearchSphereMm;
        ConfigSettings.Search = Search;
    }

    //private List<RevitCategory> GetCategories(Document document) {
    //    return ConfigSettings.Categories
    //        .Select(reviCat => )
    //        .Select(document.Settings.Categories.get_Item)
    //        .Where(cat => cat != null)
    //        .Select(cat => new RevitCategory() { Category = cat, IsChecked = true })
    //        .ToList();
    //}
}

