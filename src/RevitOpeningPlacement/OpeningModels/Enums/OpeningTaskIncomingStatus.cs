using System.ComponentModel;

namespace RevitOpeningPlacement.OpeningModels.Enums {
    /// <summary>
    /// Статусы отработки входящих заданий на отверстия
    /// </summary>
    internal enum OpeningTaskIncomingStatus {
        /// <summary>
        /// Новое задание на отверстие
        /// </summary>
        [Description("Новое задание")]
        New,
        /// <summary>
        /// Частично выполненное задание на отверстие: уже размещенное отверстие не полностью соответствует заданию на отверстие
        /// </summary>
        [Description("Частично выполненное задание")]
        NotMatch,
        /// <summary>
        /// Задание на отверстие отработано: размещенное отверстие полностью соответствует заданию
        /// </summary>
        [Description("Выполненное задание")]
        Completed,
        /// <summary>
        /// Задание на отверстие не пересекается ни с одним элементом конструкции и ни с одним проемом текущем файле АР или КР
        /// </summary>
        [Description("Нет пересечения")]
        NoIntersection
    }
}
