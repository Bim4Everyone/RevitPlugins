using System.IO;
using System.Text;

using Autodesk.Revit.DB;

using RevitVORChecker.Models.Report;

namespace RevitVORChecker.Models.Revit;

/// <summary>
/// Скриншоты элементов замечаний: временный 3D-вид, подрезка по элементам, элементы — красным, остальное — полупрозрачно.
/// </summary>
/// <remarks>
/// Все изменения делаются в транзакции, которая откатывается: в модели ничего не остаётся.
/// </remarks>
internal static class ScreenshotMaker {
    private const double FeetToMillimeters = 304.8;
    private const int ContextTransparency = 75;

    /// <summary>
    /// Снимает скриншоты по заданиям. Возвращает строки отчёта по каждому заданию.
    /// </summary>
    public static List<string> Take(Document document, string imageFolder, IEnumerable<ScreenshotJob> jobs) {
        Directory.CreateDirectory(imageFolder);
        ViewFamilyType view3DType = new FilteredElementCollector(document)
            .OfClass(typeof(ViewFamilyType))
            .Cast<ViewFamilyType>()
            .First(type => type.ViewFamily == ViewFamily.ThreeDimensional);
        FillPatternElement solidFill = new FilteredElementCollector(document)
            .OfClass(typeof(FillPatternElement))
            .Cast<FillPatternElement>()
            .FirstOrDefault(pattern => pattern.GetFillPattern().IsSolidFill);
        var report = new List<string>();
        foreach(ScreenshotJob job in jobs) {
            report.Add(TakeOne(document, imageFolder, job, view3DType, solidFill));
        }

        return report;
    }

    private static string TakeOne(
        Document document,
        string imageFolder,
        ScreenshotJob job,
        ViewFamilyType view3DType,
        FillPatternElement solidFill) {

        var ids = job.ElementIds
            .Select(RevitIds.Create)
            .Where(id => document.GetElement(id) != null)
            .ToList();
        if(ids.Count == 0) {
            return job.Name + ": нет элементов";
        }

        string fragmentFile = Path.Combine(imageFolder, job.Name + ".fragment.txt");
        if(File.Exists(fragmentFile)) {
            File.Delete(fragmentFile);
        }

        BoundingBoxXYZ frame = GetFrame(document, ids, job.Framing, fragmentFile);
        if(frame == null) {
            return job.Name + ": нет габаритов";
        }

        string lastError = null;
        for(int attempt = 0; attempt < 2; attempt++) {
            // повтор после внутренней ошибки Revit: без подрезки и без подсветки остальных элементов
            bool isSimplified = attempt == 1;
            using var transaction = new Transaction(document, "Проверка ВОР: скриншот (откат)");
            transaction.Start();
            try {
                string result = Export(document, imageFolder, job, ids, frame, view3DType, solidFill, isSimplified);
                if(result != null) {
                    return result;
                }

                lastError = "файл не найден";
            } catch(Exception ex) {
                lastError = ex.Message;
            } finally {
                if(transaction.GetStatus() == TransactionStatus.Started) {
                    transaction.RollBack();
                }
            }
        }

        return job.Name + ": ОШИБКА " + lastError;
    }

    /// <summary>
    /// Габарит подрезки вида с запасом.
    /// </summary>
    private static BoundingBoxXYZ GetFrame(
        Document document,
        List<ElementId> ids,
        ScreenshotFraming framing,
        string fragmentFile) {

        var boxes = ids.Select(id => document.GetElement(id).get_BoundingBox(null)).Where(box => box != null).ToList();
        if(boxes.Count == 0) {
            return null;
        }

        bool isIntersection = framing == ScreenshotFraming.Intersection;
        XYZ minimum = boxes[0].Min;
        XYZ maximum = boxes[0].Max;
        foreach(BoundingBoxXYZ box in boxes.Skip(1)) {
            if(isIntersection) {
                minimum = new XYZ(Math.Max(minimum.X, box.Min.X), Math.Max(minimum.Y, box.Min.Y), Math.Max(minimum.Z, box.Min.Z));
                maximum = new XYZ(Math.Min(maximum.X, box.Max.X), Math.Min(maximum.Y, box.Max.Y), Math.Min(maximum.Z, box.Max.Z));
            } else {
                minimum = new XYZ(Math.Min(minimum.X, box.Min.X), Math.Min(minimum.Y, box.Min.Y), Math.Min(minimum.Z, box.Min.Z));
                maximum = new XYZ(Math.Max(maximum.X, box.Max.X), Math.Max(maximum.Y, box.Max.Y), Math.Max(maximum.Z, box.Max.Z));
            }
        }

        bool isFragment = framing == ScreenshotFraming.Fragment && ids.Count > 1;
        if(isFragment) {
            // мелкие элементы на общем виде не видны — снимаем фрагмент вокруг первого элемента
            double medianDiagonal = boxes
                .Select(box => (box.Max - box.Min).GetLength())
                .OrderBy(length => length)
                .ElementAt(boxes.Count / 2);
            XYZ firstCenter = (boxes[0].Min + boxes[0].Max) / 2;
            double radius = Math.Max(1500 / FeetToMillimeters, medianDiagonal);
            var nearBoxes = boxes.Where(box => ((box.Min + box.Max) / 2 - firstCenter).GetLength() <= radius).ToList();
            minimum = new XYZ(nearBoxes.Min(box => box.Min.X), nearBoxes.Min(box => box.Min.Y), nearBoxes.Min(box => box.Min.Z));
            maximum = new XYZ(nearBoxes.Max(box => box.Max.X), nearBoxes.Max(box => box.Max.Y), nearBoxes.Max(box => box.Max.Z));
            File.WriteAllText(fragmentFile, nearBoxes.Count + " из " + boxes.Count, Encoding.UTF8);
        }

        if(isIntersection && (maximum.X < minimum.X || maximum.Y < minimum.Y || maximum.Z < minimum.Z)) {
            XYZ center = (minimum + maximum) / 2;
            minimum = center;
            maximum = center;
        }

        double margin = isIntersection || isFragment
            ? 1500 / FeetToMillimeters
            : Math.Max(3000 / FeetToMillimeters, (maximum - minimum).GetLength() * 0.15);
        var marginVector = new XYZ(margin, margin, margin);
        return new BoundingBoxXYZ { Min = minimum - marginVector, Max = maximum + marginVector };
    }

    /// <summary>
    /// Создаёт временный вид и сохраняет PNG. Возвращает строку отчёта или null, если файл не появился.
    /// </summary>
    private static string Export(
        Document document,
        string imageFolder,
        ScreenshotJob job,
        List<ElementId> ids,
        BoundingBoxXYZ frame,
        ViewFamilyType view3DType,
        FillPatternElement solidFill,
        bool isSimplified) {

        View3D view = View3D.CreateIsometric(document, view3DType.Id);
        try {
            view.ViewTemplateId = ElementId.InvalidElementId;
        } catch(Exception) {
            // у вида может не быть шаблона
        }

        view.DetailLevel = ViewDetailLevel.Fine;
        view.DisplayStyle = DisplayStyle.ShadingWithEdges;
        if(!isSimplified) {
            view.SetSectionBox(frame);
            try {
                view.IsSectionBoxActive = true;
            } catch(Exception) {
                // подрезка уже включена
            }
        }

        HideAnnotations(document, view);
        document.Regenerate();

        var highlightedIds = new HashSet<long>(ids.Select(RevitIds.GetValue));
        foreach(ElementId id in ids) {
            var familyInstance = document.GetElement(id) as FamilyInstance;
            while(familyInstance?.SuperComponent != null) {
                highlightedIds.Add(RevitIds.GetValue(familyInstance.SuperComponent.Id));
                familyInstance = familyInstance.SuperComponent as FamilyInstance;
            }
        }

        OverrideGraphicSettings contextOverride = CreateContextOverride();
        foreach(ElementId elementId in new FilteredElementCollector(document, view.Id).WhereElementIsNotElementType().ToElementIds()) {
            if(!highlightedIds.Contains(RevitIds.GetValue(elementId))) {
                TrySetOverride(view, elementId, contextOverride);
            }
        }

        int revealedCount = Highlight(document, view, highlightedIds, CreateHighlightOverride(solidFill));
        document.Regenerate();
        var visibleIds = new HashSet<long>(new FilteredElementCollector(document, view.Id)
            .WhereElementIsNotElementType()
            .ToElementIds()
            .Select(RevitIds.GetValue));
        int visibleCount = highlightedIds.Count(visibleIds.Contains);
        if(revealedCount > 0) {
            // элементы, ставшие видимыми после включения рабочих наборов и категорий, — тоже полупрозрачные
            foreach(long visibleId in visibleIds.Where(id => !highlightedIds.Contains(id))) {
                TrySetOverride(view, RevitIds.Create(visibleId), contextOverride);
            }
        }

        document.Regenerate();
        if(!ExportImage(document, view, imageFolder, job.Name)) {
            return null;
        }

        string simplifiedNote = isSimplified ? " (упрощённый вид)" : "";
        string revealedNote = revealedCount > 0 ? "; включено категорий/РН " + revealedCount : "";
        return job.Name + ": OK" + simplifiedNote + " (" + ids.Count + " эл.; в виде видно " + visibleCount + " из "
               + highlightedIds.Count + revealedNote + ")";
    }

    private static void HideAnnotations(Document document, View3D view) {
        var hiddenCategories = new HashSet<long> {
            (long) BuiltInCategory.OST_Levels,
            (long) BuiltInCategory.OST_Grids,
            (long) BuiltInCategory.OST_RvtLinks,
            (long) BuiltInCategory.OST_SectionBox
        };
        foreach(Category category in document.Settings.Categories) {
            try {
                bool isHidden = category.CategoryType == CategoryType.Annotation
                                || hiddenCategories.Contains(RevitIds.GetValue(category.Id));
                if(isHidden && view.CanCategoryBeHidden(category.Id)) {
                    view.SetCategoryHidden(category.Id, true);
                }
            } catch(Exception) {
                // часть категорий нельзя скрыть
            }
        }
    }

    /// <summary>
    /// Подсвечивает элементы; включает их категории и рабочие наборы, если они скрыты в новом виде.
    /// Возвращает число включённых категорий и наборов.
    /// </summary>
    private static int Highlight(
        Document document,
        View3D view,
        HashSet<long> highlightedIds,
        OverrideGraphicSettings highlight) {

        int revealedCount = 0;
        foreach(long id in highlightedIds) {
            Element element = document.GetElement(RevitIds.Create(id));
            if(element?.Category != null) {
                try {
                    if(view.GetCategoryHidden(element.Category.Id)) {
                        view.SetCategoryHidden(element.Category.Id, false);
                        revealedCount++;
                    }
                } catch(Exception) {
                    // категорию нельзя включить
                }
            }

            if(element != null && document.IsWorkshared) {
                try {
                    if(view.GetWorksetVisibility(element.WorksetId) != WorksetVisibility.Visible) {
                        view.SetWorksetVisibility(element.WorksetId, WorksetVisibility.Visible);
                        revealedCount++;
                    }
                } catch(Exception) {
                    // рабочий набор нельзя включить
                }
            }

            TrySetOverride(view, RevitIds.Create(id), highlight);
        }

        return revealedCount;
    }

    private static OverrideGraphicSettings CreateContextOverride() {
        var settings = new OverrideGraphicSettings();
        settings.SetSurfaceTransparency(ContextTransparency);
        settings.SetHalftone(true);
        return settings;
    }

    private static OverrideGraphicSettings CreateHighlightOverride(FillPatternElement solidFill) {
        var lineColor = new Color(160, 0, 0);
        var fillColor = new Color(235, 40, 40);
        var settings = new OverrideGraphicSettings();
        settings.SetProjectionLineColor(lineColor);
        settings.SetProjectionLineWeight(5);
        settings.SetCutLineColor(lineColor);
        if(solidFill != null) {
            settings.SetSurfaceForegroundPatternId(solidFill.Id);
            settings.SetSurfaceForegroundPatternColor(fillColor);
            settings.SetCutForegroundPatternId(solidFill.Id);
            settings.SetCutForegroundPatternColor(fillColor);
        }

        return settings;
    }

    private static void TrySetOverride(View view, ElementId elementId, OverrideGraphicSettings settings) {
        try {
            view.SetElementOverrides(elementId, settings);
        } catch(Exception) {
            // у части элементов переопределения недоступны
        }
    }

    private static bool ExportImage(Document document, View3D view, string imageFolder, string name) {
        var options = new ImageExportOptions {
            ExportRange = ExportRange.SetOfViews,
            ZoomType = ZoomFitType.FitToPage,
            PixelSize = 1600,
            FilePath = Path.Combine(imageFolder, name),
            FitDirection = FitDirectionType.Horizontal,
            HLRandWFViewsFileType = ImageFileType.PNG,
            ShadowViewsFileType = ImageFileType.PNG,
            ImageResolution = ImageResolution.DPI_150
        };
        options.SetViewsAndSheets(new List<ElementId> { view.Id });
        DateTime startTime = DateTime.Now.AddSeconds(-2);
        document.ExportImage(options);

        // Revit добавляет к имени файла имя вида — переименовываем в «<имя>.png»
        FileInfo produced = new DirectoryInfo(imageFolder)
            .GetFiles(name + "*.png")
            .Where(file => file.LastWriteTime >= startTime)
            .OrderByDescending(file => file.LastWriteTime)
            .FirstOrDefault();
        string target = Path.Combine(imageFolder, name + ".png");
        if(produced != null && !string.Equals(produced.FullName, target, StringComparison.OrdinalIgnoreCase)) {
            if(File.Exists(target)) {
                File.Delete(target);
            }

            produced.MoveTo(target);
        }

        return File.Exists(target);
    }
}
