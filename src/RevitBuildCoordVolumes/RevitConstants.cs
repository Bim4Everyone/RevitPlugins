using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitBuildCoordVolumes;
internal class RevitConstants {

    public readonly ICollection<BuiltInCategory> SlabCategories = [
        BuiltInCategory.OST_StructuralFoundation,
        BuiltInCategory.OST_Floors];

    public readonly ICollection<string> SlabTypeNames = [
        "(КР)",
        "(К)",
        "КЖ",
        "кж",
        "ЖБ",
        "жб",
        "Плита",
        "плита",
        "Железобетон",
        "железобетон",
        "Монолит",
        "монолит"];


}
