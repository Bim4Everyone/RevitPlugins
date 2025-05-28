using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitFinishing.Models.Finishing {
    internal class FinishingCategory
    {
        public string Name { get; set; }
        public string KeyWord { get; set; }
        public List<BuiltInCategory> Category { get; set; }

        public static FinishingCategory Walls { get; } = new FinishingCategory() {
            Name = "Стены",
            KeyWord = "(О) Стена",
            Category = [BuiltInCategory.OST_Walls]
        };

        public static FinishingCategory Baseboards { get; } = new FinishingCategory() {
            Name = "Плинтусы",
            KeyWord = "(О) Плинтус",
            Category = [BuiltInCategory.OST_Walls]
        };

        public static FinishingCategory Floors { get; } = new FinishingCategory() {
            Name = "Перекрытия",
            KeyWord = "(АР)",
            Category = [BuiltInCategory.OST_Floors]
        };

        public static FinishingCategory Ceilings { get; } = new FinishingCategory() {
            Name = "Потолки",
            KeyWord = "(О) Потолок",
            Category = [BuiltInCategory.OST_Ceilings, BuiltInCategory.OST_Walls]
        };

        public bool CheckCategory(Element element, FinishingCategory category) {
            if(element.Name.Contains(category.KeyWord)) {
                return true;
            }
            return false;
        }
    }
}
