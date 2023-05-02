namespace RevitOpeningPlacement.OpeningModels.Enums {
    /// <summary>
    /// Статусы чистовых экземпляров отверстий, которые идут на чертежи, относительно выданных заданий на отверстия 
    /// </summary>
    internal enum OpeningRealTaskStatusEnum {
        /// <summary>
        /// Нет ни одного задания на это отверстие
        /// </summary>
        NoTask,
        /// <summary>
        /// Выполненное отверстие и задание на него различаются
        /// </summary>
        NotMatchTask,
        /// <summary>
        /// Выполненное отверстие и задание на него полностью совпадают
        /// </summary>
        MatchTask,
        /// <summary>
        /// Не удалось определить статус чистового отверстия. 
        /// Использовать при некорректной обработке геометрии чистового отверстия
        /// </summary>
        NotDefined
    }
}
