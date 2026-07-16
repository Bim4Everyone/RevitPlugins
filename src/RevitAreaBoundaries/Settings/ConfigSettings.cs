using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitAreaBoundaries.Models;
using RevitAreaBoundaries.Models.Enums;

namespace RevitAreaBoundaries.Settings;

internal class ConfigSettings {
    public AlgorithmType AlgorithmType { get; set; }
    public double SectionHeightMm { get; set; }
    public  List<ElementId> Views { get; set; }
    public  List<ElementId> Types { get; set; }
    public string GroupParam { get; set; }

    public void ApplyDefaultValues(SystemPluginConfig systemPluginConfig) {
        AlgorithmType = systemPluginConfig.DefaultAlgorithmType;
        SectionHeightMm = systemPluginConfig.DefaultSectionHeightMm;
        Views = systemPluginConfig.DefaultListViewPlans;
        Types = systemPluginConfig.DefaultListTypes;
        GroupParam = systemPluginConfig.DefaultGroupParamName;
    }
}
