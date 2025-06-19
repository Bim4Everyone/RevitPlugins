using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.Models;
internal class WallDesignData {
    /// <summary>
    /// Id стены
    /// </summary>
    public ElementId WallId { get; set; }

    /// <summary>
    /// Линии для построения стены
    /// </summary>
    public IList<Line> LinesForDraw { get; set; }

    /// <summary>
    /// Линия стены
    /// </summary>
    public Curve WallLine { get; set; }

    /// <summary>
    /// Расстояние до границы помещения
    /// </summary>
    public double DistanceFromBorder { get; set; }

    /// <summary>
    /// Граница помещения, к которой относится стена
    /// </summary>
    public RoomBorder RoomBorder { get; set; }

    /// <summary>
    /// Номер слоя стены
    /// </summary>
    public int LayerNumber { get; set; }

    /// <summary>
    /// Id типоразмера линий
    /// </summary>
    public ElementId LineStyleId { get; set; }

    /// <summary>
    /// Вектор направления внутрь помещения
    /// </summary>
    public XYZ DirectionToRoom { get; set; }
}
