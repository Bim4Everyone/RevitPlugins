using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models;

/// <summary>
/// Файлы папки с моделями КР.
/// </summary>
internal static class ModelsFolder {
    /// <summary>
    /// Модели для проверки: имя модели → путь. Без резервных копий (*.0001.rvt) и координационных моделей.
    /// </summary>
    public static OrderedMap<string, string> GetModels(string folder) {
        var models = new OrderedMap<string, string>();
        var paths = Directory.GetFiles(folder, "*.rvt")
            .Where(path => !Regex.IsMatch(path, @"\.\d{4}\.rvt$", RegexOptions.IgnoreCase)
                           && !Regex.IsMatch(Path.GetFileName(path), "KOORD|КООРД", RegexOptions.IgnoreCase))
            .OrderBy(path => path, StringComparer.Ordinal);
        foreach(string path in paths) {
            models[Path.GetFileNameWithoutExtension(path)] = path;
        }

        return models;
    }

    /// <summary>
    /// Классификатор видов работ: указанный файл, файл «*лассификатор*.xlsx» в папке или файл из данных проекта.
    /// </summary>
    public static string? FindClassifier(string folder, string? classifierPath, ProjectCard project) {
        if(classifierPath is { } path && !string.IsNullOrWhiteSpace(path)) {
            return path.Trim();
        }

        return Directory.GetFiles(folder, "*.xlsx").FirstOrDefault(path => Path.GetFileName(path).Contains("лассификатор"))
               ?? project?.ClassifierFile;
    }

    /// <summary>
    /// Папка для реестра, журнала и данных проекта.
    /// </summary>
    public static string GetOutputFolder(string folder) {
        return Path.Combine(folder, ProjectCardStorage.OutputFolderName);
    }
}
