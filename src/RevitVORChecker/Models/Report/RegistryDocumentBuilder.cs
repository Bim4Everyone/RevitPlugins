using System.IO.Compression;
using System.IO;
using System.Text;

using RevitVORChecker.Models.Findings;

namespace RevitVORChecker.Models.Report;

/// <summary>
/// Реестр замечаний по контролю качества BIM-моделей (ВОР КР) в формате .docx.
/// </summary>
/// <remarks>
/// Документ строится целиком в коде (без шаблона): шапка, таблица по разделам, рисунки, таблицы расхождений.
/// ID элементов перечисляются, только если их не больше 10, иначе — количество, пример ID и рисунок.
/// Разделы без замечаний — «Замечаний нет».
/// </remarks>
internal static class RegistryDocumentBuilder {
    private const int MaxListedIds = 10;

    /// <summary>
    /// Ширина рисунка, EMU (75% от 16,5 см).
    /// </summary>
    private const long ImageWidthEmu = (long) (16.5 * 360000 * 0.75);

    private const long MaxImageHeightEmu = 9 * 360000;

    /// <summary>
    /// Столбцы таблицы реестра и их ширина (в пятидесятых долях процента).
    /// </summary>
    private static readonly (string Title, int Width)[] _columns = {
        ("№ п/п", 189), ("Замечание", 3043), ("Дата выдачи", 380), ("Срок устранения", 349),
        ("Комментарий Исполнителя", 348), ("Комментарий Заказчика", 349), ("Результат", 342)
    };

    /// <summary>
    /// Разделы реестра, которые выводятся всегда (в т.ч. «Замечаний нет»).
    /// </summary>
    private static readonly string[] _standardSections = {
        ReportSections.Collisions, ReportSections.Integrity, ReportSections.General, ReportSections.SmrBlock,
        ReportSections.SmrSection, ReportSections.SmrFloor, ReportSections.MaterialKeynote,
        ReportSections.MaterialName, ReportSections.MaterialDescription, ReportSections.MaterialChapter,
        ReportSections.MaterialWork, ReportSections.MaterialUnit, ReportSections.MaterialCountType,
        ReportSections.Location, ReportSections.Purpose, ReportSections.MaterialType, ReportSections.Size,
        ReportSections.Reinforcement, ReportSections.AtsNumber, ReportSections.AdditionalSorting
    };

    /// <summary>
    /// Собирает реестр.
    /// </summary>
    /// <param name="findings">Замечания.</param>
    /// <param name="outputPath">Путь реестра; если файл занят — «… (2).docx».</param>
    /// <param name="objectName">Наименование объекта для шапки.</param>
    /// <param name="checkedModels">Перечень проверенных моделей для шапки.</param>
    public static RegistryResult Build(List<Finding> findings, string outputPath, string objectName, string checkedModels) {
        var images = new List<(string RelationId, string FileName, byte[] Content)>();
        string body = BuildBody(findings, objectName, checkedModels, images);
        string path = GetFreePath(outputPath);
        using(var stream = new FileStream(path, FileMode.Create))
        using(var archive = new ZipArchive(stream, ZipArchiveMode.Create)) {
            WriteEntry(archive, "[Content_Types].xml", OpenXmlParts.ContentTypes);
            WriteEntry(archive, "_rels/.rels", OpenXmlParts.PackageRelations);
            WriteEntry(archive, "word/document.xml", OpenXmlParts.WrapDocument(body));
            WriteEntry(archive, "word/styles.xml", OpenXmlParts.Styles);
            WriteEntry(archive, "word/settings.xml", OpenXmlParts.Settings);
            WriteEntry(archive, "word/_rels/document.xml.rels", OpenXmlParts.DocumentRelations(images
                .Select(image => (image.RelationId, "media/" + image.FileName))));
            foreach(var image in images) {
                ZipArchiveEntry entry = archive.CreateEntry("word/media/" + image.FileName, CompressionLevel.Optimal);
                using Stream entryStream = entry.Open();
                entryStream.Write(image.Content, 0, image.Content.Length);
            }
        }

        return new RegistryResult(path, images.Count);
    }

    private static string BuildBody(
        List<Finding> findings,
        string objectName,
        string checkedModels,
        List<(string RelationId, string FileName, byte[] Content)> images) {

        var body = new StringBuilder();
        body.Append(WordXml.Paragraph("Реестр замечаний по контролю качества BIM-моделей. ВОР КР", bold: true, size: 28));
        body.Append(WordXml.LabeledParagraph("Наименование объекта", objectName ?? ""));
        body.Append(WordXml.LabeledParagraph("Раздел", "КР"));
        if(!string.IsNullOrEmpty(checkedModels)) {
            body.Append(WordXml.LabeledParagraph("Проверенные модели", checkedModels));
        }

        body.Append(WordXml.Paragraph(""));
        body.Append(WordXml.TableStart(_columns.Select(column => column.Width)));
        body.Append(WordXml.HeaderRow(_columns.Select(column => column.Title), _columns.Select(column => column.Width)));

        bool hasSeveralFiles = findings
            .SelectMany(finding => finding.Entries)
            .Select(entry => entry.FileName)
            .Where(name => !string.IsNullOrEmpty(name))
            .Distinct()
            .Count() > 1;
        var sections = _standardSections.ToList();
        foreach(Finding finding in findings.Where(finding => !sections.Contains(finding.Section))) {
            sections.Add(finding.Section);
        }

        var widths = _columns.Select(column => column.Width).ToList();
        foreach(string section in sections) {
            var sectionFindings = findings.Where(finding => finding.Section == section).ToList();
            string extraTitle = sectionFindings.Select(finding => finding.Title).FirstOrDefault();
            string title = _standardSections.Contains(section)
                ? ReportSections.GetTitle(section)
                : string.IsNullOrEmpty(extraTitle) ? section : extraTitle;
            body.Append(WordXml.SectionRow(title, widths));
            if(sectionFindings.Count == 0) {
                body.Append(WordXml.Row(new[] { WordXml.Paragraph(""), WordXml.Paragraph("Замечаний нет.") }, widths));
                continue;
            }

            for(int number = 1; number <= sectionFindings.Count; number++) {
                string content = BuildFindingContent(sectionFindings[number - 1], hasSeveralFiles, images);
                string findingNumber = ReportSections.GetNumber(section) + "." + number;
                body.Append(WordXml.Row(
                    new[] { WordXml.Paragraph(findingNumber), content },
                    _columns.Select(column => column.Width)));
            }
        }

        body.Append("</w:tbl>");
        body.Append(WordXml.Paragraph(""));
        body.Append(WordXml.Paragraph("Ответственный BIM-координатор КР: ", bold: true));
        return body.ToString();
    }

    private static string BuildFindingContent(
        Finding finding,
        bool hasSeveralFiles,
        List<(string RelationId, string FileName, byte[] Content)> images) {

        string prefix = finding.Kind switch {
            FindingKinds.Clarify => "Требует уточнения. ",
            FindingKinds.Info => "Примечание. ",
            _ => ""
        };
        var content = new StringBuilder(WordXml.Paragraph(prefix + finding.Text));
        foreach(FindingEntry entry in finding.Entries) {
            var parts = new List<string>();
            if(!string.IsNullOrEmpty(entry.FileName) && (hasSeveralFiles || finding.Entries.Count > 1)) {
                parts.Add("Файл " + entry.FileName);
            }

            if(!string.IsNullOrEmpty(entry.Detail)) {
                parts.Add(entry.Detail);
            }

            string idsText = DescribeElementIds(entry);
            if(idsText != null) {
                parts.Add(idsText);
            }

            if(parts.Count > 0) {
                content.Append(WordXml.Paragraph("• " + string.Join(". ", parts.Select(part => part.TrimEnd('.'))) + "."));
            }

            if(!string.IsNullOrEmpty(entry.ImagePath) && File.Exists(entry.ImagePath)) {
                content.Append(AddImage(entry.ImagePath, images));
            }
        }

        if(finding.Table != null) {
            content.Append(WordXml.NestedTable(finding.Table.Header, finding.Table.Rows));
            content.Append(WordXml.Paragraph(""));
        }

        return content.ToString();
    }

    /// <summary>
    /// Текст об ID элементов записи; null — ID не выводятся (нет элементов или пары ID уже в уточнении).
    /// </summary>
    private static string DescribeElementIds(FindingEntry entry) {
        List<long> ids = entry.ElementIds;
        if(ids.Count == 0 || (entry.Detail ?? "").Contains("Пары ID:")) {
            return null;
        }

        if(ids.Count <= MaxListedIds) {
            return "ID: " + string.Join(", ", ids);
        }

        string imagePath = entry.ImagePath ?? "";
        string fragmentFile = imagePath.Length > 0
            ? Path.Combine(Path.GetDirectoryName(imagePath), Path.GetFileNameWithoutExtension(imagePath)) + ".fragment.txt"
            : "";
        if(imagePath.Length > 0 && File.Exists(fragmentFile)) {
            return "элементов: " + ids.Count + ", пример ID " + ids[0] + " (на рисунке — фрагмент модели вокруг этого "
                   + "элемента; элементы мелкие и разбросаны по модели, на общем виде не различимы)";
        }

        return "элементов: " + ids.Count + ", пример ID " + ids[0] + " (остальные — на рисунке)";
    }

    private static string AddImage(string imagePath, List<(string RelationId, string FileName, byte[] Content)> images) {
        int imageNumber = images.Count + 1;
        string fileName = "vor_img_" + imageNumber.ToString("000") + ".png";
        string relationId = "rIdVor" + imageNumber;
        images.Add((relationId, fileName, File.ReadAllBytes(imagePath)));
        (int width, int height) = GetPngSize(imagePath);
        long widthEmu = ImageWidthEmu;
        long heightEmu = (long) (widthEmu * height / (double) width);
        if(heightEmu > MaxImageHeightEmu) {
            heightEmu = MaxImageHeightEmu;
            widthEmu = (long) (heightEmu * width / (double) height);
        }

        return WordXml.ImageParagraph(relationId, 5000 + imageNumber, widthEmu, heightEmu, fileName);
    }

    private static (int Width, int Height) GetPngSize(string path) {
        using FileStream stream = File.OpenRead(path);
        var header = new byte[24];
        stream.Read(header, 0, header.Length);
        int width = (header[16] << 24) | (header[17] << 16) | (header[18] << 8) | header[19];
        int height = (header[20] << 24) | (header[21] << 16) | (header[22] << 8) | header[23];
        return (width, height);
    }

    /// <summary>
    /// Путь, по которому можно записать файл: если исходный занят (открыт в Word) — «… (2).docx» и т.д.
    /// </summary>
    private static string GetFreePath(string outputPath) {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(outputPath)));
        string path = outputPath;
        int copyNumber = 1;
        while(true) {
            try {
                using(File.Open(path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None)) { }

                return path;
            } catch(IOException) {
                copyNumber++;
                path = Path.Combine(
                    Path.GetDirectoryName(outputPath),
                    Path.GetFileNameWithoutExtension(outputPath) + " (" + copyNumber + ")" + Path.GetExtension(outputPath));
            }
        }
    }

    private static void WriteEntry(ZipArchive archive, string name, string content) {
        ZipArchiveEntry entry = archive.CreateEntry(name, CompressionLevel.Optimal);
        using var writer = new StreamWriter(entry.Open(), new UTF8Encoding(false));
        writer.Write(content);
    }
}

/// <summary>
/// Результат сборки реестра.
/// </summary>
internal class RegistryResult {
    public RegistryResult(string path, int imageCount) {
        Path = path;
        ImageCount = imageCount;
    }

    public string Path { get; }

    public int ImageCount { get; }
}
