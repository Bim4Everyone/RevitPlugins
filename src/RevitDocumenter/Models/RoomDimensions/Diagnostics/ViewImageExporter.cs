using System.IO;

using Autodesk.Revit.DB;

namespace RevitDocumenter.Models.RoomDimensions.Diagnostics;

/// <summary>
/// Выгружает активный вид в PNG рядом с журналом, чтобы результат можно было проверить без Revit.
/// </summary>
internal class ViewImageExporter {
    // Ширина изображения в пикселях: достаточно, чтобы читались значения размеров при увеличении
    private const int _pixelSize = 4096;

    private readonly RevitRepository _revitRepository;

    public ViewImageExporter(RevitRepository revitRepository) {
        _revitRepository = revitRepository.ThrowIfNull();
    }

    /// <summary>
    /// Экспортирует активный вид. Вызывать вне транзакции.
    /// </summary>
    /// <returns>Путь к изображению.</returns>
    public string Export(string outputFolder, string baseName) {
        var document = _revitRepository.Document;
        var view = _revitRepository.ActiveView;
        Directory.CreateDirectory(outputFolder);

        // Revit сам задает имя файла по виду, поэтому выгружаем во временную папку и переносим
        string tempFolder = Path.GetTempPath();
        var options = new ImageExportOptions {
            FilePath = tempFolder,
            PixelSize = _pixelSize,
            FitDirection = FitDirectionType.Horizontal,
            ImageResolution = ImageResolution.DPI_300,
            HLRandWFViewsFileType = ImageFileType.PNG,
            ShadowViewsFileType = ImageFileType.PNG,
            ExportRange = ExportRange.SetOfViews
        };
        options.SetViewsAndSheets([view.Id]);
        document.ExportImage(options);

        string exportedPath = Path.Combine(tempFolder, ImageExportOptions.GetFileName(document, view.Id) + ".png");
        string targetPath = Path.Combine(outputFolder, baseName + ".png");
        File.Copy(exportedPath, targetPath, true);
        File.Delete(exportedPath);
        return targetPath;
    }
}
