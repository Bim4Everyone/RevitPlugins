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

    public XYZ GetOrientation() {
        if(_orientation is not null) {
            return _orientation;
        }
        var faces = _familyInstance.GetSolids()
            .OrderByDescending(s => s.Volume)
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
}
