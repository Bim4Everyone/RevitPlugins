namespace RevitStairRailing.Models;

/// <summary>
/// Ошибка построения ограждения, понятная пользователю: текст берётся из локализации по ключу.
/// </summary>
internal class RailingException : Exception {
    public RailingException(string messageKey)
        : base(messageKey) {
        MessageKey = messageKey;
    }

    /// <summary>
    /// Ключ текста ошибки в словаре локализации.
    /// </summary>
    public string MessageKey { get; }
}
