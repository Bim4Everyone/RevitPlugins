using System.IO;
using System.Text.RegularExpressions;

using RevitVORChecker.Core;
using RevitVORChecker.Models.Checks.Rules;
using RevitVORChecker.Models.Data;
using RevitVORChecker.Models.Findings;
using RevitVORChecker.Models.Project;

namespace RevitVORChecker.Models.Checks;

/// <summary>
/// Проверка выгруженных моделей КР по правилам ВОР (разделы 1–19 реестра).
/// </summary>
internal static class ModelChecker {
    /// <summary>
    /// Проверяет модели рабочей папки.
    /// </summary>
    /// <param name="modelFiles">Файлы model_*.json.</param>
    /// <param name="clashFiles">Файлы clashes_*.json.</param>
    /// <param name="classifier">Классификатор видов работ.</param>
    /// <param name="project">Данные проекта.</param>
    /// <param name="checkWorksets">Проверять распределение элементов ВОР по рабочим наборам.</param>
    public static List<Finding> Check(
        IEnumerable<string> modelFiles,
        IEnumerable<string> clashFiles,
        WorkClassifier classifier,
        ProjectCard project,
        bool checkWorksets) {

        var findings = new FindingCollection();
        var clashesByModel = new Dictionary<string, ClashesData>();
        foreach(string clashFile in clashFiles) {
            ClashesData clashes = JsonFiles.Load<ClashesData>(clashFile);
            clashesByModel[clashes.FileTitle ?? ""] = clashes;
        }

        foreach(string modelFile in modelFiles) {
            ModelData model = LoadModel(modelFile);
            clashesByModel.TryGetValue(model.File.Title ?? "", out ClashesData clashes);
            var context = new ModelCheckContext(model, clashes, classifier, project, checkWorksets);
            CheckModel(context, findings);
        }

        return findings.ToList();
    }

    /// <summary>
    /// Правила в порядке разделов реестра: порядок добавления замечаний важен.
    /// </summary>
    public static void CheckModel(ModelCheckContext context, FindingCollection findings) {
        CollisionRules.Check(context, findings);
        IntegrityRules.Check(context, findings);
        TypeNameRules.Check(context, findings);
        GeneralRules.Check(context, findings);
        SmrParameterRules.Check(context, findings);
        MaterialRules.Check(context, findings);
        LocationAndPurposeRules.Check(context, findings);
        TypeParameterRules.Check(context, findings);
        ReinforcementRules.Check(context, findings);
    }

    /// <summary>
    /// Читает данные модели; связи берутся из links_*.json, записанного при открытии модели (связи удаляются).
    /// </summary>
    public static ModelData LoadModel(string modelFile) {
        ModelData model = JsonFiles.Load<ModelData>(modelFile);
        string modelName = Regex.Replace(model.File.Title ?? "", "_?отсоединено$", "");
        string linksFile = Path.Combine(
            Path.GetDirectoryName(Path.GetFullPath(modelFile)),
            "links_" + modelName + ".json");
        if(File.Exists(linksFile)) {
            model.Links = JsonFiles.Load<List<LinkData>>(linksFile);
        }

        return model;
    }
}
