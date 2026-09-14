using System;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс, обозначающий чистовое отверстие АР, идущее на чертежи. 
/// Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
/// </summary>
internal class OpeningRealAr : OpeningRealBase, IEquatable<OpeningRealAr> {
    /// <summary>
    /// Создает экземпляр класса <see cref="OpeningRealAr"/>. 
    /// Использовать для обертки проемов из файла АР, когда активный файл - этот же АР или файл ВИС.
    /// </summary>
    /// <param name="openingReal">Экземпляр семейства чистового отверстия АР, идущего на чертежи</param>
    /// <param name="geometryProvider">Сервис построения геометрии по форме семейства</param>
    public OpeningRealAr(FamilyInstance openingReal, IFamilyGeometryProvider geometryProvider)
        : base(openingReal, geometryProvider) {
        Diameter = GetStringParamValue(RealOpeningArPlacer.RealOpeningArDiameter);
        Width = GetStringParamValue(RealOpeningArPlacer.RealOpeningArWidth);
        Height = GetStringParamValue(RealOpeningArPlacer.RealOpeningArHeight);
        Name = _familyInstance.Name;
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

    public string Name { get; }

    public string Comment { get; }

    /// <summary>
    /// Статус текущего отверстия относительно полученных заданий
    /// <para>Для обновления использовать <see cref="OpeningRealArInfoUpdater"/></para>
    /// </summary>
    public OpeningRealStatus Status { get; set; } = OpeningRealStatus.NotActual;


    public override bool Equals(object obj) {
        return (obj is OpeningRealAr opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return (int) Id.GetIdValue();
    }

    public bool Equals(OpeningRealAr other) {
        return (other != null) && (Id == other.Id);
    }

    public override Solid GetSolid() {
        return GetOpeningSolid();
    }

    public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return _boundingBox;
    }
}
