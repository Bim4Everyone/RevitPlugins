using System;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels.Enums;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс для обертки проема из связанного файла АР, подгруженного в активный документ КР
/// </summary>
internal class OpeningArTaskIncoming : OpeningRealBase, IEquatable<OpeningArTaskIncoming>, IOpeningTaskIncoming {
    /// <summary>
    /// Конструктор класса для обертки проема из связанного файла АР, подгруженного в активный документ КР
    /// </summary>
    /// <param name="openingTask">Задание на отверстие от АР</param>
    /// <param name="transform">
    /// Трансформация файла задания на отверстие от АР относительно активного документа КР</param>
    /// <param name="geometryProvider">Сервис построения геометрии по форме семейства</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OpeningArTaskIncoming(
        FamilyInstance openingTask,
        Transform transform,
        IFamilyGeometryProvider geometryProvider)
        : base(openingTask, geometryProvider) {
        FileName = _familyInstance.Document.PathName;
        Transform = transform ?? throw new ArgumentNullException(nameof(transform));
        Location = Transform.OfPoint(((LocationPoint) _familyInstance.Location).Point);
        // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
        Rotation = ((LocationPoint) _familyInstance.Location).Rotation
                   + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);
        OpeningType = RevitRepository.GetOpeningType(_familyInstance.Symbol.FamilyName);

        DisplayDiameter = GetStringParamValue(RealOpeningArPlacer.RealOpeningArDiameter);
        DisplayWidth = GetStringParamValue(RealOpeningArPlacer.RealOpeningArWidth);
        DisplayHeight = GetStringParamValue(RealOpeningArPlacer.RealOpeningArHeight);
        Comment = _familyInstance.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);

        Diameter = GetDoubleParamValue(RealOpeningArPlacer.RealOpeningArDiameter);
        Height = GetDoubleParamValue(RealOpeningArPlacer.RealOpeningArHeight);
        Width = GetDoubleParamValue(RealOpeningArPlacer.RealOpeningArWidth);
    }


    public string FileName { get; }

    /// <summary>
    /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    /// Точка расположения экземпляра семейства входящего задания на отверстие
    /// в координатах активного документа - получателя заданий
    /// </summary>
    public XYZ Location { get; }

    /// <summary>
    /// Угол поворота задания на отверстие в радианах в координатах активного файла,
    /// в который подгружена связь с заданием на отверстие от АР
    /// </summary>
    public double Rotation { get; }

    public string Comment { get; }

    public OpeningType OpeningType { get; }

    /// <summary>
    /// Статус входящего задания на отверстие от АР
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingArInfoUpdater"/></para>
    /// </summary>
    public OpeningTaskIncomingStatus Status { get; set; } = OpeningTaskIncomingStatus.New;

    /// <summary>
    /// Значение диаметра в мм. Если диаметра у отверстия нет, будет пустая строка.
    /// </summary>
    public string DisplayDiameter { get; }

    /// <summary>
    /// Значение ширины в мм. Если ширины у отверстия нет, будет пустая строка.
    /// </summary>
    public string DisplayWidth { get; }

    /// <summary>
    /// Значение высоты в мм. Если высоты у отверстия нет, будет пустая строка.
    /// </summary>
    public string DisplayHeight { get; }

    /// <summary>
    /// Диаметр в единицах ревита или 0, если диаметра нет
    /// </summary>
    public double Diameter { get; } = 0;

    /// <summary>
    /// Ширина в единицах ревита или 0, если ширины нет
    /// </summary>
    public double Width { get; } = 0;

    /// <summary>
    /// Высота в единицах ревита или 0, если высоты нет
    /// </summary>
    public double Height { get; } = 0;

    /// <summary>
    /// Хост входящего задания на отверстие из активного документа
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingArInfoUpdater"/></para>
    /// </summary>
    public Element Host { get; set; }


    public override bool Equals(object obj) {
        return (obj is OpeningArTaskIncoming opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return (int) (Id.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(OpeningArTaskIncoming other) {
        return (other != null)
            && (Id == other.Id)
            && FileName.Equals(other.FileName, StringComparison.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Возвращает солид архитектурного проема в координатах активного файла (КР) - получателя заданий на отверстия
    /// </summary>
    public override Solid GetSolid() {
        return SolidUtils.CreateTransformed(GetOpeningSolid(), Transform);
    }

    /// <summary>
    /// Возвращает BBox в координатах активного документа-получателя (КР) заданий на отверстия
    /// </summary>
    public override BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return GetSolid().GetTransformedBoundingBox();
    }
}
