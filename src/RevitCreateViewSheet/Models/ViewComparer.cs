using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitCreateViewSheet.Models {
    /// <summary>
    /// Сравнивает виды по Id
    /// </summary>
    internal class ViewComparer : IEqualityComparer<View> {
        /// <summary>
        /// Создает экземпляр класса для сравнения видов по Id
        /// </summary>
        public ViewComparer() { }


        public bool Equals(View x, View y) {
            return x is not null
                && y is not null
                && x.Id == y.Id;
        }

        public int GetHashCode(View obj) {
            return 156577554 + EqualityComparer<ElementId>.Default.GetHashCode(obj?.Id);
        }
    }
}
