using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.ViewServices;

internal class ExportOption {
    public double MappingStepInFeet { get; set; }
    public Color ColorForAnchorLines { get; set; }
    public XYZ StartPointInRevit { get; set; }
    public XYZ EndPointInRevit { get; set; }
    public int StepCountX { get; set; }
    public int StepCountY { get; set; }
    public List<ElementId> AnchorLineIds { get; set; }

    /// <summary>
    /// Сводка по подготовке вида, которая дописывается в сообщение об ошибке построения карты.
    /// </summary>
    /// <remarks>Диагностика носит временный характер, убирается вместе с настройкой алгоритма.</remarks>
    public string Diagnostics { get; set; }
}
