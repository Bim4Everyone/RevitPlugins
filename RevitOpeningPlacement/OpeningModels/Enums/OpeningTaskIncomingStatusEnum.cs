namespace RevitOpeningPlacement.OpeningModels.Enums {
    /// <summary>
    /// Статусы отработки входящих заданий на отверстия
    /// </summary>
    internal enum OpeningTaskIncomingStatusEnum {
        /// <summary>
        /// Новое задание на отверстие
        /// </summary>
        NewTask,
        /// <summary>
        /// Частично выполненное задание на отверстие: уже размещенное отверстие не полностью соответствует заданию на отверстие
        /// </summary>
        NotMatch,
        /// <summary>
        /// Задание на отверстие отработано: размещенное отверстие полностью соответствует заданию
        /// </summary>
        Completed
    }
}
