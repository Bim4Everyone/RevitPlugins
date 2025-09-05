namespace RevitClashDetective.Models;

/// <summary>
/// Режим 3D подрезки
/// </summary>
internal enum SectionBoxMode {
    /// <summary>
    /// Вокруг коллизии
    /// </summary>
    AroundCollision,
    /// <summary>
    /// Вокруг элементов
    /// </summary>
    AroundElements
}
