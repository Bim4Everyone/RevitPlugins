using Autodesk.Revit.DB;

using Bim4Everyone.RevitFiltration.Controls;

using dosymep.Bim4Everyone;

namespace RevitParamsChecker.ViewModels.Filtration;

internal class FilterableParam : IParam {
    public FilterableParam(RevitParam revitParam, ElementId id) {
        Name = revitParam.Name;
        UnitType = revitParam.UnitType;
        StorageType = revitParam.StorageType;
        Id = id;
    }

    public string Name { get; }
#if REVIT_2020_OR_LESS
    UnitType = parameter.UnitType;
#else
    public ForgeTypeId UnitType { get; }
#endif
    public StorageType StorageType { get; }
    public ElementId Id { get; }
}
