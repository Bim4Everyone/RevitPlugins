using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Сервис работы с библиотекой семейств на диске: получение списка семейств в папке,
/// чтение типоразмеров из файла семейства и загрузка семейства в проект
/// </summary>
internal class FamilyLibraryService {
    private const string _familyExtension = ".rfa";
    private const string _partAtomStreamName = "PartAtom";
    private const string _partAtomNamespace = "urn:schemas-autodesk-com:partatom";
    private const string _atomNamespace = "http://www.w3.org/2005/Atom";
    private const string _revitGroupingScheme = "adsk:revit:grouping";

    // Файлы резервных копий Revit: Имя.0001.rfa
    private static readonly Regex _backupFileRegex = new(@"\.\d{4}$", RegexOptions.Compiled);

    private readonly RevitRepository _revitRepository;

    // Категории текущего документа по их локализованным именам, для распознавания категории из PartAtom
    private Dictionary<string, BuiltInCategory> _categoriesByName;

    public FamilyLibraryService(RevitRepository revitRepository) {
        _revitRepository = revitRepository;
    }

    /// <summary>
    /// Возвращает путь до файла семейства в папке
    /// </summary>
    public string GetFamilyPath(string folderPath, string familyName) {
        return Path.Combine(folderPath, familyName + _familyExtension);
    }

    /// <summary>
    /// Возвращает отсортированный список имен семейств (имен файлов .rfa без расширения) в папке.
    /// Поиск выполняется только в указанной папке, без вложенных. Резервные копии исключаются.
    /// </summary>
    /// <exception cref="IOException">Ошибка чтения папки</exception>
    /// <exception cref="UnauthorizedAccessException">Нет доступа к папке</exception>
    public List<string> GetFamilyNames(string folderPath) {
        return Directory.EnumerateFiles(folderPath, "*" + _familyExtension, SearchOption.TopDirectoryOnly)
            // Маска "*.rfa" в Windows также находит расширения, начинающиеся с ".rfa"
            .Where(path => string.Equals(Path.GetExtension(path), _familyExtension, StringComparison.OrdinalIgnoreCase))
            .Select(Path.GetFileNameWithoutExtension)
            .Where(name => !_backupFileRegex.IsMatch(name))
            .OrderBy(name => name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();
    }

    /// <summary>
    /// Получает сведения о семействе (категория и типоразмеры) без загрузки его в проект.
    /// PartAtom читается напрямую из файла, без вызовов Revit API, поэтому семейство
    /// не открывается и не обновляется до текущей версии Revit.
    /// </summary>
    /// <remarks>
    /// Файл читается при каждом вызове, результаты не сохраняются между вызовами
    /// </remarks>
    /// <returns>True, если сведения удалось получить</returns>
    public bool TryGetFamilyInfo(string familyPath, out FamilyFileInfo familyInfo) {
        familyInfo = null;
        if(string.IsNullOrWhiteSpace(familyPath) || !File.Exists(familyPath)) {
            return false;
        }

        familyInfo = ReadByPartAtom(familyPath) ?? ReadByOpeningDocument(familyPath);
        return familyInfo != null;
    }

    /// <summary>
    /// Загружает семейство в текущий документ с заменой. Должен вызываться внутри открытой транзакции.
    /// </summary>
    /// <returns>Загруженное семейство или null, если загрузить не удалось</returns>
    public Family LoadFamily(string familyPath) {
        if(string.IsNullOrWhiteSpace(familyPath) || !File.Exists(familyPath)) {
            return null;
        }

        try {
            _revitRepository.Document.LoadFamily(familyPath, new FamilyLoadOptions(), out Family family);
            // Если такое же семейство уже есть в проекте и не изменилось, Revit возвращает false и не отдает семейство,
            // поэтому дополнительно ищем его по имени
            return family ?? _revitRepository.GetFamilyByName(Path.GetFileNameWithoutExtension(familyPath));
        } catch(Exception) {
            return null;
        }
    }

    /// <summary>
    /// Чтение PartAtom напрямую из файла .rfa (без вызовов Revit API, семейство не открывается и не обновляется)
    /// </summary>
    private FamilyFileInfo ReadPartAtomFromFile(string familyPath) {
        try {
            byte[] data = CompoundFileReader.ReadStream(familyPath, _partAtomStreamName);
            if(data is null || data.Length == 0) {
                return null;
            }

            using var memoryStream = new MemoryStream(data);
            return ParsePartAtom(XDocument.Load(memoryStream), familyPath);
        } catch(Exception) {
            return null;
        }
    }

    /// <summary>
    /// Чтение сведений о семействе из PartAtom, извлеченного средствами Revit.
    /// Для семейств старых версий Revit открывает и обновляет файл.
    /// </summary>
    private FamilyFileInfo ReadByPartAtom(string familyPath) {
        string xmlPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        try {
            _revitRepository.Application.ExtractPartAtomFromFamilyFile(familyPath, xmlPath);
            if(!File.Exists(xmlPath)) {
                return null;
            }

            return ParsePartAtom(XDocument.Load(xmlPath), familyPath);
        } catch(Exception) {
            return null;
        } finally {
            try {
                if(File.Exists(xmlPath)) {
                    File.Delete(xmlPath);
                }
            } catch(Exception) { }
        }
    }

    /// <summary>
    /// Разбор XML PartAtom: категория и имена типоразмеров.
    /// Возвращает null, если категорию не удалось распознать или типоразмеров нет.
    /// </summary>
    private FamilyFileInfo ParsePartAtom(XDocument xml, string familyPath) {
        XNamespace atom = _atomNamespace;
        XNamespace partAtom = _partAtomNamespace;

        // Категория Revit хранится в <category><term>..</term><scheme>adsk:revit:grouping</scheme></category>
        // на языке Revit, в котором было сохранено семейство
        string categoryName = xml.Root?
            .Elements(atom + "category")
            .FirstOrDefault(c => (string) c.Element(atom + "scheme") == _revitGroupingScheme)
            ?.Element(atom + "term")
            ?.Value;

        // Категория не распознана, например семейство сохранено в Revit другого языка
        var builtInCategory = GetBuiltInCategory(categoryName);
        if(builtInCategory is null) {
            return null;
        }

        var typeNames = xml.Root?
            .Element(partAtom + "family")
            ?.Elements(partAtom + "part")
            .Select(p => p.Element(atom + "title")?.Value)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct()
            .ToList() ?? [];

        if(typeNames.Count == 0) {
            return null;
        }

        return new FamilyFileInfo(Path.GetFileNameWithoutExtension(familyPath), builtInCategory, typeNames);
    }

    /// <summary>
    /// Чтение сведений о семействе путем его открытия в фоне (без отображения в интерфейсе Revit).
    /// Для семейств старых версий Revit обновляет файл.
    /// </summary>
    private FamilyFileInfo ReadByOpeningDocument(string familyPath) {
        Document familyDocument = null;
        try {
            familyDocument = _revitRepository.Application.OpenDocumentFile(familyPath);
            if(familyDocument is null || !familyDocument.IsFamilyDocument) {
                return null;
            }

            string familyName = Path.GetFileNameWithoutExtension(familyPath);
            var builtInCategory = familyDocument.OwnerFamily?.FamilyCategory?.GetBuiltInCategory();

            var typeNames = familyDocument.FamilyManager.Types
                .Cast<FamilyType>()
                .Select(t => t.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct()
                .ToList();

            // Семейство без типоразмеров при загрузке получает один типоразмер с именем семейства
            if(typeNames.Count == 0) {
                typeNames.Add(familyName);
            }

            return new FamilyFileInfo(familyName, builtInCategory, typeNames);
        } catch(Exception) {
            return null;
        } finally {
            try {
                familyDocument?.Close(false);
                familyDocument?.Dispose();
            } catch(Exception) { }
        }
    }

    /// <summary>
    /// Переводит имя категории из PartAtom в BuiltInCategory. PartAtom хранит имя категории на языке той версии
    /// Revit, в которой семейство было сохранено, поэтому сопоставление идет с именами категорий текущего документа.
    /// Возвращает null, если категорию распознать не удалось
    /// </summary>
    private BuiltInCategory? GetBuiltInCategory(string categoryName) {
        if(string.IsNullOrWhiteSpace(categoryName)) {
            return null;
        }

        _categoriesByName ??= _revitRepository.Document.Settings.Categories
            .OfType<Category>()
            .GroupBy(c => c.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First().GetBuiltInCategory(), StringComparer.CurrentCultureIgnoreCase);

        return _categoriesByName.TryGetValue(categoryName, out var builtInCategory)
            ? builtInCategory
            : null;
    }
}
