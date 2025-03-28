namespace RevitCreateViewSheet.Models {
    internal interface IEntity {
        /// <summary>
        /// Состояние объекта
        /// </summary>
        EntityState State { get; }

        /// <summary>
        /// Применяет изменения объекта в документе Revit
        /// </summary>
        void SaveChanges(RevitRepository repository);
    }
}
