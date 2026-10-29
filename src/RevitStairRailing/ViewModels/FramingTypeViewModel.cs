using Autodesk.Revit.DB;

namespace RevitStairRailing.ViewModels;

/// <summary>
/// Типоразмер семейства «Каркас несущий» в выпадающем списке.
/// </summary>
internal class FramingTypeViewModel {
    public FramingTypeViewModel(FamilySymbol symbol) {
        Id = symbol.Id;
        FamilyName = symbol.FamilyName;
        TypeName = symbol.Name;
    }

    public ElementId Id { get; }

    public string FamilyName { get; }

    public string TypeName { get; }

    /// <summary>
    /// «Семейство : Тип» — показывается в списке и сохраняется в настройках.
    /// </summary>
    public string Name => FamilyName + " : " + TypeName;

    public override string ToString() {
        return Name;
    }
}

/// <summary>
/// Семейство пластин в выпадающем списке.
/// </summary>
internal class PlateFamilyViewModel {
    public PlateFamilyViewModel(Family family) {
        Id = family.Id;
        Name = family.Name;
    }

    public ElementId Id { get; }

    public string Name { get; }

    public override string ToString() {
        return Name;
    }
}
