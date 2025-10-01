using System;

using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Electrical;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.DB.Plumbing;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services;
internal class ElementOffsetFinder : IOutcomingTaskOffsetFinder {
    private readonly PipeOffsetFinder _pipeFinder;
    private readonly DuctOffsetFinder _ductFinder;
    private readonly ConduitOffsetFinder _conduitFinder;
    private readonly CableTrayOffsetFinder _cableTrayFinder;
    private readonly FamilyInstanceOffsetFinder _familyInstanceFinder;


    public ElementOffsetFinder(
        PipeOffsetFinder pipeFinder,
        DuctOffsetFinder ductFinder,
        ConduitOffsetFinder conduitFinder,
        CableTrayOffsetFinder cableTrayFinder,
        FamilyInstanceOffsetFinder familyInstanceFinder) {

        _pipeFinder = pipeFinder
            ?? throw new ArgumentNullException(nameof(pipeFinder));
        _ductFinder = ductFinder
            ?? throw new ArgumentNullException(nameof(ductFinder));
        _conduitFinder = conduitFinder
            ?? throw new ArgumentNullException(nameof(conduitFinder));
        _cableTrayFinder = cableTrayFinder
            ?? throw new ArgumentNullException(nameof(cableTrayFinder));
        _familyInstanceFinder = familyInstanceFinder
            ?? throw new ArgumentNullException(nameof(familyInstanceFinder));
    }


    public double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
        return GetOffsetFinder(mepElement).FindHorizontalOffsetsSum(opening, mepElement);
    }

    public double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement) {
        return GetOffsetFinder(mepElement).FindVerticalOffsetsSum(opening, mepElement);
    }

    public double GetMaxHorizontalOffsetSum(Element mepElement) {
        return GetOffsetFinder(mepElement).GetMaxHorizontalOffsetSum(mepElement);
    }

    public double GetMinHorizontalOffsetSum(Element mepElement) {
        return GetOffsetFinder(mepElement).GetMinHorizontalOffsetSum(mepElement);
    }

    public double GetMaxVerticalOffsetSum(Element mepElement) {
        return GetOffsetFinder(mepElement).GetMaxVerticalOffsetSum(mepElement);
    }

    public double GetMinVerticalOffsetSum(Element mepElement) {
        return GetOffsetFinder(mepElement).GetMinVerticalOffsetSum(mepElement);
    }

    private IOutcomingTaskOffsetFinder GetOffsetFinder(Element mepElement) {
        if(mepElement is Pipe) {
            return _pipeFinder;
        } else if(mepElement is Duct) {
            return _ductFinder;
        } else if(mepElement is Conduit) {
            return _conduitFinder;
        } else {
            return mepElement is CableTray
                ? _cableTrayFinder
                : mepElement is FamilyInstance
                            ? (IOutcomingTaskOffsetFinder) _familyInstanceFinder
                            : throw new InvalidOperationException($"Type doesn't support: {mepElement.GetType().FullName}");
        }
    }
}
