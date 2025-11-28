using Autodesk.Revit.DB;

namespace RevitParamsChecker.Models.Filtration;

internal class Filter {
    public Filter() {
    }

    public string Name { get; set; }

    // TODO заменить на функционал из либы по фильтрации
    public ElementFilter GetFilter() {
        return new ElementCategoryFilter(BuiltInCategory.OST_PipeCurves);
    }
}
