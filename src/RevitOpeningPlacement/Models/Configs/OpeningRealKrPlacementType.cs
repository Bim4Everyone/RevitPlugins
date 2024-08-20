namespace RevitOpeningPlacement.Models.Configs {
    /// <summary>
    /// Типы расстановки чистовых отверстий КР
    /// </summary>
    internal enum OpeningRealKrPlacementType {
        /// <summary>
        /// Расстановка на основе входящих заданий от ВИС
        /// </summary>
        PlaceByMep,
        /// <summary>
        /// Расстановка на основе входящих заданий от АР
        /// </summary>
        PlaceByAr
    }
}
