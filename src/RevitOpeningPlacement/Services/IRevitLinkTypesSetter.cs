namespace RevitOpeningPlacement.Services;
/// <summary>
/// Интерфейс для назначения используемых типов связей в <see cref="Models.RevitRepository"/>.
/// </summary>
internal interface IRevitLinkTypesSetter {
    /// <summary>
    /// Назначает используемые типы связей в <see cref="Models.RevitRepository"/>.
    /// </summary>
    void SetRevitLinkTypes();
}
