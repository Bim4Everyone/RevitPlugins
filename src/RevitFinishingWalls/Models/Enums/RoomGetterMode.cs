namespace RevitFinishingWalls.Models.Enums {
    internal enum RoomGetterMode {
        /// <summary>
        /// Помещения, выбранные перед запуском плагина
        /// </summary>
        AlreadySelectedRooms,
        /// <summary>
        /// Помещения на активном виде
        /// </summary>
        RoomsOnActiveView,
        /// <summary>
        /// Помещения, выбранные вручную
        /// </summary>
        ManuallySelectedRooms
    }
}
