using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

using dosymep.Bim4Everyone;
using dosymep.SimpleServices;

using RevitServerFolders.Models;

namespace RevitServerFolders.Services;
/// <summary>
/// Экспортирует файлы с Revit-server в заданную директорию
/// </summary>
internal class RvtExportService : IModelsExportService<RsModelObjectExportSettings> {
    private const string _revitServerToolPath
        = @"C:\Program Files\Autodesk\Revit {0}\RevitServerToolCommand\RevitServerTool.exe";
    private const string _revitServerToolArgs = @"createLocalRvt ""{0}"" -s ""{1}"" -d ""{2}/"" -o";
    private const string _rvtSearchPattern = "*.rvt";
    private readonly ILoggerService _loggerService;

    public RvtExportService(ILoggerService loggerService) {
        _loggerService = loggerService ?? throw new ArgumentNullException(nameof(loggerService));
    }


    public void ExportModelObjects(
        string[] modelFiles,
        RsModelObjectExportSettings settings,
        IProgress<int> progress = null,
        CancellationToken ct = default,
        int startProcess = 0) {
        if(settings is null) {
            throw new ArgumentException(nameof(settings));
        }
        if(modelFiles is null) {
            throw new ArgumentNullException(nameof(modelFiles));
        }

        Directory.CreateDirectory(settings.TargetFolder);

        if(settings.ClearTargetFolder) {
            string[] revitFiles = Directory.GetFiles(settings.TargetFolder, _rvtSearchPattern);
            foreach(string revitFile in revitFiles) {
                File.SetAttributes(revitFile, FileAttributes.Normal);
                File.Delete(revitFile);
            }
        }

        foreach(string modelFile in modelFiles) {
            progress?.Report(startProcess++);
            ct.ThrowIfCancellationRequested();

            try {
                ExportDocument(modelFile, settings.TargetFolder);
                dosymep.Revit.DocumentExtensions.UnloadAllLinks(
                    Directory.GetFiles(settings.TargetFolder, _rvtSearchPattern));
            } catch(Exception ex) {
                _loggerService.Warning(ex, $"Ошибка экспорта в rvt в файле: {modelFile}");
            }
        }
    }


    private void ExportDocument(string modelName, string targetFolder) {
        // https://help.autodesk.com/view/RVT/2022/ENU/?guid=GUID-77F62A96-8A61-4539-9664-0DD0AB08B6ED
        var uri = new Uri(modelName);

        string arguments = string.Format(
            _revitServerToolArgs, uri.LocalPath.Trim('\\').Trim('/'), uri.Host, targetFolder);

        string fileName = string.Format(_revitServerToolPath, ModuleEnvironment.RevitVersion);

        using var process = Process.Start(new ProcessStartInfo() {
            Arguments = arguments,
            FileName = fileName,
            CreateNoWindow = true,
            UseShellExecute = false,
            WindowStyle = ProcessWindowStyle.Hidden,
        });
        process?.WaitForExit();
    }
}
