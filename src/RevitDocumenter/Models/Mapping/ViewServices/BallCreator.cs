using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.Mapping.ViewServices;

internal class BallCreator {
    private readonly RevitRepository _revitRepository;

    public BallCreator(RevitRepository revitRepository) {
        _revitRepository = revitRepository.ThrowIfNull();
    }

    public void CreateSphere(XYZ center, double radius) {
        center.ThrowIfNull();
        radius.ThrowIfLessOrEqualThan();

        var profile = new List<Curve>();

        radius = UnitUtilsHelper.ConvertToInternalValue(radius);
        var profile00 = center;
        var profilePlus = center + new XYZ(0, radius, 0);
        var profileMinus = center - new XYZ(0, radius, 0);

        profile.Add(Line.CreateBound(profilePlus, profileMinus));
        profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

        var curveLoop = CurveLoop.Create(profile);
        var options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

        var frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
        if(Frame.CanDefineRevitGeometry(frame) != true) {
            return;
        }
        var sphere = GeometryCreationUtilities.CreateRevolvedGeometry(
            frame,
            [curveLoop],
            0,
            2 * Math.PI,
            options);
        var ds = DirectShape.CreateElement(
            _revitRepository.Document,
            new ElementId(BuiltInCategory.OST_GenericModel));
        ds.SetShape([sphere]);
    }
}
