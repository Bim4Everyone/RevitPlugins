using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitClassifierParameters.Models.FacadeType;

/// <summary>
/// Заполняет параметр типа фасада у стен проекта по правилам, прочитанным из Excel.
/// Логика сопоставления имени типоразмера стены с правилами и записи значений
/// сосредоточена в этом классе.
/// </summary>
internal class FacadeTypeSetter {
    private readonly RevitRepository _revitRepository;
    private readonly FacadeTypeNameParser _facadeTypeNameParser = new();

    public FacadeTypeSetter(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
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

        var rules = BuildRules(facadeTypes);

        using var transaction = _revitRepository.Document.StartTransaction("Заполнение типа фасада");

        foreach(var wall in _revitRepository.GetWalls()) {
            string typeName = _revitRepository.Document.GetElement(wall.GetTypeId())?.Name ?? string.Empty;
            var parts = _facadeTypeNameParser.Parse(typeName);
            if(!parts.IsValid) {
                continue;
            }

            var key = (Normalize(parts.Function), Normalize(parts.Material));
            if(!rules.TryGetValue(key, out string value)) {
                continue;
            }

            var param = wall.LookupParameter(paramName);
            if(param == null || param.IsReadOnly || param.StorageType != StorageType.String) {
                continue;
            }

            if(param.AsString() != value) {
                param.Set(value);
            }
        }

        transaction.Commit();
    }

    /// <summary>
    /// Строит словарь для быстрого поиска значения по паре
    /// (характеристика функции, сокращение материала).
    /// </summary>
    private Dictionary<(string Function, string Material), string> BuildRules(IReadOnlyList<FacadeTypeItem> facadeTypes) {
        var map = new Dictionary<(string, string), string>();
        foreach(var item in facadeTypes) {
            var key = (Normalize(item.FunctionCharacteristic), Normalize(item.MaterialAbbreviation));
            map[key] = item.Value;
        }
        return map;
    }

    private static string Normalize(string value) {
        return (value ?? string.Empty).Trim().ToUpperInvariant();
    }
}
