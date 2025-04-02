namespace RevitCreateViewSheet.Models {
    internal interface IEntity {
        /// <summary>
        /// Состояние объекта
        /// </summary>
        EntityState State { get; }

        /// <summary>
        /// Помечает объект на удаление
        /// </summary>
        void MarkAsDeleted();

        /// <summary>
        /// Применяет изменения объекта в документе Revit
        /// </summary>
        void SaveChanges(RevitRepository repository);
    }
}
