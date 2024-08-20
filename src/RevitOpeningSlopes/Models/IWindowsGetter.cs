using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal interface IWindowsGetter {
        /// <summary>
        /// Метод возвращает коллекцию семейств категории "Окно"
        /// </summary>
        ICollection<FamilyInstance> GetOpenings();

        /// <summary>
        /// Наименование метода
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Экземпляры окон
        /// </summary>
        ICollection<FamilyInstance> Openings { get; }
    }
}
