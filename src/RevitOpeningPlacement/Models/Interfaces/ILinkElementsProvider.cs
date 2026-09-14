using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.Interfaces;

/// <summary>
/// Интерфейс, представляющий обертку над связанным файлом ревита.
/// </summary>
internal interface ILinkElementsProvider {
    /// <summary>
    /// Документ связанного файла
    /// </summary>
    Document Document { get; }

    /// <summary>
    /// Трансформация <see cref="Document">связанного файла</see> относительно активного документа ревита
    /// </summary>
    Transform DocumentTransform { get; }

    /// <summary>
    /// Возвращает солид из координат активного документа в координатах связанного файла
    /// </summary>
    /// <param name="link">Связанный файл</param>
    /// <param name="solidInActiveDoc">Солид в координатах активного документа</param>
    Solid ToLinkCoordinates(Solid solidInActiveDoc);

    /// <summary>
    /// Возвращает бокс из координат активного документа в координатах связанного файла
    /// </summary>
    /// <param name="link">Связанный файл</param>
    /// <param name="bboxInActiveDoc">Бокс в координатах активного документа</param>
    BoundingBoxXYZ ToLinkCoordinates(BoundingBoxXYZ bboxInActiveDoc);

    /// <summary>
    /// Возвращает солид из координат связанного файла в координатах активного документа
    /// </summary>
    /// <param name="link">Связанный файл</param>
    /// <param name="solidInLink">Солид в координатах связанного файла</param>
    Solid ToActiveDocCoordinates(Solid solidInLink);

    /// <summary>
    /// Возвращает бокс из координат связанного файла в координатах активного документа
    /// </summary>
    /// <param name="link">Связанный файл</param>
    /// <param name="bboxInLink">Бокс в координатах связанного файла</param>
    BoundingBoxXYZ ToActiveDocCoordinates(BoundingBoxXYZ bboxInLink);
}
