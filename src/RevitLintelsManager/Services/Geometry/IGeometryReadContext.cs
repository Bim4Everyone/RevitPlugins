using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

/// <summary>Геометрические ресурсы одного чтения в API-контексте исходного документа.</summary>
internal interface IGeometryReadContext : IDisposable {
    Document Document { get; }

    /// <summary>Получает мировые границы элемента в текущем поколении.</summary>
    /// <param name="owner">Владелец геометрии.</param>
    /// <returns>Границы для предварительного отбора или null.</returns>
    GeometryBounds GetBounds(Element owner);

    /// <summary>Вычисляет общие границы элемента и владельцев его вложенной геометрии.</summary>
    /// <param name="element">Проверяемая конструкция.</param>
    /// <returns>Охватывающие границы или null при неполных данных.</returns>
    GeometryBounds GetOwnerEnvelope(Element element);

    /// <summary>Пересекает конечный отрезок с внутренностью тел элемента.</summary>
    /// <param name="candidate">Элемент исходного документа.</param>
    /// <param name="start">Начало отрезка.</param>
    /// <param name="end">Конец отрезка.</param>
    /// <returns>Интервалы в футах, пустой список при промахе или null при недоступной геометрии.</returns>
    IReadOnlyList<MaterialInterval> Intersect(Element candidate, XYZ start, XYZ end);

    /// <summary>Определяет физическую систему координат прямой вертикальной стены.</summary>
    /// <param name="wall">Стена-хост проёма.</param>
    /// <returns>Локальная система или null для неподдерживаемой стены.</returns>
    WallFrame GetWallFrame(Wall wall);

    /// <summary>Читает плоскости, определяющие начало загруженного семейства.</summary>
    /// <param name="family">Определение семейства.</param>
    /// <returns>Имена трёх плоскостей или null при неопределённой привязке.</returns>
    (string X, string Y, string Z)? GetOriginPlanes(Family family);

    /// <summary>Читает имя плоскости начала с нормалью по оси ширины семейства.</summary>
    /// <param name="family">Определение семейства.</param>
    /// <returns>Имя подтверждённой плоскости или null.</returns>
    string GetWidthOriginPlane(Family family);

    /// <summary>Индексирует грани вырезов хоста по породившим их вставкам.</summary>
    /// <param name="wall">Стена-хост проёма.</param>
    /// <returns>Грани по идентификатору вставки или null при недоступном теле стены.</returns>
    ILookup<ElementId, Face> GetRevealFaces(Wall wall);
}
