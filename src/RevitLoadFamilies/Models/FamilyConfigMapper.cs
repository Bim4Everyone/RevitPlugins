namespace RevitLoadFamilies.Models;
internal static class FamilyConfigMapper {
    public static FamilyConfigVM ToViewModel(this FamilyConfig model) {
        return new FamilyConfigVM(model.Name) {
            FamilyPaths = model.FamilyPaths
        };
    }
}
