using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

public class ConfigSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public double SectionHeight { get; set; }
    public  List<ElementId> Views { get; set; }
    public  List<ElementId> Types { get; set; }
    public string GroupParam { get; set; }


    public void ApplyDefaultValues(SystemPluginConfig systemPluginConfig) {
        AlgorithmType = systemPluginConfig.DefaultAlgorithmType;
        SectionHeight = systemPluginConfig.DefaultSectionHeight;
        Views = systemPluginConfig.DefaultListViewPlans;
        Types = systemPluginConfig.DefaultListTypes;
        GroupParam = systemPluginConfig.DefaultGroupParamName;
    }
}
