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

        return annotationSymbol;
    }

    private static double GetSpotDimensionLevel(SpotDimension spot) {
        return spot.GetParamValueOrDefault<double>(BuiltInParameter.SPOT_ELEV_SINGLE_OR_UPPER_VALUE);
    }
}
