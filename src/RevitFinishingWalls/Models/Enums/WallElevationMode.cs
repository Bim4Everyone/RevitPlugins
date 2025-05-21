namespace RevitFinishingWalls.Models.Enums {
    /// <summary>
    /// Варианты задания отметки отделочных стен от уровня
    /// </summary>
    internal enum WallElevationMode {
        /// <summary>
        /// Отметка отделочных стен от уровня (сверху/снизу) задается вручную
        /// </summary>
        ManualHeight,
        /// <summary>
        /// Отметка отделочных стен от уровня (сверху/снизу) равна отметке помещения
        /// </summary>
        HeightByRoom
    }
}
