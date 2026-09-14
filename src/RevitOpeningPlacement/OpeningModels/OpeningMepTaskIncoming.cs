using System;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.Revit.Geometry;

using RevitClashDetective.Models.Extensions;

using RevitOpeningPlacement.Models;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels.Enums;
using RevitOpeningPlacement.Services;

namespace RevitOpeningPlacement.OpeningModels;
/// <summary>
/// Класс, обозначающий экземпляры семейств заданий на отверстия из связанного файла-задания на отверстия,
/// подгруженного в текущий документ получателя
/// </summary>
internal class OpeningMepTaskIncoming : IOpeningTaskIncoming, IEquatable<OpeningMepTaskIncoming> {
    /// <summary>
    /// Экземпляр семейства задания на отверстие из связанного файла
    /// </summary>
    private readonly FamilyInstance _familyInstance;

    /// <summary>
    /// Закэшированный солид в координатах активного документа
    /// </summary>
    private Solid _solid;


    /// <summary>
    /// Экземпляр семейства задания на отверстие, расположенного в связанном файле задания на отверстия
    /// 
    /// <para>Примечание: конструктор не обновляет свойства <see cref="Status"/> и <see cref="Host"/>. Для обновления этих свойств использовать <see cref="OpeningTaskIncomingMepInfoUpdater"/></para>
    /// </summary>
    /// <param name="openingTaskIncoming">Экземпляр семейства задания на отверстие из связанного файла</param>
    /// <param name="transform">Трансформация связанного файла, в котором создано задание на отверстие</param>
    public OpeningMepTaskIncoming(FamilyInstance openingTaskIncoming, Transform transform) {
        _familyInstance = openingTaskIncoming ?? throw new ArgumentNullException(nameof(openingTaskIncoming));

        Id = _familyInstance.Id;
        Transform = transform;
        Location = Transform.OfPoint(((LocationPoint) _familyInstance.Location).Point);
        // https://forums.autodesk.com/t5/revit-api-forum/get-angle-from-transform-basisx-basisy-and-basisz/td-p/5326059
        Rotation = ((LocationPoint) _familyInstance.Location).Rotation
                   + Transform.BasisX.AngleOnPlaneTo(Transform.OfVector(Transform.BasisX), Transform.BasisZ);
        FileName = _familyInstance.Document.PathName;
        OpeningType = RevitRepository.GetOpeningType(openingTaskIncoming.Symbol.Family.Name);

        Date = GetStringParamValue(RevitRepository.OpeningDate);
        MepSystem = GetStringParamValue(RevitRepository.OpeningMepSystem);
        Description = GetStringParamValue(RevitRepository.OpeningDescription);
        CenterOffset = GetStringParamValue(RevitRepository.OpeningOffsetCenter);
        BottomOffset = GetStringParamValue(RevitRepository.OpeningOffsetBottom);
        DisplayDiameter = GetStringParamValue(RevitRepository.OpeningDiameter);
        DisplayHeight = GetStringParamValue(RevitRepository.OpeningHeight);
        DisplayWidth = GetStringParamValue(RevitRepository.OpeningWidth);
        DisplayThickness = GetStringParamValue(RevitRepository.OpeningThickness);
        Comment = _familyInstance.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
        Username = GetStringParamValue(RevitRepository.OpeningAuthor);

        Diameter = GetDoubleParamValue(RevitRepository.OpeningDiameter);
        Height = GetDoubleParamValue(RevitRepository.OpeningHeight);
        Width = GetDoubleParamValue(RevitRepository.OpeningWidth);
        Thickness = GetDoubleParamValue(RevitRepository.OpeningThickness);

        string[] famNameParts = _familyInstance.Symbol.FamilyName.Split('_');
        if(famNameParts.Length > 0) {
            FamilyShortName = famNameParts.Last();
        }
    }


    public string FileName { get; }

    /// <summary>
    /// Id экземпляра семейства задания на отверстие
    /// </summary>
    public ElementId Id { get; }

    /// <summary>
    /// Точка расположения экземпляра семейства входящего задания на отверстие в координатах активного документа - получателя заданий
    /// </summary>
    public XYZ Location { get; }

    public string Date { get; }

    public string MepSystem { get; }

    public string Description { get; }

    public string CenterOffset { get; }

    public string BottomOffset { get; }

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
    /// Значение толщины в мм. Если толщины у отверстия нет, будет пустая строка.
    /// </summary>
    public string DisplayThickness { get; }

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
    /// Толщина в единицах ревита или 0, если толщины нет
    /// </summary>
    public double Thickness { get; } = 0;

    /// <summary>
    /// Трансформация связанного файла с заданием на отверстие относительно активного документа - получателя заданий
    /// </summary>
    public Transform Transform { get; }

    /// <summary>
    /// Короткое обозначение семейства задания на отверстие
    /// </summary>
    public string FamilyShortName { get; } = string.Empty;

    /// <summary>
    /// Элемент из активного документа, в котором расположено задание на отверстие из связи.
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingMepInfoUpdater"/></para>
    /// </summary>
    public Element Host { get; set; } = null;

    /// <summary>
    /// Комментарий экземпляра семейства задания на отверстие
    /// </summary>
    public string Comment { get; }

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Статус отработки задания на отверстие
    /// <para>Для обновления использовать <see cref="OpeningTaskIncomingMepInfoUpdater"/></para>
    /// </summary>
    public OpeningTaskIncomingStatus Status { get; set; } = OpeningTaskIncomingStatus.New;

    /// <summary>
    /// Тип проема
    /// </summary>
    public OpeningType OpeningType { get; }

    /// <summary>
    /// Угол поворота задания на отверстие в радианах в координатах активного файла, в который подгружена связь с заданием на отверстие
    /// </summary>
    public double Rotation { get; } = 0;

    public override bool Equals(object obj) {
        return (obj is OpeningMepTaskIncoming opening) && Equals(opening);
    }

    public override int GetHashCode() {
        return (int) (Id.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(OpeningMepTaskIncoming other) {
        return (other != null) && (Id == other.Id) && FileName.Equals(other.FileName);
    }


    public FamilyInstance GetFamilyInstance() {
        return _familyInstance;
    }

    /// <summary>
    /// Возвращает солид входящего задания на отверстие в координатах активного файла - получателя заданий на отверстия
    /// </summary>
    /// <exception cref="ArgumentException">Исключение, если солид элемента пустой</exception>
    public Solid GetSolid() {
        if(_solid is not null) {
            return _solid;
        }
        var famInstSolid = _familyInstance.GetSolid();
        if(famInstSolid?.GetVolumeOrDefault() > 0) {
            return _solid ??= SolidUtils.CreateTransformed(famInstSolid, Transform);
        } else {
            throw new ArgumentException(
                $"У задания на отверстие нет геометрии ID: {Id}, файл: \'{FileName}\'");
        }
    }

    /// <summary>
    /// Возвращает BBox в координатах активного документа-получателя заданий на отверстия, в который подгружены связи с заданиями
    /// </summary>
    public BoundingBoxXYZ GetTransformedBBoxXYZ() {
        // _familyInstance.GetBoundingBox().TransformBoundingBox(Transform);
        // при получении бокса сразу из экземпляра семейства
        // сначала строится бокс в координатах экземпляра семейства,
        // а потом строится описанный бокс в координатах проекта.
        //      B = b*cos(a) + b*sin(a), где
        //      (a) - угол поворота
        //      (B) - сторона описанного бокса в координатах проекта
        //      (b) - сторона вписанного бокса в координатах экземпляра семейства
        // Если семейство - вертикальный цилиндр, то это приведет к значительной погрешности.
        return GetSolid().GetTransformedBoundingBox();
    }

    /// <summary>
    /// Возвращает значение double параметра экземпляра семейства задания на отверстие в единицах ревита, или 0, если параметр отсутствует
    /// </summary>
    /// <param name="paramName">Название параметра</param>
    private double GetDoubleParamValue(string paramName) {
        return _familyInstance.IsExistsSharedParam(paramName)
            ? _familyInstance.GetSharedParamValueOrDefault<double>(paramName)
            : 0;
    }

    /// <summary>
    /// Возвращает значение параметра, или пустую строку, если параметра у семейства нет
    /// </summary>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    private string GetStringParamValue(string paramName) {
        return _familyInstance.IsExistsSharedParam(paramName)
            ? _familyInstance.GetSharedParam(paramName).AsValueString()
            : string.Empty;
    }
}
