using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Заполняет параметр типа фасада у стен проекта по правилам, прочитанным из Excel.
/// Логика сопоставления имени типоразмера стены с правилами и записи значений
/// сосредоточена в этом классе.
/// </summary>
internal class FacadeTypeSetter {
    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly FacadeTypeNameParser _facadeTypeNameParser = new();

    public FacadeTypeSetter(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Перебирает стены проекта и, если имя типоразмера содержит на нужных местах
    /// характеристику функции и сокращение основного материала из файла правил,
    /// записывает соответствующее значение в параметр с именем <paramref name="paramName"/>.
    /// </summary>
    /// <param name="paramName">Имя параметра стены, в который записывается тип фасада.</param>
    /// <param name="facadeTypes">Правила заполнения типа фасада, прочитанные из Excel.</param>
    public void SetFacadeType(string paramName, IReadOnlyList<FacadeTypeItem> facadeTypes) {
        if(string.IsNullOrEmpty(paramName) || facadeTypes is null || facadeTypes.Count == 0) {
            return;
        }

        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("Transaction.SetFacadeType"));

        foreach(var wall in _revitRepository.GetWalls()) {
            string typeName = _revitRepository.Document.GetElement(wall.GetTypeId())?.Name ?? string.Empty;
            var parts = _facadeTypeNameParser.Parse(typeName);
            if(!parts.IsValid) {
                continue;
            }

            // Ключевые поля правил уже нормализованы при чтении из Excel,
            // поэтому здесь нормализуем только подстроки, извлечённые из имени типоразмера стены.
            string function = parts.Function.Trim().ToUpperInvariant();
            string material = parts.Material.Trim().ToUpperInvariant();

            var rule = facadeTypes.FirstOrDefault(x =>
                x.FunctionCharacteristic == function
                && x.MaterialAbbreviation == material);
            if(rule is null) {
                continue;
            }

            var param = wall.LookupParameter(paramName);
            if(param == null || param.IsReadOnly || param.StorageType != StorageType.String) {
                continue;
            }

            if(param.AsString() != rule.Value) {
                param.Set(rule.Value);
            }
        }

        transaction.Commit();
    }
}
