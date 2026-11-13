namespace RevitPackageDocumentation.ViewModels.Validation;

/// <summary>
/// Модуль, который можно включить или исключить из генерации галочкой
/// </summary>
internal interface ICheckableModule {
    /// <summary>
    /// Модуль включен в генерацию
    /// </summary>
    bool IsModuleCheck { get; }
}
