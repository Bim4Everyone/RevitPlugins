using Autodesk.Revit.DB;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.Models;

internal abstract class LinkElementsProvider : ILinkElementsProvider {
    protected LinkElementsProvider(RevitLinkInstance link) {
        Document = link.GetLinkDocument();
        DocumentTransform = link.GetTransform();
    }

    public Document Document { get; }
    public Transform DocumentTransform { get; }

    public Solid ToLinkCoordinates(Solid solidInActiveDoc) {
        return SolidUtils.CreateTransformed(solidInActiveDoc, DocumentTransform.Inverse);
    }

    public BoundingBoxXYZ ToLinkCoordinates(BoundingBoxXYZ bboxInActiveDoc) {
        return bboxInActiveDoc.GetTransformedBoundingBox(DocumentTransform.Inverse);
    }

    public Solid ToActiveDocCoordinates(Solid solidInLink) {
        return SolidUtils.CreateTransformed(solidInLink, DocumentTransform);
    }

    public BoundingBoxXYZ ToActiveDocCoordinates(BoundingBoxXYZ bboxInLink) {
        return bboxInLink.GetTransformedBoundingBox(DocumentTransform);
    }
}
