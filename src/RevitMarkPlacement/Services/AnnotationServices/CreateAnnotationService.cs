using System;
using System.Linq;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitMarkPlacement.Models;
using RevitMarkPlacement.Models.AnnotationTemplates;

namespace RevitMarkPlacement.Services.AnnotationServices;

internal sealed class CreateAnnotationService : AnnotationService {
    private const double _tolerance = 0.01;

    public CreateAnnotationService(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig)
        : base(revitRepository, systemPluginConfig) {
    }

    protected override IEnumerable<IAnnotationTemplate> LoadAnnotationsImpl(ICollection<SpotDimension> spotDimensions) {
        PrepareAnnotations();

        IReadOnlyCollection<AnnotationSymbolType> annotationSymbolTypes = _revitRepository.GetAnnotationSymbolTypes();
        foreach(SpotDimension spotDimension in spotDimensions) {
            yield return new CreateAnnotationTemplate(
                spotDimension,
                GetSymbolType(spotDimension, annotationSymbolTypes),
                _revitRepository,
                _systemPluginConfig);
        }
    }

    private void PrepareAnnotations() {
        var transaction = _revitRepository.StartTransaction("aaaaa");
      
        IReadOnlyCollection<AnnotationSymbol> annotationSymbols = _revitRepository.GetAnnotationSymbols();
        foreach(AnnotationSymbol annotationSymbol in annotationSymbols) {
            _revitRepository.DeleteElement(annotationSymbol);
        }

        transaction.Commit();
    }

    private AnnotationSymbolType GetSymbolType(
        SpotDimension spotDimension,
        IReadOnlyCollection<AnnotationSymbolType> annotationSymbolTypes) {
        var boundingBox = spotDimension.get_BoundingBox(spotDimension.View);
        var viewRightDirection = spotDimension.View.RightDirection;

        var boundingBoxDirection = new XYZ(
            (boundingBox.Max - boundingBox.Min).X,
            (boundingBox.Max - boundingBox.Min).Y,
            0).Normalize();

        var maxPoint = boundingBox.Max;
        var minPoint = boundingBox.Min;

        if(IsBoundingBoxRotated(viewRightDirection, boundingBoxDirection)) {
            maxPoint = new XYZ(boundingBox.Max.X, boundingBox.Min.Y, boundingBox.Max.Z);
            minPoint = new XYZ(boundingBox.Min.X, boundingBox.Max.Y, boundingBox.Min.Z);
        }

        if(IsSpotDimensionAboveMaxPoint(spotDimension.Origin, maxPoint)) {
            return GetSymbolType(_systemPluginConfig.FamilyTypeTopName, annotationSymbolTypes);
        }

        return GetSymbolType(_systemPluginConfig.FamilyTypeBottomName, annotationSymbolTypes);
    }

    private static bool IsBoundingBoxRotated(XYZ viewDirection, XYZ boundingBoxDirection) {
        double angle = viewDirection.AngleTo(boundingBoxDirection);
        return Math.Abs(angle - Math.PI) > _tolerance && Math.Abs(angle) > _tolerance;
    }

    private static bool IsSpotDimensionAboveMaxPoint(XYZ origin, XYZ maxPoint) {
        return origin.Z < maxPoint.Z && Math.Abs(origin.Z - maxPoint.Z) > _tolerance;
    }

    private AnnotationSymbolType GetSymbolType(
        string typeName,
        IEnumerable<AnnotationSymbolType> annotationSymbolTypes) {
        return annotationSymbolTypes.FirstOrDefault(item =>
            item.Name.Equals(typeName, StringComparison.CurrentCultureIgnoreCase));
    }
}
