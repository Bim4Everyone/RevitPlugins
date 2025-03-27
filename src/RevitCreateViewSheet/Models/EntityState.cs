namespace RevitCreateViewSheet.Models {
    /// <summary>
    /// Состояние объекта
    /// </summary>
    internal enum EntityState {
        /// <summary>
        /// Объект не изменен
        /// </summary>
        Unchanged,
        /// <summary>
        /// Объект удален
        /// </summary>
        Deleted,
        /// <summary>
        /// Объект изменен
        /// </summary>
        Modified,
        /// <summary>
        /// Объект добавлен
        /// </summary>
        Added
    }
}
