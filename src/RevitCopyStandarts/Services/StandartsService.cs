using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace RevitCopyStandarts.Services;

internal sealed class StandartsService : IStandartsService {
    private const string _mainFolder =
        @"W:\Проектный институт\Отд.стандарт.BIM и RD\BIM-Ресурсы\5-Надстройки\Bim4Everyone\A101";

    public string GetBimPart(FileInfo bimFile) {
        return bimFile.Name.Split('_').FirstOrDefault();
    }
    
    public string GetFileName(FileInfo bimFile) {
        return string.Join("_", Path.GetFileNameWithoutExtension(bimFile.Name).Split('_').Skip(1));
    }

    public IEnumerable<FileInfo> GetStandartsFiles() {
        return Directory
            .EnumerateFiles(_mainFolder, "*.rvt", SearchOption.TopDirectoryOnly)
            .Select(item => new FileInfo(item));
    }
}
