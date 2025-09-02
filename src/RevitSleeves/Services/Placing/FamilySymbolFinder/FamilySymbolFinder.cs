using System;
using System.Linq;

using Autodesk.Revit.DB;

using RevitSleeves.Models;
using RevitSleeves.Services.Core;

namespace RevitSleeves.Services.Placing.FamilySymbolFinder;
/// <summary>
/// Провайдер типоразмеров семейств гильз из активного документа
/// </summary>
internal class FamilySymbolFinder : IFamilySymbolFinder {
    private FamilySymbol _symbol;
    private readonly RevitRepository _repository;

    public FamilySymbolFinder(RevitRepository repository) {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }


    public FamilySymbol GetFamilySymbol() {
        var document = _repository.Document;
        return _symbol ??= (FamilySymbol) document.GetElement(
            GetFamilySymbol(document, NamesProvider.FamilyNameSleeve, NamesProvider.SleeveSymbolName));
    }

    /// <summary>
    /// Возвращает заданный типоразмер из заданного семейства из заданного документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <param name="familyName">Название семейства</param>
    /// <param name="symbolName">Название типоразмера</param>
    /// <returns>Id типоразмера семейства</returns>
    private ElementId GetFamilySymbol(Document doc, string familyName, string symbolName) {
        var familyId = GetFamily(doc, familyName);
        return new FilteredElementCollector(doc)
            .WherePasses(new FamilySymbolFilter(familyId))
            .First(s => s.Name.Equals(symbolName, StringComparison.InvariantCultureIgnoreCase))
            .Id;
    }

    /// <summary>
    /// Возвращает id семейства с заданным названием из заданного документа
    /// </summary>
    /// <param name="doc">Документ</param>
    /// <param name="familyName">Название семейства</param>
    /// <returns>Id семейства</returns>
    private ElementId GetFamily(Document doc, string familyName) {
        return new FilteredElementCollector(doc)
            .OfClass(typeof(Family))
            .First(family => family.Name.Equals(familyName, StringComparison.InvariantCultureIgnoreCase))
            .Id;
    }
}
