using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;

/// <summary>
/// Класс для обертки вентблока из связанного файла АР, подгруженного в активный документ КР.
/// <para>
/// Вентблок размещается так, что его геометрия пересекает перекрытия,
/// поэтому для КР он является входящим заданием на отверстие в перекрытии.
/// </para>
/// </summary>
internal class VentBlockAr : IOpeningTaskIncoming, IEquatable<VentBlockAr> {
    private readonly FamilyInstance _familyInstance;

    /// <summary>
    /// Закэшированный солид в координатах активного документа
    /// </summary>
    private Solid _solid;

    /// <summary>
    /// Конструктор класса для обертки вентблока из связанного файла АР,
    /// подгруженного в активный документ КР
    /// </summary>
    /// <param name="ventBlock">Экземпляр семейства вентблока из связанного файла АР</param>
    /// <param name="transform">
    /// Трансформация связанного файла АР относительно активного документа КР</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public VentBlockAr(
        FamilyInstance ventBlock,
        Transform transform) {
        _familyInstance = ventBlock ?? throw new ArgumentNullException(nameof(ventBlock));
        Transform = transform ?? throw new ArgumentNullException(nameof(transform));

        Id = _familyInstance.Id;
        FileName = _familyInstance.Document.PathName;
        Location = Transform.OfPoint(((LocationPoint) _familyInstance.Location).Point);
        // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
        Rotation = ((LocationPoint) _familyInstance.Location).Rotation
                   + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);

        // габарит вдоль локальной оси OX вентблока соответствует ширине задания на отверстие,
        // габарит вдоль локальной оси OY - высоте задания на отверстие в перекрытии
        Width = _familyInstance.Symbol.GetParamValue<double>(SharedParamsConfig.Instance.SizeLength);
        Height = _familyInstance.Symbol.GetParamValue<double>(SharedParamsConfig.Instance.SizeWidth);

        DisplayWidth = _familyInstance.Symbol.GetParam(SharedParamsConfig.Instance.SizeLength).AsValueString();
        DisplayHeight = _familyInstance.Symbol.GetParam(SharedParamsConfig.Instance.SizeWidth).AsValueString();
        Comment = _familyInstance.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
    }

    public ElementId Id { get; }

    public string FileName { get; }

    /// <summary>
    /// Трансформация связанного файла АР относительно активного документа - получателя заданий
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    /// Точка расположения вентблока в координатах активного документа - получателя заданий
    /// </summary>
    public XYZ Location { get; }

    /// <summary>
    /// Угол поворота вентблока в радианах в координатах активного файла,
    /// в который подгружена связь АР
    /// </summary>
    public double Rotation { get; }

    /// <summary>
    /// Вентблок всегда пересекает перекрытие
    /// </summary>
    public OpeningType OpeningType { get; } = OpeningType.FloorRectangle;

    /// <summary>
    /// Диаметра у вентблока нет
    /// </summary>
    public double Diameter { get; } = 0;

    /// <summary>
    /// Ширина в единицах ревита (габарит вдоль локальной оси OX вентблока)
    /// </summary>
    public double Width { get; }

    /// <summary>
    /// Высота в единицах ревита (габарит вдоль локальной оси OY вентблока)
    /// </summary>
    public double Height { get; }

    /// <summary>
    /// Значение ширины в мм
    /// </summary>
    public string DisplayWidth { get; }

    /// <summary>
    /// Значение высоты в мм
    /// </summary>
    public string DisplayHeight { get; }

    /// <summary>
    /// Комментарий
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// Статус вентблока как входящего задания на отверстие
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingArInfoUpdater"/></para>
    /// </summary>
    public OpeningTaskIncomingStatus Status { get; set; } = OpeningTaskIncomingStatus.New;

    /// <summary>
    /// Хост вентблока из активного документа КР
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingArInfoUpdater"/></para>
    /// </summary>
    public Element Host { get; set; }

    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }

    /// <summary>
    /// Возвращает солид вентблока в координатах активного файла (КР) - получателя заданий на отверстия
    /// </summary>
    public Solid GetSolid() {
        if(_solid is not null) {
            return _solid;
        }

        // геометрия вентблока находится во вложенном семействе
        var famInstSolid = _familyInstance.GetSubComponentIds()
            .Select(s => _familyInstance.Document.GetElement(s))
            .First(element => element is FamilyInstance inst && inst.Symbol.FamilyName == "_Корпус вентаблока")
            .GetSolid();
        if(famInstSolid?.GetVolumeOrDefault() > 0) {
            return _solid ??= SolidUtils.CreateTransformed(famInstSolid, Transform);
        } else {
            throw new ArgumentException(
                $"У вентблока нет геометрии ID: {Id}, файл: \'{FileName}\'");
        }
    }

    /// <summary>
    /// Возвращает BBox в координатах активного документа-получателя (КР) заданий на отверстия
    /// </summary>
    public BoundingBoxXYZ GetTransformedBBoxXYZ() {
        return GetSolid().GetTransformedBoundingBox();
    }

    public override bool Equals(object obj) {
        return (obj is VentBlockAr ventBlock) && Equals(ventBlock);
    }

    public override int GetHashCode() {
        return (int) (Id.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(VentBlockAr other) {
        return (other != null)
               && (Id == other.Id)
               && FileName.Equals(other.FileName, StringComparison.CurrentCultureIgnoreCase);
    }
}
