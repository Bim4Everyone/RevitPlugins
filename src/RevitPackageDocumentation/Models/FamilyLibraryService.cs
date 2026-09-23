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
    private const string _partAtomNamespace = "urn:schemas-autodesk-com:partatom";
    private const string _atomNamespace = "http://www.w3.org/2005/Atom";
    private const string _revitGroupingScheme = "adsk:revit:grouping";

    // Файлы резервных копий Revit: Имя.0001.rfa
    private static readonly Regex _backupFileRegex = new(@"\.\d{4}$", RegexOptions.Compiled);

    private readonly RevitRepository _revitRepository;

    // Кэш сведений о файлах семейств на время сеанса плагина. Ключ - полный путь до файла
    private readonly Dictionary<string, (DateTime LastWriteTime, FamilyFileInfo Info)> _familyInfoCache =
        new(StringComparer.OrdinalIgnoreCase);

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
    /// Сначала выполняется попытка чтения PartAtom, если не удалось - семейство открывается в фоне и закрывается.
    /// </summary>
    /// <returns>True, если сведения удалось получить</returns>
    public bool TryGetFamilyInfo(string familyPath, out FamilyFileInfo familyInfo) {
        familyInfo = null;
        if(string.IsNullOrWhiteSpace(familyPath) || !File.Exists(familyPath)) {
            return false;
        }

        DateTime lastWriteTime = File.GetLastWriteTimeUtc(familyPath);
        if(_familyInfoCache.TryGetValue(familyPath, out var cached) && cached.LastWriteTime == lastWriteTime) {
            familyInfo = cached.Info;
            return true;
        }

        familyInfo = ReadByPartAtom(familyPath) ?? ReadByOpeningDocument(familyPath);
        if(familyInfo is null) {
            return false;
        }

        _familyInfoCache[familyPath] = (lastWriteTime, familyInfo);
        return true;
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
            family ??= _revitRepository.GetFamilyByName(Path.GetFileNameWithoutExtension(familyPath));
            if(family != null) {
                _revitRepository.RaiseFamilySymbolsChanged();
            }
            return family;
        } catch(Exception) {
            return null;
        }
    }

    /// <summary>
    /// Чтение сведений о семействе из PartAtom (XML, хранящийся в файле семейства) без открытия семейства
    /// </summary>
    private FamilyFileInfo ReadByPartAtom(string familyPath) {
        string xmlPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.xml");
        try {
            _revitRepository.Application.ExtractPartAtomFromFamilyFile(familyPath, xmlPath);
            if(!File.Exists(xmlPath)) {
                return null;
            }

            var xml = XDocument.Load(xmlPath);
            XNamespace atom = _atomNamespace;
            XNamespace partAtom = _partAtomNamespace;

            // Категория Revit хранится в <category><term>..</term><scheme>adsk:revit:grouping</scheme></category>
            // на языке Revit, в котором было сохранено семейство
            string categoryName = xml.Root?
                .Elements(atom + "category")
                .FirstOrDefault(c => (string) c.Element(atom + "scheme") == _revitGroupingScheme)
                ?.Element(atom + "term")
                ?.Value;

            // Если категорию не удалось распознать, например семейство сохранено в Revit другого языка,
            // - переходим к открытию семейства, где категория известна точно
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

            // Если типоразмеров в PartAtom нет - перепроверяем через открытие семейства
            if(typeNames.Count == 0) {
                return null;
            }

            return new FamilyFileInfo(Path.GetFileNameWithoutExtension(familyPath), builtInCategory, typeNames);
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
    /// Чтение сведений о семействе путем его открытия в фоне (без отображения в интерфейсе Revit)
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
