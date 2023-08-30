namespace RevitOpeningPlacement.Models.Interfaces {
    /// <summary>
    /// Интерфейс, предоставляющий типоразмер семейства задания на отверстие
    /// </summary>
    internal interface IOpeningTaskTypeProvider {
        /// <summary>
        /// Возвращает тип проема задания на отверстие
        /// </summary>
        /// <returns></returns>
        OpeningType GetOpeningTaskType();
    }
}
