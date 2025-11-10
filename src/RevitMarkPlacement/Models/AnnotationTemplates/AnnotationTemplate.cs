using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Revit;

namespace RevitMarkPlacement.Models.AnnotationTemplates;

internal abstract class AnnotationTemplateOptions {
}

internal abstract class AnnotationTemplate<T> : IAnnotationTemplate where T : AnnotationTemplateOptions {
    protected readonly RevitRepository _revitRepository;
    protected readonly SystemPluginConfig _systemPluginConfig;

    protected AnnotationTemplate(
        SpotDimension spotDimension,
        AnnotationSymbolType annotationSymbolType,
        RevitRepository revitRepository,
        SystemPluginConfig systemPluginConfig) {
        _revitRepository = revitRepository;
        _systemPluginConfig = systemPluginConfig;
        SpotDimension = spotDimension;
        AnnotationSymbolType = annotationSymbolType;
    }

    public SpotDimension SpotDimension { get; }
    public AnnotationSymbolType AnnotationSymbolType { get; }

    public AnnotationInstance Process(AnnotationTemplateOptions annotationTemplateOptions) {
        AnnotationSymbol annotationSymbol = ProcessAnnotation((T) annotationTemplateOptions);
        return new AnnotationInstance(SpotDimension, annotationSymbol, AnnotationSymbolType);
    }

    protected abstract AnnotationSymbol ProcessAnnotation(T annotationTemplateOptions);

    protected AnnotationSymbol CreateAnnotation(int levelCount, double levelHeightMm) {
        var annotationSymbol = CreateAnnotationSymbol(levelCount, levelHeightMm);

        MirrorAnnotationSymbol(annotationSymbol);

        return annotationSymbol;
    }

    private AnnotationSymbol CreateAnnotationSymbol(int levelCount, double levelHeightMm) {
        var transaction = _revitRepository.StartTransaction("aaaaa");
        var placePoint = SpotDimension.LeaderEndPosition;

        AnnotationSymbol annotationSymbol = _revitRepository.CreateAnnotationSymbol(
            AnnotationSymbolType,
            placePoint,
            SpotDimension.View);

        annotationSymbol.SetParamValue(_systemPluginConfig.FirstLevelOnParamName, 0);
        annotationSymbol.SetParamValue(_systemPluginConfig.LevelCountParamName, levelCount);
        annotationSymbol.SetParamValue(_systemPluginConfig.LevelHeightParamName, levelHeightMm / 1000);

        // HACK: могут быть проблемы, если идентификатор будет больше int
        double level = GetSpotDimensionLevel(SpotDimension);

        annotationSymbol.SetParamValue(
            _systemPluginConfig.SpotDimensionIdParamName,
            (int) SpotDimension.Id.GetIdValue());

        annotationSymbol.SetParamValue(
            _systemPluginConfig.FirstLevelParamName,
            UnitUtils.ConvertFromInternalUnits(level, UnitTypeId.Meters));
        transaction.Commit();

        return annotationSymbol;
    }
    
    private void MirrorAnnotationSymbol(AnnotationSymbol annotationSymbol) {
        if(IsMirrored()) {
            var transaction = _revitRepository.StartTransaction("aaaaa");
            
            _revitRepository.MirrorAnnotationSymbol(annotationSymbol, SpotDimension.View.RightDirection);
            
            transaction.Commit();
        }
    }

    private static double GetSpotDimensionLevel(SpotDimension spot) {
        return spot.GetParamValueOrDefault<double>(BuiltInParameter.SPOT_ELEV_SINGLE_OR_UPPER_VALUE);
    }

    private bool IsMirrored() {
        double diff = SpotDimension.View.RightDirection
            .DotProduct(SpotDimension.LeaderEndPosition - SpotDimension.LeaderShoulderPosition);

        return diff < 0;
    }
}
