using System.Collections.Generic;
using System.Text.RegularExpressions;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;

namespace RevitClassifierParameters.Models.ConcreteParams;

/// <summary>
/// Заполняет параметры бетона у железобетонных элементов по всему проекту.
/// Порт Python-плагина "Параметры бетона":
/// - в типоразмер пишутся марки бетона B, F, W (числовые);
/// - в каждый экземпляр пишется тип материала (строковый).
/// Значения извлекаются из имени типоразмера.
/// Блоки записи (марки бетона и тип материала) выполняются независимо.
/// </summary>
internal class ConcreteParamsSetter {
    private const string _markBParamName = "обр_ФОП_Марка бетона B";
    private const string _markFParamName = "обр_ФОП_Марка бетона F";
    private const string _markWParamName = "обр_ФОП_Марка бетона W";
    private const string _materialTypeParamName = "ФОП_ТИП_Тип материала";

    // Отбираем элементы только с корректными наименованиями,
    // например "(ЖБ B30 ...)" или "(Б В7.5 ...)". Буква B может быть латиницей или кириллицей.
    private static readonly Regex _nameFilterRegex =
        new(@"(\(ЖБ|\(Б)( B| В)", RegexOptions.Compiled);

    private readonly RevitRepository _revitRepository;
    private readonly ILocalizationService _localizationService;
    private readonly ConcreteMarksNameParser _nameParser = new();

    public ConcreteParamsSetter(RevitRepository revitRepository, ILocalizationService localizationService) {
        _revitRepository = revitRepository;
        _localizationService = localizationService;
    }

    /// <summary>
    /// Заполняет параметры бетона у подходящих элементов проекта.
    /// </summary>
    /// <param name="writeMarks">Заполнять марки бетона B, F, W у типоразмера.</param>
    /// <param name="writeMaterialType">Заполнять тип материала у экземпляров.</param>
    public void SetConcreteParams(bool writeMarks, bool writeMaterialType) {
        if(!writeMarks && !writeMaterialType) {
            return;
        }

        var elementsByTypeName = GroupElementsByTypeName(_revitRepository.GetElementsForConcreteParams());
        if(elementsByTypeName.Count == 0) {
            return;
        }

        using var transaction = _revitRepository.Document.StartTransaction(
            _localizationService.GetLocalizedString("Transaction.SetConcreteParams"));

        foreach(var group in elementsByTypeName) {
            WriteValues(group.Key, group.Value, writeMarks, writeMaterialType);
        }

        transaction.Commit();
    }

    /// <summary>
    /// Отбирает подходящие элементы по имени типоразмера и группирует их по имени типоразмера.
    /// Проверка наличия конкретного параметра выполняется на этапе записи.
    /// </summary>
    private Dictionary<string, List<Element>> GroupElementsByTypeName(List<Element> elements) {
        var result = new Dictionary<string, List<Element>>();

        foreach(var element in elements) {
            string name;
            try {
                name = element.Name;
            } catch {
                continue;
            }

            if(string.IsNullOrEmpty(name) || !_nameFilterRegex.IsMatch(name)) {
                continue;
            }

            if(result.TryGetValue(name, out var list)) {
                list.Add(element);
            } else {
                result[name] = [element];
            }
        }

        return result;
    }

    /// <summary>
    /// Записывает марки бетона в типоразмер и/или тип материала в его экземпляры.
    /// Значения всегда перезаписываются.
    /// </summary>
    private void WriteValues(
        string typeName,
        List<Element> instances,
        bool writeMarks,
        bool writeMaterialType) {

        var marks = _nameParser.Parse(typeName);

        if(writeMarks) {
            WriteMarks(instances[0], marks);
        }

        if(writeMaterialType) {
            WriteMaterialType(instances, marks);
        }
    }

    /// <summary>
    /// Записывает марки бетона B, F, W в типоразмер элемента.
    /// </summary>
    private void WriteMarks(Element instance, ConcreteMarks marks) {
        var elementType = _revitRepository.Document.GetElement(instance.GetTypeId());
        if(elementType is null) {
            return;
        }

        try {
            elementType.SetParamValue(_markBParamName, marks.MarkB);
            elementType.SetParamValue(_markFParamName, marks.MarkF);
            elementType.SetParamValue(_markWParamName, marks.MarkW);
        } catch {
            // Если у типоразмера отсутствуют параметры марок бетона — пропускаем.
        }
    }

    /// <summary>
    /// Записывает тип материала в экземпляры типоразмера.
    /// </summary>
    private void WriteMaterialType(List<Element> instances, ConcreteMarks marks) {
        foreach(var instance in instances) {
            try {
                instance.SetParamValue(_materialTypeParamName, marks.MaterialType);
            } catch {
                // Пропускаем экземпляр, если параметр недоступен для записи.
            }
        }
    }
}
