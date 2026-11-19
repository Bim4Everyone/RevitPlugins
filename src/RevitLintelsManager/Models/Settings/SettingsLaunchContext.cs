namespace RevitLintelsManager.Models.Settings;

/// <summary>Единственная загрузка настроек для всех вкладок модального окна.</summary>
internal sealed class SettingsLaunchContext(LintelManagerSettings settings, bool hasSavedSettings) {
    public LintelManagerSettings Settings { get; } = settings;
    public bool HasSavedSettings { get; } = hasSavedSettings;
}
