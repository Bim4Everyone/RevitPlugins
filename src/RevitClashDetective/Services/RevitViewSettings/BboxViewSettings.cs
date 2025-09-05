using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit.Geometry;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Interfaces;

namespace RevitClashDetective.Services.RevitViewSettings;

internal class BboxViewSettings : IView3DSetting {
    private readonly RevitRepository _revitRepository;
    private readonly ICollection<ElementModel> _elements;
    private readonly SettingsConfig _config;

    /// <summary>
    /// Конструктор класса для настроек 3D подрезки вида
    /// </summary>
    /// <param name="revitRepository">Репозиторий</param>
    /// <param name="elements">Элементы, по которым надо сделать 3D подрезку</param>
    /// <param name="config">Настройки</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    /// <exception cref="System.ArgumentOutOfRangeException"></exception>
    public BboxViewSettings(
        RevitRepository revitRepository,
        ICollection<ElementModel> elements,
        SettingsConfig config) {

        _revitRepository = revitRepository ?? throw new System.ArgumentNullException(nameof(revitRepository));
        _elements = elements ?? throw new System.ArgumentNullException(nameof(elements));
        _config = config ?? throw new System.ArgumentNullException(nameof(config));
        if(elements.Count == 0) {
            throw new System.ArgumentOutOfRangeException(nameof(elements));
        }
    }


    public void Apply(View3D view3D) {
        BoundingBoxXYZ bbox;
        if(_config.SectionBoxModeSettings == SectionBoxMode.AroundElements) {
            bbox = _revitRepository.GetBoundingBoxes(_elements).CreateUnitedBoundingBox();
        } else {
            bbox = _revitRepository.GetBoundingBoxes(_elements).ToArray().CreateCommonBoundingBox();
        }
        _revitRepository.SetSectionBox(
            bbox,
            view3D,
            _revitRepository.ConvertToInternal(_config.SectionBoxOffset));
    }
}
