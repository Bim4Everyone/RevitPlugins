using System;

using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Models.RealOpeningArPlacement.Providers;
/// <summary>
/// Класс, предоставляющий типоразмер семейства прямоугольного чистового отверстия на основе хоста. Использовать для получения типоразмера чистового отверстия для размещения по нескольким заданиям на отверстия
/// </summary>
internal class RectangleFamilySymbolProvider {
    private readonly RevitRepository _revitRepository;
    private readonly Element _host;


    /// <summary>
    /// Конструктор класса, предоставляющего типоразмер семейства прямоугольного чистового отверстия на основе хоста
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа Revit, в котором будет происходить расстановка чистовых отверстий</param>
    /// <param name="host">Хост для чистового отверстия - стена или перекрытие</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public RectangleFamilySymbolProvider(RevitRepository revitRepository, Element host) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _host = host ?? throw new ArgumentNullException(nameof(host));
    }


    /// <summary>
    /// Возвращает типоразмер семейства прямоугольного чистового отверстия
    /// </summary>
    /// <exception cref="ArgumentException">Исключение, если <see cref="_host"/> не стена или перекрытие</exception>
    public FamilySymbol GetFamilySymbol() {
        return _host is Wall
            ? _revitRepository.GetOpeningRealArType(OpeningType.WallRectangle)
            : _host is Floor
                ? _revitRepository.GetOpeningRealArType(OpeningType.FloorRectangle)
                : throw new ArgumentException(nameof(_host));
    }
}
