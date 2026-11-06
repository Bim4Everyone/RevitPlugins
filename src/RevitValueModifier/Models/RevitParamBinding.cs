namespace RevitValueModifier.Models;
/// <summary>
/// Где находится параметр у выбранных элементов
/// </summary>
internal enum RevitParamBinding {
    /// <summary>
    /// Параметр экземпляра
    /// </summary>
    Instance,
    /// <summary>
    /// Параметр типа
    /// </summary>
    Type,
    /// <summary>
    /// У одних выбранных элементов параметр экземпляра, у других - типа
    /// (например, параметр проекта назначен разным категориям по-разному)
    /// </summary>
    InstanceAndType
}
