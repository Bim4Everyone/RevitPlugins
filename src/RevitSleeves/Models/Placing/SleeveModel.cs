using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitSleeves.Models.Navigator;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Models.Placing;
internal class SleeveModel : IEquatable<SleeveModel> {
    private readonly FamilyInstance _familyInstance;
    private XYZ _orientation;

    public SleeveModel(FamilyInstance familyInstance) {
        _familyInstance = familyInstance ?? throw new ArgumentNullException(nameof(familyInstance));
        Diameter = _familyInstance.GetParamValue<double>(NamesProvider.ParameterSleeveDiameter);
        Length = _familyInstance.GetParamValue<double>(NamesProvider.ParameterSleeveLength);
        Location = ((LocationPoint) _familyInstance.Location).Point;
        Id = _familyInstance.Id;
    }


    public SleeveStatus Status { get; set; }

    /// <summary>
    /// Диаметр гильзы в единицах Revit
    /// </summary>
    public double Diameter { get; }

    /// <summary>
    /// Длина гильзы в единицах Revit
    /// </summary>
    public double Length { get; }

    public XYZ Location { get; }

    public ElementId Id { get; }

    public override bool Equals(object obj) {
        return Equals(obj as SleeveModel);
    }

    public bool Equals(SleeveModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(other, this)) { return true; }

        return _familyInstance.Id == other._familyInstance.Id;
    }

    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }

    public override int GetHashCode() {
        return -1618034841 + EqualityComparer<ElementId>.Default.GetHashCode(_familyInstance.Id);
    }

    /// <summary>
    /// Возвращает вектор, направленный из <seealso cref="GetEndPoints">начальной</seealso> точки тела гильзы 
    /// в <seealso cref="GetEndPoints">конечную</seealso>
    /// </summary>
    public XYZ GetOrientation() {
        if(_orientation is not null) {
            return _orientation;
        }
        var faces = _familyInstance.GetSolids()
            .OrderByDescending(s => s.GetVolumeOrDefault() ?? 0)
            .First()
            .Faces
            .OfType<PlanarFace>()
            .Take(2)
            .OrderBy(f => f.FaceNormal.AngleTo(_familyInstance.HandOrientation))
            .ToArray();
        var start = faces[1].Origin;
        var end = faces[0].Origin;
        _orientation = (end - start).Normalize();
        return _orientation;
    }

    /// <summary>
    /// Возвращает начальную и конечную точки тела гильзы
    /// </summary>
    public (XYZ Start, XYZ End) GetEndPoints() {
        var orientation = GetOrientation();
        var start = Location - orientation * Length / 2;
        return (start, start + orientation * Length);
    }

    /// <summary>
    /// Гильзы можно объединить, только если их диаметры равны и их оси лежат на одной прямой
    /// </summary>
    public bool CanMerge(SleeveModel second) {
        if(this.Equals(second)) { return false; }
        double distanceTolerance = _familyInstance.Document.Application.ShortCurveTolerance;
        if(Math.Abs(this.Diameter - second.Diameter) > distanceTolerance) {
            return false;
        }
        double angleTolerance = _familyInstance.Document.Application.AngleTolerance;
        var locationDiff = this.Location - second.Location;
        double angle = locationDiff.IsAlmostEqualTo(XYZ.Zero) ? 0 : this.GetOrientation().AngleTo(locationDiff);
        return locationDiff.GetLength() <= (this.Length / 2 + second.Length / 2 + distanceTolerance * 2)
            && (angle <= angleTolerance || (Math.PI - angleTolerance) <= angle && angle <= Math.PI);
    }
}
