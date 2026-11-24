using System.Collections.Generic;

namespace RevitLintelsManager.Models.Configs;

/// <summary>Контракт LintelManagerConfigValidator.</summary>
internal interface ILintelManagerConfigValidator {

    /// <summary>Проверяет конфигурацию и возвращает причины отказа.</summary>
    /// <param name="config">Разрешённая конфигурация.</param>
    /// <returns>Ключи локализации; пустой список означает успех.</returns>
    IReadOnlyList<string> ValidateDetailed(LintelManagerConfig config);
}
