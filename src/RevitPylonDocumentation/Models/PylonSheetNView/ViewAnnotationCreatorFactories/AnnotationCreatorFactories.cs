namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewAnnotationCreatorFactories;
internal static class AnnotationCreatorFactories {
    internal static readonly IAnnotationCreatorFactory General = new GeneralViewAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory GeneralPerp = new GeneralViewPerpAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseFirst = new TransViewFirstAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseSecond = new TransViewSecondAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseThird = new TransViewThirdAnnotCreatorFactory();

    internal static readonly IAnnotationCreatorFactory GeneralRebar = new GeneralViewRebarAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory GeneralRebarPerp = new GeneralViewPerpRebarAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseRebarFirst = new TransViewFirstRebarAnnotCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseRebarSecond = new TransViewSecondRebarAnnotCreatorFactory();
}
