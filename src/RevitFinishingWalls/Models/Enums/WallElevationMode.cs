namespace RevitFinishingWalls.Models.Enums {
    /// <summary>
    /// Варианты задания верхней отметки отделочных стен от уровня
    /// </summary>
    internal enum WallElevationMode {
        /// <summary>
        /// Верхняя отметка отделочных стен от уровня задается вручную
        /// </summary>
        ManualHeight,
        /// <summary>
        /// Верхняя отметка отделочных стен равна отметке верхней границы помещения (с учетом ограничивающих помещение элементов)
        /// </summary>
        HeightByRoom
    }
}
