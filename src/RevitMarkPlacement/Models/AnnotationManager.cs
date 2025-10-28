using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitMarkPlacement.Models;

internal class AnnotationManager {
    private readonly IAnnotationPosition _annotationPosition;
    private readonly RevitRepository _revitRepository;
    private readonly SystemPluginConfig _systemPluginConfig;

    public AnnotationManager(RevitRepository revitRepository, SystemPluginConfig systemPluginConfig, IAnnotationPosition annotationPosition) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        _annotationPosition = annotationPosition;
    }

    public void CreateAnnotation(SpotDimension spot, int floorCount, double floorHeight) {
        FamilyInstance annotation;
        using(var t = _revitRepository.StartTransaction("Создание аннотации")) {
            annotation = PlaceAnnotation(spot, floorCount, floorHeight);
            SetParameters(annotation, spot, floorCount, floorHeight);
            t.Commit();
        }

        if(_annotationPosition.NeedFlip) {
            _revitRepository.MirrorAnnotation(annotation, _annotationPosition.ViewRightDirection);
        }
    }

    private FamilyInstance PlaceAnnotation(SpotDimension spot, int floorCount, double floorHeight) {
        FamilyInstance annotation = null;
        var placePoint = GetPlacePoint(spot);
        annotation = _revitRepository.CreateAnnotation(_annotationPosition.FamilySymbol, placePoint, spot.View);
        return annotation;
    }

    private double GetSpotDimensionLevel(SpotDimension spot) {
        return spot.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SINGLE_OR_UPPER_VALUE, 0.0);
    }

    private XYZ GetPlacePoint(SpotDimension spot) {
#if REVIT_2020_OR_LESS
            var elevSymbolId =
 (ElementId) spot.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.SPOT_ELEV_SYMBOL);
            var elevSymbol = _revitRepository.GetElement(elevSymbolId) as FamilySymbol;
            var width = (double) elevSymbol.GetParamValueOrDefault(RevitRepository.ElevSymbolWidth);
            var height = (double) elevSymbol.GetParamValueOrDefault(RevitRepository.ElevSymbolHeight);
            var textHeight =
 1.7 * (double) spot.SpotDimensionType.GetParamValueOrDefault(BuiltInParameter.TEXT_SIZE); // умножение на 1,7 из-за рамки вокруг текста
            var bb = spot.get_BoundingBox(spot.View);
            var scale = spot.View.Scale;
            var dir = spot.View.RightDirection;
            var bbDir = new XYZ((bb.Max - bb.Min).X, (bb.Max - bb.Min).Y, 0).Normalize();
            XYZ max = bb.Max;
            XYZ min = bb.Min;
            if(Math.Abs(dir.AngleTo(bbDir) - Math.PI) > 0.01 && Math.Abs(dir.AngleTo(bbDir)) > 0.01) {
                max = new XYZ(bb.Max.X, bb.Min.Y, bb.Max.Z);
                min = new XYZ(bb.Min.X, bb.Max.Y, bb.Min.Z);
            }
            bbDir = new XYZ((max - min).X, (max - min).Y, 0).Normalize();
            if(Math.Abs(dir.AngleTo(bbDir) - Math.PI) < 0.01) {
                var temp = new XYZ(max.X, max.Y, min.Z);
                max = new XYZ(min.X, min.Y, max.Z);
                min = temp;
            }
            return _annotationPosition.GetPoint(min, max, width, height, textHeight, scale);
#else
        return spot.LeaderEndPosition;
#endif
    }

    private void SetParameters(FamilyInstance annotation, SpotDimension spot, int count, double templateFloorHeight) {
        double level = GetSpotDimensionLevel(spot);
        annotation.SetParamValue(_systemPluginConfig.LevelCountParamName, count);
        annotation.SetParamValue(_systemPluginConfig.LevelHeightParamName, templateFloorHeight / 1000);
        annotation.SetParamValue(_systemPluginConfig.FirstLevelOnParamName, 0);

        // могут быть проблемы, если идентификатор будет больше int
        annotation.SetParamValue(_systemPluginConfig.SpotDimensionIdParamName, (int) spot.Id.GetIdValue());

#if REVIT_2020_OR_LESS
            annotation.SetParamValue(_systemPluginConfig.FirstLevelParamName, UnitUtils.ConvertFromInternalUnits(level, DisplayUnitType.DUT_METERS));
#else
        annotation.SetParamValue(
            _systemPluginConfig.FirstLevelParamName,
            UnitUtils.ConvertFromInternalUnits(level, UnitTypeId.Meters));
#endif
    }

    public void OverwriteAnnotation(
        SpotDimension spot,
        AnnotationSymbol annotation,
        int floorCount,
        double floorHeight) {
        _revitRepository.DeleteElement(annotation);
        CreateAnnotation(spot, floorCount, floorHeight);
    }

    public void UpdateAnnotation(SpotDimension spot, AnnotationSymbol annotation) {
        int floorCount = (int) annotation.GetParamValueOrDefault(_systemPluginConfig.LevelCountParamName);
        double floorHeight = (double) annotation.GetParamValueOrDefault(_systemPluginConfig.LevelHeightParamName);
        OverwriteAnnotation(spot, annotation, floorCount, floorHeight * 1000);
    }
}
