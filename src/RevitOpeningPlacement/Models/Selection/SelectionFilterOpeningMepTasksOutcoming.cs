using System.Linq;

using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;

using dosymep.Revit;

namespace RevitOpeningPlacement.Models.Selection {
    /// <summary>
    /// Фильтр выбора пользователем экземпляров семейств заданий на отверстия из активного документа
    /// </summary>
    internal class SelectionFilterOpeningMepTasksOutcoming : ISelectionFilter {
        /// <summary>
        /// Конструктор фильтра выбора пользователем экземпляров семейств заданий на отверстия из активного документа
        /// </summary>
        public SelectionFilterOpeningMepTasksOutcoming() { }


        public bool AllowElement(Element elem) {
            return (elem != null)
                && (elem is FamilyInstance famInst)
                && (famInst.Category.GetBuiltInCategory() == BuiltInCategory.OST_GenericModel)
                && RevitRepository.OpeningTaskTypeName.Any(n => n.Value.Equals(famInst.Name))
                && RevitRepository.OpeningTaskFamilyName.Any(n => n.Value.Equals(famInst.Symbol?.FamilyName));
        }

        public bool AllowReference(Reference reference, XYZ position) {
            return false;
        }
    }
}
