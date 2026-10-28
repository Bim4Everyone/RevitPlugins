using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models.Checks;

/// <summary>
/// Данные одной модели, подготовленные для правил проверки: элементы ВОР, материалы, признаки модели.
/// </summary>
internal class ModelCheckContext {
    public const string BlockParameter = "ФОП_Блок СМР";
    public const string SectionParameter = "ФОП_Секция СМР";
    public const string FloorParameter = "ФОП_Этаж СМР";
    public const string LocationParameter = "ФОП_ТИП_Местоположение";
    public const string PurposeParameter = "ФОП_ТИП_Назначение";
    public const string MaterialTypeParameter = "ФОП_ТИП_Тип материала";
    public const string SizeParameter = "ФОП_Размер";
    public const string ReinforcementParameter = "ФОП_ТИП_Армирование";
    public const string AdditionalSortingParameter = "ФОП_ТИП_Доп. сортировка";

    /// <summary>
    /// Дата, с которой для монолита назначается материал «Бетон» (стадия П).
    /// </summary>
    private const string _concreteMaterialRuleDate = "2025-03-07";

    private static readonly HashSet<string> _concreteMaterialNames = new() {
        "бетон", "бетон с пенетроном", "железобетон", "железобетон с пенетроном"
    };

    private static readonly string[] _voidKeys = {
        "Отверстие", "Ниша", "Скошенный угол", "Модель грунта", "Проем", "Проём"
    };

    public ModelCheckContext(
        ModelData model,
        ClashesData clashes,
        WorkClassifier classifier,
        ProjectCard project,
        bool checkWorksets) {

        Model = model;
        Clashes = clashes;
        Classifier = classifier;
        Project = project;
        CheckWorksets = checkWorksets;
        FileName = Regex.Replace(model.File.Title ?? "", "_?отсоединено$", "");
        IsPitModel = Regex.IsMatch(FileName, "kotlovan|котлован", RegexOptions.IgnoreCase);
        string assignmentDate = project.AssignmentDate ?? _concreteMaterialRuleDate;
        IsConcreteMaterialRequired = string.CompareOrdinal(assignmentDate, _concreteMaterialRuleDate) >= 0;

        foreach(MaterialData material in model.Materials) {
            Materials[material.Id] = material;
        }

        foreach(ElementData element in model.Elements) {
            element.TypeName ??= "";
        }

        // решение BIM-отдела: элементы в РН «Скрытые» / «На скрытие» / «Вспомогательные» не участвуют в проверке
        ExcludedElementIds = new HashSet<long>(model.Elements.Where(IsInExcludedWorkset).Select(element => element.Id));
        Elements = model.Elements.Where(element => !ExcludedElementIds.Contains(element.Id)).ToList();
        VorElements = Elements
            .Where(element => GetVorMaterials(element).Count > 0
                              && !IsInExcludedWorkset(element)
                              && !IsOpeningOrVoid(element))
            .ToList();
        ConcreteElements = VorElements
            .Where(element => GetVorMaterials(element).Any(IsConcreteMaterial) && !IsInsulation(element))
            .ToList();
        UsedMaterialIds = new HashSet<long>(VorElements.SelectMany(element => GetVorMaterials(element)
            .Select(material => material.Id)));

        var blocks = PythonFormat.MostCommon(VorElements.Select(element => element.GetParameterOrEmpty(BlockParameter)));
        MainBlock = blocks.Count > 0 ? blocks[0].Key : "";
        UndergroundFloorCount = model.Levels
            .Select(level => level.Name)
            .Where(name => Regex.IsMatch(name, @"^П\d{2} этаж"))
            .Select(name => name.Split('_')[0])
            .Distinct()
            .Count();
    }

    /// <summary>
    /// Имя файла модели без суффикса «_отсоединено».
    /// </summary>
    public string FileName { get; }

    public ModelData Model { get; }

    /// <summary>
    /// Пересечения модели или null, если их не удалось выгрузить.
    /// </summary>
    public ClashesData Clashes { get; }

    public WorkClassifier Classifier { get; }

    public ProjectCard Project { get; }

    /// <summary>
    /// Проверять распределение элементов ВОР по рабочим наборам.
    /// </summary>
    public bool CheckWorksets { get; }

    /// <summary>
    /// Модель котлована: материалы ВОР определяются по имени, часть проверок не выполняется.
    /// </summary>
    public bool IsPitModel { get; }

    /// <summary>
    /// ЗнП выдано не ранее 07.03.2025: для монолита обязателен материал «Бетон».
    /// </summary>
    public bool IsConcreteMaterialRequired { get; }

    public Dictionary<long, MaterialData> Materials { get; } = new();

    /// <summary>
    /// Id элементов во вспомогательных и скрытых рабочих наборах — не проверяются.
    /// </summary>
    public HashSet<long> ExcludedElementIds { get; }

    /// <summary>
    /// Элементы модели без исключённых.
    /// </summary>
    public List<ElementData> Elements { get; }

    /// <summary>
    /// Элементы, попадающие в ВОР (есть материал с кодом «г02…»).
    /// </summary>
    public List<ElementData> VorElements { get; }

    /// <summary>
    /// Монолитные элементы ВОР (материал бетон / железобетон), кроме изоляции.
    /// </summary>
    public List<ElementData> ConcreteElements { get; }

    public HashSet<long> UsedMaterialIds { get; }

    /// <summary>
    /// Самое частое значение ФОП_Блок СМР у элементов ВОР.
    /// </summary>
    public string MainBlock { get; }

    public int UndergroundFloorCount { get; }

    /// <summary>
    /// Id элементов, попавших в замечания о коллизиях: по ним не дублируются предупреждения Revit.
    /// </summary>
    public HashSet<long> CollisionElementIds { get; } = new();

    public static bool IsConcreteMaterial(MaterialData material) {
        string name = PythonFormat.NormalizeText(material.Name).ToLowerInvariant();
        return _concreteMaterialNames.Contains(name) || PythonFormat.Trim(material.Keynote) == "г02";
    }

    public static bool IsInsulation(ElementData element) {
        return element.Category == "Соединения несущих конструкций" || element.TypeName.Contains("Изол");
    }

    public static bool IsThermalInsert(ElementData element) {
        return element.TypeName.Contains("Термовкладыш") || (element.FamilyName ?? "").Contains("Термовкладыш");
    }

    public static bool IsInExcludedWorkset(ElementData element) {
        string workset = (element.Workset ?? "").ToLowerInvariant();
        return workset.Contains("спомогат") || workset.Contains("скрыт");
    }

    /// <summary>
    /// Отверстия, ниши и прочие «вычитающие» элементы — не конструкции.
    /// </summary>
    public static bool IsOpeningOrVoid(ElementData element) {
        string names = element.TypeName + " " + (element.FamilyName ?? "");
        return _voidKeys.Any(names.Contains);
    }

    public static bool ContainsAny(string text, IEnumerable<string> fragments) {
        return fragments.Any(text.Contains);
    }

    /// <summary>
    /// Перечень типоразмеров элементов для текста замечания: первые <paramref name="maxCount"/> по алфавиту.
    /// </summary>
    public static string DescribeTypes(IEnumerable<ElementData> elements, int maxCount = 6) {
        var typeNames = PythonFormat.SortOrdinal(elements.Select(element => element.TypeName).Distinct());
        string more = typeNames.Count > maxCount ? " и др." : "";
        return string.Join(", ", typeNames.Take(maxCount)) + more;
    }

    /// <summary>
    /// Материалы элемента, которые попадают в ВОР.
    /// </summary>
    public List<MaterialData> GetVorMaterials(ElementData element) {
        IEnumerable<MaterialData> materials = element.GetMaterialIds()
            .Where(Materials.ContainsKey)
            .Select(id => Materials[id]);
        return IsPitModel
            ? materials.Where(material => (material.Name ?? "").Contains("г02")).ToList()
            : materials.Where(material => (material.Keynote ?? "").StartsWith("г02", StringComparison.Ordinal)).ToList();
    }
}
