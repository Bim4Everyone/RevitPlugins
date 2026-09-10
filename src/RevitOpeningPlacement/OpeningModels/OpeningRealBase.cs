using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Базовый класс полого экземпляра семейства
/// </summary>
internal abstract class OpeningRealBase : IOpeningReal {
    /// <summary>
    /// Экземпляр семейства чистового отверстия
    /// </summary>
    private protected readonly FamilyInstance _familyInstance;

    /// <summary>
    /// Сервис построения геометрии по форме семейства
    /// </summary>
    private readonly IFamilyGeometryProvider _geometryProvider;

    /// <summary>
    /// Закэшированный BBox
    /// </summary>
    private protected BoundingBoxXYZ _boundingBox;

    /// <summary>
    /// Закэшированный солид в координатах файла
    /// </summary>
    private Solid _solid;

    /// <summary>
    /// Базовый конструктор, устанавливающий <see cref="_familyInstance"/>, <see cref="_boundingBox"/>
    /// </summary>
    /// <param name="openingReal">Экземпляр семейства проема в стене или перекрытии</param>
    /// <param name="geometryProvider">Сервис построения геометрии по форме семейства</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр является null</exception>
    /// <exception cref="ArgumentException">Исключение, если экземпляр семейства не имеет хоста</exception>
    protected OpeningRealBase(FamilyInstance openingReal, IFamilyGeometryProvider geometryProvider) {
        if(openingReal is null) { throw new ArgumentNullException(nameof(openingReal)); }
        if(openingReal.Host is null) {
            throw new ArgumentException(
                $"{nameof(openingReal)} с Id {openingReal.Id} не содержит ссылки на хост элемент");
        }
        _familyInstance = openingReal;
        _geometryProvider = geometryProvider ?? throw new ArgumentNullException(nameof(geometryProvider));
        Id = _familyInstance.Id;

        SetTransformedBBoxXYZ();
    }

    /// <summary>
    /// Id экземпляра семейства чистового проема
    /// </summary>
    public ElementId Id { get; }


    public abstract Solid GetSolid();

    public abstract BoundingBoxXYZ GetTransformedBBoxXYZ();


    /// <summary>
    /// Возвращает хост экземпляра семейства отверстия
    /// </summary>
    public Element GetHost() {
        return _familyInstance.Host;
    }

    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }

    /// <summary>
    /// Возвращает строковое значение параметра, или пустую строку, если параметра у семейства нет
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private protected string GetStringParamValue(string paramName) {
        return _familyInstance.IsExistsSharedParam(paramName)
            ? _familyInstance.GetSharedParam(paramName).AsValueString()
            : string.Empty;
    }

    /// <summary>
    /// Возвращает солид отверстия в координатах собственного файла
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Исключение, если солид не удалось построить по форме семейства</exception>
    private protected Solid GetOpeningSolid() {
        _solid ??= _geometryProvider.GetSolid(_familyInstance);
        return _solid;
    }

    /// <summary>
    /// Возвращает солид отверстия в координатах собственного файла с габаритами,
    /// увеличенными на <paramref name="inflation"/> в плоскости, перпендикулярной оси семейства
    /// </summary>
    /// <param name="inflation">Увеличение габаритов в единицах длины Revit (футах)</param>
    /// <exception cref="InvalidOperationException">
    /// Исключение, если солид не удалось построить по форме семейства</exception>
    private protected Solid GetOpeningSolid(double inflation) {
        return _geometryProvider.GetSolid(_familyInstance, inflation);
    }

    /// <summary>
    /// Возвращает значение double параметра экземпляра семейства задания на отверстие в единицах ревита,
    /// или 0, если параметр отсутствует
    /// </summary>
    /// <param name="paramName">Название параметра</param>
    private protected double GetDoubleParamValue(string paramName) {
        return _familyInstance.IsExistsSharedParam(paramName)
            ? _familyInstance.GetSharedParamValueOrDefault<double>(paramName)
            : 0;
    }

    /// <summary>
    /// Устанавливает значение полю <see cref="_boundingBox"/>
    /// </summary>
    private void SetTransformedBBoxXYZ() {
        _boundingBox = _familyInstance.GetBoundingBox();
    }
}
