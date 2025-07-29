using System.Collections.Generic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class BboxViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ICollection<ElementModel> _elements;
    private readonly double _additionalSize;

    /// <summary>
    /// Конструктор класса для настроек 3D подрезки вида
    /// </summary>
    /// <param name="revitRepository">Репозиторий</param>
    /// <param name="elements">Элементы, по которым надо сделать 3D подрезку</param>
    /// <param name="additionalSize">Добавочный размер в футах, 
    /// на который нужно увеличить бокс в каждом направлении: OX, OY, OZ</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public BboxViewSettings(
        RevitRepository revitRepository,
        ICollection<ElementModel> elements,
        double additionalSize) {

        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _elements = elements ?? throw new System.ArgumentNullException(nameof(elements));
        _additionalSize = additionalSize;
        if(elements.Count == 0) {
            throw new System.ArgumentOutOfRangeException(nameof(elements));
        }
    }


    public void Apply(View3D view3D) {
        var bbox = _revitRepository.GetCommonBoundingBox(_elements);
        _revitRepository.SetSectionBox(bbox, view3D, _additionalSize);
    }
}
