namespace RevitCreateViewSheet.Models {
    internal enum RevitViewType {
        /// <summary>
        /// Любой
        /// </summary>
        Any,
        /// <summary>
        /// План этажа
        /// </summary>
        FloorPlan,
        /// <summary>
        /// План потолка
        /// </summary>
        CeilingPlan,
        /// <summary>
        /// План несущих конструкций
        /// </summary>
        EngineeringPlan,
        /// <summary>
        /// План зонирования
        /// </summary>
        AreaPlan,
        /// <summary>
        /// Разрез
        /// </summary>
        Section,
        /// <summary>
        /// Фасад
        /// </summary>
        Elevation,
        /// <summary>
        /// Вид узла
        /// </summary>
        Detail,
        /// <summary>
        /// 3D
        /// </summary>
        ThreeD,
        /// <summary>
        /// Визуализация
        /// </summary>
        Rendering,
        /// <summary>
        /// Чертежный вид
        /// </summary>
        DraftingView,
        /// <summary>
        /// Легенда
        /// </summary>
        Legend
    }
}
