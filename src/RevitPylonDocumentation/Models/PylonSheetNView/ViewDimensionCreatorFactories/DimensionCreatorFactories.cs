namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal static class DimensionCreatorFactories {
    internal static readonly IAnnotationCreatorFactory General = new GeneralViewDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory GeneralPerp = new GeneralViewPerpDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory GeneralRebar = new GeneralViewRebarDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory GeneralRebarPerp = new GeneralViewPerpRebarDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseFirst = new TransViewFirstDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseSecond = new TransViewSecondDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseThird = new TransViewThirdDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseRebarFirst = new TransViewFirstRebarDimCreatorFactory();
    internal static readonly IAnnotationCreatorFactory TransverseRebarSecond = new TransViewSecondRebarDimCreatorFactory();
}
