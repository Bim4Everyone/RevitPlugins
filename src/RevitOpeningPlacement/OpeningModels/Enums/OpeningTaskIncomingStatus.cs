namespace RevitOpeningPlacement.OpeningModels.Enums;
/// <summary>
/// Статусы отработки входящих заданий на отверстия
/// </summary>
internal enum OpeningTaskIncomingStatus {
    /// <summary>
    /// Новое задание на отверстие
    /// </summary>
    New,
    /// <summary>
    /// Частично выполненное задание на отверстие: уже размещенное отверстие не полностью соответствует заданию на отверстие
    /// </summary>
    NotMatch,
    /// <summary>
    /// Задание на отверстие отработано: размещенное отверстие полностью соответствует заданию
    /// </summary>
    Completed,
    /// <summary>
    /// Задание на отверстие не пересекается ни с одним элементом конструкции и ни с одним проемом текущем файле АР или КР
    /// </summary>
    NoIntersection,
    /// <summary>
    /// Произошла ошибка обработки геометрии в процессе определения статуса
    /// </summary>
    Invalid,
    /// <summary>
    /// Задание на отверстие находится в разных конструкциях
    /// </summary>
    DifferentConstructions,
    /// <summary>
    /// Задание на отверстие находится в недопустимых конструкциях
    /// </summary>
    UnacceptableConstructions
}
