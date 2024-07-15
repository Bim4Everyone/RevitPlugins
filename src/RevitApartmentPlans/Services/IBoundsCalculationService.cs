using Autodesk.Revit.DB;

using RevitApartmentPlans.Models;

namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Сервис для обработки контуров помещений и квартир
    /// </summary>
    internal interface IBoundsCalculationService {
        /// <summary>
        /// Находит наружный контур квартиры, увеличенный на заданный отступ в футах
        /// </summary>
        /// <param name="apartment">Квартира</param>
        /// <param name="feetOffset">Смещение контура квартиры наружу в футах. Должно быть неотрицательное значение.</param>
        /// <returns>Наружный контур квартиры, увеличенный на заданный отступ в футах.</returns>
        CurveLoop CreateBounds(Apartment apartment, double feetOffset);
    }
}
