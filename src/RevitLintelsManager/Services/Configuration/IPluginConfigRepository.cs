using RevitLintelsManager.Models.Settings;

namespace RevitLintelsManager.Models.Configs;

/// <summary>Контракт PluginConfigRepository.</summary>
internal interface IPluginConfigRepository {
    string ConfigPath { get; }

    /// <summary>Загружает сохранённые настройки исходного проекта.</summary>
    /// <returns>Настройки или null.</returns>
    LintelManagerSettings LoadSettings();

    /// <summary>Сохраняет настройки проекта с восстановлением при ошибке.</summary>
    /// <param name="lintelManagerSettings">Новые настройки.</param>
    void SaveSettings(LintelManagerSettings lintelManagerSettings);

    /// <summary>Создаёт настройки по умолчанию для исходного проекта.</summary>
    /// <returns>Начальные настройки.</returns>
    LintelManagerSettings GetDefaultLintelManagerSettings();
}
