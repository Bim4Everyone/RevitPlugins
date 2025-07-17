using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing;
internal class FinishingCategory {
    public string KeyWord { get; set; }
    public List<BuiltInCategory> Category { get; set; }

    public static readonly FinishingCategory Walls = new FinishingCategory() {
        KeyWord = "(О) Стена",
        Category = [BuiltInCategory.OST_Walls]
    };

    public static readonly FinishingCategory Baseboards = new FinishingCategory() {
        KeyWord = "(О) Плинтус",
        Category = [BuiltInCategory.OST_Walls]
    };

    public static readonly FinishingCategory Floors = new FinishingCategory() {
        KeyWord = "(АР)",
        Category = [BuiltInCategory.OST_Floors]
    };

    public static readonly FinishingCategory Ceilings = new FinishingCategory() {
        KeyWord = "(О) Потолок",
        Category = [BuiltInCategory.OST_Ceilings, BuiltInCategory.OST_Walls]
    };
}
