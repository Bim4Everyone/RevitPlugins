namespace RevitParamsChecker.Models.Results;

internal enum StatusCode {
    /// <summary>
    /// Ошибка определения статуса
    /// </summary>
    Error,

    /// <summary>
    /// Параметр не найден
    /// </summary>
    ParamNotFound,

    /// <summary>
    /// Не удовлетворяет правилу проверки
    /// </summary>
    Invalid,

    /// <summary>
    /// Удовлетворяет правилу проверки
    /// </summary>
    Valid
}
