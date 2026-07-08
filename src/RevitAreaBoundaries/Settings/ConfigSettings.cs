using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

public class ConfigSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public double SectionHeight { get; set; }
    public  List<RevitElementView> SelectedViewPlans { get; set; }
    public  List<RevitElementType> SelectedTypes { get; set; }
    public string GroupParam { get; set; }


    public void ApplyDefaultValues(SystemPluginConfig systemPluginConfig) {
        AlgorithmType = systemPluginConfig.DefaultAlgorithmType;
        SectionHeight = systemPluginConfig.DefaultSectionHeight;
        SelectedViewPlans = systemPluginConfig.DefaultListViewPlans;
        SelectedTypes = systemPluginConfig.DefaultListTypes;
        GroupParam = systemPluginConfig.DefaultGroupParamName;
    }
}
