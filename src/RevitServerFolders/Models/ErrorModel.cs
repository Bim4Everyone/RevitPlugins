using System;
using System.IO;

namespace RevitServerFolders.Models;
internal class ErrorModel {
    public ErrorModel(string modelPath, string errorDescription, ExportSettings exportSettings) {
        if(string.IsNullOrWhiteSpace(modelPath)) {
            throw new ArgumentException(nameof(modelPath));
        }
        if(string.IsNullOrWhiteSpace(errorDescription)) {
            throw new ArgumentException(nameof(errorDescription));
        }

        ModelPath = modelPath;
        ErrorDescription = errorDescription;
        ExportSettings = exportSettings ?? throw new ArgumentNullException(nameof(exportSettings));
        ModelName = Path.GetFileName(ModelPath);
    }


    public string ModelName { get; }

    public string ModelPath { get; }

    public string ErrorDescription { get; }

    public ExportSettings ExportSettings { get; }
}
