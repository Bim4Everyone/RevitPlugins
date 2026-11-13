using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services;

/// <summary>
/// Сервис построения геометрии экземпляров семейств, у которых нет собственной геометрии в модели.
/// <para>
/// Использовать для чистовых отверстий АР и КР.
/// </para>
/// </summary>
internal interface IFamilyGeometryProvider {
    /// <summary>
    /// Возвращает солид экземпляра семейства в координатах его собственного файла.
    /// </summary>
    /// <param name="instance">Экземпляр семейства.</param>
    /// <returns>Солид экземпляра семейства.</returns>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null.</exception>
    /// <exception cref="System.InvalidOperationException">
    /// Исключение, если семейство не поддерживается, либо у него отсутствуют необходимые общие параметры.
    /// </exception>
    Solid GetSolid(FamilyInstance instance);

    /// <summary>
    /// Возвращает солид экземпляра семейства в координатах его собственного файла с габаритами,
    /// увеличенными на <paramref name="inflation"/> с каждой стороны
    /// в плоскости, перпендикулярной оси семейства.
    /// <para>Габарит вдоль оси семейства (толщина отверстия) не изменяется.</para>
    /// </summary>
    /// <param name="instance">Экземпляр семейства.</param>
    /// <param name="inflation">Увеличение габаритов в единицах длины Revit (футах).</param>
    /// <returns>Увеличенный солид экземпляра семейства.</returns>
    /// <exception cref="System.ArgumentNullException">Исключение, если обязательный параметр null.</exception>
    /// <exception cref="System.InvalidOperationException">
    /// Исключение, если семейство не поддерживается, либо у него отсутствуют необходимые общие параметры.
    /// </exception>
    Solid GetSolid(FamilyInstance instance, double inflation);
}
