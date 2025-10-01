using System;
using System.Diagnostics;
using System.IO;

namespace RevitOpeningPlacement.Services;
internal class ConfigFileService {
    private const string _explorer = "explorer.exe";
    private const string _selectMask = "/select,\"{0}\"";
    public ConfigFileService() { }

    /// <summary>
    /// Открывает проводник в заданной папке.
    /// </summary>
    /// <param name="folderPath">Папка, в которой надо открыть проводник.</param>
    /// <param name="selectFilePath">Файл, который надо выбрать в проводнике.</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр Null.</exception>
    /// <exception cref="ArgumentException">Исключение, если передан невалидный параметр.</exception>
    /// <exception cref="DirectoryNotFoundException">Исключение, если заданная папка не существует.</exception>
    public void OpenFolder(string folderPath, string selectFilePath = null) {
        if(folderPath is null) {
            throw new ArgumentNullException(nameof(folderPath));
        }
        if(string.IsNullOrWhiteSpace(folderPath)) {
            throw new ArgumentException(nameof(folderPath));
        }
        if(!Directory.Exists(folderPath)) {
            throw new DirectoryNotFoundException(nameof(folderPath));
        }

        if(!string.IsNullOrWhiteSpace(selectFilePath) && File.Exists(selectFilePath)) {
            Process.Start(_explorer, string.Format(_selectMask, Path.GetFullPath(selectFilePath)));
        } else {
            Process.Start(Path.GetFullPath(folderPath));
        }
    }
}
