using System.IO;
using System.Text;

namespace RevitVORChecker.Models;

/// <summary>
/// Журнал проверки: пишется в файл по ходу проверки (если Revit упадёт, будет видно, на каком шаге).
/// </summary>
/// <remarks>
/// Журнал лежит в папке с моделями и в журнал платформы не передаётся: в нём имена моделей и файлов проекта.
/// </remarks>
internal class CheckLog {
    private readonly string _path;

    public CheckLog(string path) {
        _path = path;
        try {
            File.WriteAllText(_path, "", new UTF8Encoding(true));
        } catch(IOException) {
            // журнал не обязателен для проверки
        } catch(UnauthorizedAccessException) {
            // нет прав на запись в папку
        }
    }

    public string Path => _path;

    public void Write(string message) {
        string line = DateTime.Now.ToString("HH:mm:ss") + "  " + message;
        try {
            File.AppendAllText(_path, line + "\r\n", Encoding.UTF8);
        } catch(IOException) {
            // журнал не обязателен для проверки
        } catch(UnauthorizedAccessException) {
            // нет прав на запись в папку
        }
    }
}
