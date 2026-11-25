using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitLintelsManager.Models;

/// <summary>Геометрические факты для сопоставления проёма и перемычки в текущем чтении.</summary>
internal interface IPlacementGeometryReader {
    /// <summary>Определяет плоскости привязки и ориентацию перемычки.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="instance">Экземпляр исходного документа.</param>
    /// <returns>Факты привязки с причиной возможной недоступности.</returns>
    GeometryFacts ReadLintelGeometry(IGeometryReadContext geometry, FamilyInstance instance);

    /// <summary>Читает фактический вырез хоста и сопоставляет его с параметрами.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="instance">Экземпляр исходного документа.</param>
    /// <param name="widthMm">Ширина из сопоставленного параметра в миллиметрах.</param>
    /// <param name="heightMm">Высота из сопоставленного параметра в миллиметрах.</param>
    /// <returns>Границы выреза и ориентация или причина недоступности.</returns>
    GeometryFacts ReadOpeningGeometry(IGeometryReadContext geometry, FamilyInstance instance,
        double? widthMm, double? heightMm);

    /// <summary>Проверяет вырез экземпляра в соседних стенах при отсутствии его граней в стене-хосте.</summary>
    /// <param name="geometry">Общие геометрические ресурсы текущего чтения.</param>
    /// <param name="instance">Экземпляр проёма исходного документа.</param>
    /// <param name="widthMm">Сопоставленная ширина проёма в миллиметрах.</param>
    /// <param name="heightMm">Сопоставленная высота проёма в миллиметрах.</param>
    /// <param name="candidates">Соседние стены, допущенные по стадии и типу.</param>
    /// <returns>Единственная стена с подтверждённым вырезом экземпляра и геометрия выреза.</returns>
    (Wall Wall, GeometryFacts Geometry) ReadAdjacentOpeningGeometry(IGeometryReadContext geometry,
        FamilyInstance instance, double? widthMm, double? heightMm, IEnumerable<Wall> candidates);
}
