using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.DB;

using RevitVORChecker.Models.Data;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Открытие модели из папки для проверки: в фоне, с отсоединением от хранилища, без связей. Модель не сохраняется.
/// </summary>
/// <remarks>
/// Порядок открытия (требование BIM-отдела): рабочие наборы связей закрыты → все связи RVT удаляются →
/// работа с остальными рабочими наборами.
/// </remarks>
internal static class DetachedModelOpener {
    /// <summary>
    /// Открывает модель и удаляет связи RVT (в открытой копии).
    /// </summary>
    /// <param name="application">Приложение Revit.</param>
    /// <param name="path">Путь к файлу .rvt.</param>
    /// <param name="removedLinks">Сведения об удалённых связях (для проверки размещения связей).</param>
    public static Document Open(Application application, string path, out List<LinkData> removedLinks) {
        BasicFileInfo fileInfo = BasicFileInfo.Extract(path);
        ModelPath modelPath = ModelPathUtils.ConvertUserVisiblePathToModelPath(path);
        var openOptions = new OpenOptions { Audit = false };
        if(fileInfo.IsWorkshared) {
            if(fileInfo.IsCentral || fileInfo.IsLocal) {
                openOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            }

            var worksetConfiguration = new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets);
            worksetConfiguration.Open(GetNonLinkWorksetIds(application, modelPath, openOptions));
            openOptions.SetOpenWorksetsConfiguration(worksetConfiguration);
        }

        Document document = application.OpenDocumentFile(modelPath, openOptions);
        try {
            removedLinks = RemoveLinks(document);
        } catch(Exception) {
            // вызывающий код не получит документ и не закроет его — закрываем здесь
            document.Close(false);
            throw;
        }

        return document;
    }

    /// <summary>
    /// Рабочие наборы модели, кроме наборов связей.
    /// </summary>
    private static List<WorksetId> GetNonLinkWorksetIds(
        Application application,
        ModelPath modelPath,
        OpenOptions mainOpenOptions) {

        try {
            return WorksharingUtils.GetUserWorksetInfo(modelPath)
                .Where(workset => !IsLinkWorkset(workset.Name))
                .Select(workset => workset.Id)
                .ToList();
        } catch(Exception) {
            // хранилище недоступно (локальная копия) — открываем с закрытыми наборами и читаем список наборов
            mainOpenOptions.DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets;
            var openOptions = new OpenOptions {
                Audit = false,
                DetachFromCentralOption = DetachFromCentralOption.DetachAndPreserveWorksets
            };
            openOptions.SetOpenWorksetsConfiguration(new WorksetConfiguration(WorksetConfigurationOption.CloseAllWorksets));
            Document document = application.OpenDocumentFile(modelPath, openOptions);
            try {
                return new FilteredWorksetCollector(document)
                    .OfKind(WorksetKind.UserWorkset)
                    .Where(workset => !IsLinkWorkset(workset.Name))
                    .Select(workset => workset.Id)
                    .ToList();
            } finally {
                document.Close(false);
            }
        }
    }

    private static bool IsLinkWorkset(string name) {
        string lowerName = (name ?? "").ToLowerInvariant();
        return lowerName.Contains("связ") || lowerName.Contains("rvt") || lowerName.Contains("dwg") || lowerName.Contains("link");
    }

    private static List<LinkData> RemoveLinks(Document document) {
        var linkTypes = new FilteredElementCollector(document)
            .OfClass(typeof(RevitLinkType))
            .Cast<RevitLinkType>()
            .ToList();
        WorksetTable worksets = document.IsWorkshared ? document.GetWorksetTable() : null;
        var links = new List<LinkData>();
        foreach(RevitLinkType linkType in linkTypes) {
            var instances = new FilteredElementCollector(document)
                .OfClass(typeof(RevitLinkInstance))
                .Cast<RevitLinkInstance>()
                .Where(instance => instance.GetTypeId() == linkType.Id)
                .ToList();
            links.Add(new LinkData {
                Name = linkType.Name,
                IsLoaded = null,
                Attachment = linkType.AttachmentType.ToString(),
                InstanceCount = instances.Count,
                InstanceWorksets = instances
                    .Select(instance => worksets?.GetWorkset(instance.WorksetId)?.Name ?? "")
                    .ToList(),
                RemovedOnOpen = true
            });
        }

        if(linkTypes.Count > 0) {
            using var transaction = new Transaction(document, "Проверка ВОР: удаление связей");
            transaction.Start();
            document.Delete(linkTypes.Select(linkType => linkType.Id).ToList());
            transaction.Commit();
        }

        return links;
    }
}
