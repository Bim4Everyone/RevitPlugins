using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes.Models;
/// <summary>
/// Представление плиты с необходимой информацией для построения колонн
/// </summary>
internal class SlabElement {
    public Floor Floor { get; set; }
    public Level Level { get; set; }
    public string FloorName { get; set; }
    public string LevelName { get; set; }
    public Guid Guid { get; set; }
    public List<Face> UpFaces { get; set; }
    public Transform Transform { get; set; }
    public bool IsSloped { get; set; }
}
