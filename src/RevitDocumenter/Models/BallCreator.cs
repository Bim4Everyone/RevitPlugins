using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models;

internal class BallCreator {
    private readonly RevitRepository _revitRepository;

    public BallCreator(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    public void CreateSphere(XYZ center, double radius) {
        var profile = new List<Curve>();

        radius = UnitUtilsHelper.ConvertToInternalValue(radius);
        XYZ profile00 = center;
        XYZ profilePlus = center + new XYZ(0, radius, 0);
        XYZ profileMinus = center - new XYZ(0, radius, 0);

        profile.Add(Line.CreateBound(profilePlus, profileMinus));
        profile.Add(Arc.Create(profileMinus, profilePlus, center + new XYZ(radius, 0, 0)));

        CurveLoop curveLoop = CurveLoop.Create(profile);
        SolidOptions options = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

        Frame frame = new Frame(center, XYZ.BasisX, -XYZ.BasisZ, XYZ.BasisY);
        if(Frame.CanDefineRevitGeometry(frame) != true) {
            return;
        }
        Solid sphere = GeometryCreationUtilities.CreateRevolvedGeometry(
            frame,
            [curveLoop],
            0,
            2 * Math.PI,
            options);
        DirectShape ds = DirectShape.CreateElement(
            _revitRepository.Document,
            new ElementId(BuiltInCategory.OST_GenericModel));
        ds.SetShape([sphere]);
    }
}
