using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitOpeningPlacement.Models.RevitViews;
/// <summary>
/// Настройки 3D вида для подрезки вида по габаритам заданных элементов
/// </summary>
internal class SectionBoxViewSettings : IView3DSetting {
    private readonly RevitClashDetective.Models.RevitRepository _clashRepository;
    private readonly ICollection<ElementModel> _elements;
    private readonly double _additionalSize;

    /// <summary>
    /// Конструктор настроек 3D вида для подрезки вида по габаритам заданных элементов
    /// </summary>
    /// <param name="clashRepository">Репозиторий, строящий габарит элементов и подрезку вида</param>
    /// <param name="elements">Элементы, по габаритам которых нужно подрезать вид</param>
    /// <param name="additionalSize">Отступ подрезки от габарита элементов в единицах Revit</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public SectionBoxViewSettings(
        RevitClashDetective.Models.RevitRepository clashRepository,
        ICollection<ElementModel> elements,
        double additionalSize) {

        _clashRepository = clashRepository ?? throw new ArgumentNullException(nameof(clashRepository));
        _elements = elements ?? throw new ArgumentNullException(nameof(elements));
        _additionalSize = additionalSize;
    }

    public void Apply(View3D view3D) {
        _clashRepository.SetSectionBox(_clashRepository.GetCommonBoundingBox(_elements), view3D, _additionalSize);
    }
}
