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
    private readonly Action<string> _onWrite;

    /// <param name="path">Файл журнала.</param>
    /// <param name="onWrite">Получает каждую запись (для строки состояния окна прогресса).</param>
    public CheckLog(string path, Action<string> onWrite = null) {
        _path = path;
        _onWrite = onWrite;
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
        _onWrite?.Invoke(message);
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
