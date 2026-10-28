using System.IO;

using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Project;

/// <summary>
/// Хранение данных проекта в папке с моделями.
/// </summary>
internal static class ProjectCardStorage {
    public const string FileName = "Данные проекта.json";

    /// <summary>
    /// Подпапка папки с моделями для реестра, журнала и данных проекта.
    /// </summary>
    public const string OutputFolderName = "Локальная проверка";

    public static string GetPath(string modelsFolder) {
        return Path.Combine(modelsFolder, OutputFolderName, FileName);
    }

    /// <summary>
    /// Данные проекта папки или null, если их нет или файл не читается.
    /// </summary>
    public static ProjectCard Load(string modelsFolder) {
        try {
            string path = GetPath(modelsFolder);
            return File.Exists(path) ? JsonFiles.Load<ProjectCard>(path) : null;
        } catch(Exception) {
            return null;
        }
    }

    public static void Save(string modelsFolder, ProjectCard card) {
        card.SavedAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        JsonFiles.Save(GetPath(modelsFolder), card, indented: true);
    }

    /// <summary>
    /// Новые данные проекта с типовыми нормами армирования для 10–17-этажных корпусов.
    /// </summary>
    public static ProjectCard CreateDefault() {
        return new ProjectCard {
            AssignmentDate = DateTime.Today.ToString("yyyy-MM-dd"),
            Storeys = StoreysOptions.GetTitle(StoreysOption.From10To17),
            ReinforcementRules = ReinforcementNorms.CreateDefaultRules(StoreysOption.From10To17),
            ReinforcementTable = ReinforcementNorms.ToTableValues(ReinforcementNorms.CreateDefaultTable())
        };
    }
}
