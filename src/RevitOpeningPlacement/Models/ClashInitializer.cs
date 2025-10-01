using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.ClashDetection;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.FilterModel;

namespace RevitOpeningPlacement.Models;
internal class ClashInitializer {
    /// <summary>
    /// Находит коллизии между элементами ВИС из активного файла 
    /// и элементами конструкций (стены, перекрытия) из связей АР, КР, КМ
    /// </summary>
    /// <param name="revitRepository">Репозиторий активного документа ревита</param>
    /// <param name="mepFilter">Фильтр элементов ВИС</param>
    /// <param name="architectureFilter">Фильтр элементов конструкций</param>
    /// <returns>Перечисление коллизий элементов ВИС из активного файла с конструкциями из связей</returns>
    public static IEnumerable<ClashModel> GetClashes(
        RevitRepository revitRepository,
        Filter mepFilter,
        Filter architectureFilter,
        params ElementId[] mepElements) {

        var mainProvider = new FilterProvider(revitRepository.Doc, mepFilter, Transform.Identity, mepElements);
        int mainCount = mainProvider.GetElements().Count;
        if(mainCount == 0) {
            return Enumerable.Empty<ClashModel>();
        }
        var otherProviders = revitRepository
            .GetSelectedRevitLinks()
            .Select(item => new FilterProvider(item.GetLinkDocument(), architectureFilter, item.GetTransform()))
            .Where(item => item.GetElements().Count > 0)
            .ToList();
        if(otherProviders.Count == 0) {
            return Enumerable.Empty<ClashModel>();
        }
        var clashDetector = new ClashDetector(
            revitRepository.GetClashRevitRepository(),
            new[] { mainProvider },
            otherProviders);
        return clashDetector.FindClashes();
    }
}
