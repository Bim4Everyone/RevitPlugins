using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitListOfSchedules.ViewModels;

namespace RevitListOfSchedules.Interfaces;
internal interface IFamilyDocument {
    interface IFamilyDocument {
        /// <summary>
        /// Возвращает типоразмер семейства, созданный в документе.
        /// </summary>
        FamilySymbol FamilySymbol { get; }

        /// <summary>
        /// Размещает экземпляры семейств на виде (View) для каждой спецификации (ViewSchedule).
        /// </summary>
        /// <param name="view">Вид, на котором размещаются экземпляры семейств.</param>
        /// <param name="sheetViewModel">Модель представления листа.</param>
        /// <param name="viewSchedules">Список спецификаций (ViewSchedule).</param>
        /// <returns>Список созданных экземпляров семейств.</returns>
        IList<FamilyInstance> PlaceFamilyInstances(
            View view, SheetViewModel sheetViewModel, IList<ViewSchedule> viewSchedules);

        /// <summary>
        /// Создает экземпляр семейства на виде (View) с указанными параметрами.
        /// </summary>
        /// <param name="view">Вид, на котором создается экземпляр семейства.</param>
        /// <param name="name">Параметр имени спецификации.</param>
        /// <param name="number">Параметр номера листа.</param>
        /// <param name="revisionNumber">Параметр изменения листа.</param>
        /// <returns>Созданный экземпляр семейства.</returns>
        FamilyInstance CreateInstance(View view, string name, string number, string revisionNumber);
    }
}
