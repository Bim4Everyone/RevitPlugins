using System;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;

using RevitOpeningPlacement.Models.RealOpeningKrPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс, обозначающий чистовое отверстие КР, идущее на чертежи.
/// Использовать для обертки проемов из активного файла КР
/// </summary>
internal class OpeningRealKr : OpeningRealBase, IEquatable<OpeningRealKr> {
    /// <summary>
    /// Создает экземпляр класса <see cref="OpeningRealKr"/>.
    /// Использовать для обертки проемов из активного файла КР.
    /// </summary>
    /// <param name="openingReal">Экземпляр семейства чистового отверстия КР, идущего на чертежи</param>
    /// <param name="geometryProvider">Сервис построения геометрии по форме семейства</param>
    public OpeningRealKr(FamilyInstance openingReal, IFamilyGeometryProvider geometryProvider)
        : base(openingReal, geometryProvider) {
        Diameter = GetStringParamValue(RealOpeningKrPlacer.RealOpeningKrDiameter);
        Width = GetStringParamValue(RealOpeningKrPlacer.RealOpeningKrInWallWidth);
        Height = GetStringParamValue(RealOpeningKrPlacer.RealOpeningKrInWallHeight);
        Comment = _familyInstance.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
    }


    /// <summary>
    /// Диаметр в мм, если есть
    /// </summary>
    public string Diameter { get; }

    /// <summary>
    /// Ширина в мм, если есть
    /// </summary>
    public string Width { get; }

    /// <summary>
    /// Высота в мм, если есть
    /// </summary>
    public string Height { get; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// Статус текущего отверстия относительно полученных заданий
    /// <para>Для обновления использовать <see cref="OpeningRealKrInfoUpdater"/></para>
    /// </summary>
    public OpeningRealStatus Status { get; set; } = OpeningRealStatus.NotActual;


    public override bool Equals(object obj) {
        return (obj != null) && (obj is OpeningRealKr opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return (int) Id.GetIdValue();
    }

    public bool Equals(OpeningRealKr other) {
        return (other != null) && (Id == other.Id);
    }

    public override Solid GetSolid() {
        return GetOpeningSolid();
    }

    /// <summary>
    /// Возвращает солид чистового отверстия с габаритами, увеличенными на <paramref name="inflation"/>
    /// в плоскости, перпендикулярной оси семейства
    /// </summary>
    /// <param name="inflation">Увеличение габаритов в единицах длины Revit (футах)</param>
    public Solid GetInflatedSolid(double inflation) {
        return GetOpeningSolid(inflation);
    }

    public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return _boundingBox;
    }
}
