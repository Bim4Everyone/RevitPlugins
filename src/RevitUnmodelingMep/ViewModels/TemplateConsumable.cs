using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;

/// <summary>
/// Хранит шаблонный расходник вместе с исходным ключом конфигурации,
/// чтобы при добавлении недостающего расходника сохранить ключ из шаблона или проверить его на конфликт.
/// </summary>
internal sealed class TemplateConsumable {
    /// <summary>
    /// Создает внутреннее представление шаблонного расходника и подставляет пустую конфигурацию,
    /// если из настроек пришло null-значение.
    /// </summary>
    public TemplateConsumable(string configKey, UnmodelingConfigItem config) {
        ConfigKey = configKey;
        Config = config ?? new UnmodelingConfigItem();
    }

    /// <summary>
    /// Исходный ключ шаблона в словаре настроек, который используется при создании недостающего расходника.
    /// </summary>
    public string ConfigKey { get; }

    /// <summary>
    /// Значения шаблона, с которыми сравнивается и синхронизируется редактируемый расходник.
    /// </summary>
    public UnmodelingConfigItem Config { get; }
}
