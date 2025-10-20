using RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreators;

namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal static class AnnotationCreatorFactories {
    internal static readonly IAnnotationCreatorFactory General = new AnnotationCreatorFactory<GeneralViewAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory GeneralPerp = new AnnotationCreatorFactory<GeneralViewPerpAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseFirst = new AnnotationCreatorFactory<TransViewFirstAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseSecond = new AnnotationCreatorFactory<TransViewSecondAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseThird = new AnnotationCreatorFactory<TransViewThirdAnnotCreator>();

    internal static readonly IAnnotationCreatorFactory GeneralRebar = new AnnotationCreatorFactory<GeneralViewRebarAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory GeneralRebarPerp = new AnnotationCreatorFactory<GeneralViewRebarPerpAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseRebarFirst = new AnnotationCreatorFactory<TransViewFirstRebarAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseRebarSecond = new AnnotationCreatorFactory<TransViewSecondRebarAnnotCreator>();
    internal static readonly IAnnotationCreatorFactory TransverseRebarThird = new AnnotationCreatorFactory<TransViewThirdRebarAnnotCreator>();
}
