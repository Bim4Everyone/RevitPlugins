namespace RevitPylonDocumentation.Models.PylonSheetNView.ViewDimensionCreatorFactories;
internal static class DimensionCreatorFactories {
    internal static readonly IDimensionCreatorFactory General = new GeneralViewDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory GeneralPerp = new GeneralViewPerpDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory GeneralRebar = new GeneralRebarViewDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory GeneralRebarPerp = new GeneralRebarViewPerpDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory TransverseFirst = new TransverseViewFirstDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory TransverseSecond = new TransverseViewSecondDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory TransverseThird = new TransverseViewThirdDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory TransverseRebarFirst = new TransverseRebarViewFirstDimCreatorFactory();
    internal static readonly IDimensionCreatorFactory TransverseRebarSecond = new TransverseRebarViewSecondDimCreatorFactory();
}
