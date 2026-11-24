using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

using RevitLintelsManager.Models.Configs;

namespace RevitLintelsManager.Services;

/// <summary>Операции только над собственным 3D-видом менеджера.</summary>
internal interface IManagerViewService {
    /// <summary>Находит либо создаёт и настраивает персональный 3D-вид.</summary>
    /// <param name="app">Приложение текущего вызова Revit.</param>
    /// <param name="config">Неизменяемые настройки сессии.</param>
    /// <returns>UniqueId вида или null при смене документа.</returns>
    /// <exception cref="System.InvalidOperationException">Revit не подтвердил подготовку вида.</exception>
    string EnsureManagerView(UIApplication app, ManagerConfigSnapshot config);

    /// <summary>Проверяет метку владельца и идентичность персонального 3D-вида.</summary>
    /// <param name="uniqueId">Сохранённый UniqueId вида.</param>
    /// <param name="username">Имя пользователя Revit.</param>
    /// <returns>Подходящий вид или null.</returns>
    View3D GetOwnedView(string uniqueId, string username);

    /// <summary>Назначает и активирует рамку персонального вида в транзакции.</summary>
    /// <param name="view">Проверенный персональный вид менеджера.</param>
    /// <param name="sectionBox">Границы целевых элементов в мировых координатах.</param>
    /// <exception cref="System.InvalidOperationException">Revit не подтвердил подготовку вида.</exception>
    void SaveManagerSectionBox(View3D view, BoundingBoxXYZ sectionBox);

    /// <summary>Опознаёт транзакцию подготовки вида или установки его рамки.</summary>
    /// <param name="name">Имя из события DocumentChanged.</param>
    /// <returns>Принадлежность имени операциям персонального вида.</returns>
    bool IsViewTransaction(string name);
}
