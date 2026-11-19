using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

/// <summary>Контракт LintelManagerConfigResolver.</summary>
internal interface ILintelManagerConfigResolver {

    /// <summary>Разрешает сохранённые имена в исходном API-контексте.</summary>
    /// <param name="settings">Сохранённые настройки.</param>
    /// <returns>Временная runtime-конфигурация.</returns>
    LintelManagerConfig BuildLintelManagerConfig(LintelManagerSettings settings);

    /// <summary>Материализует глубокий независимый снимок проверенной конфигурации.</summary>
    /// <param name="config">Успешно проверенная конфигурация.</param>
    /// <returns>Неизменяемые значения без объектов Revit.</returns>
    ManagerConfigSnapshot CreateSnapshot(LintelManagerConfig config);
}
