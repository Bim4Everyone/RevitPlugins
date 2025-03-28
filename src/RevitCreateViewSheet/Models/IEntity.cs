namespace RevitCreateViewSheet.Models {
    internal interface IEntity {
        EntityState State { get; }

        void SaveChanges(RevitRepository repository);
    }
}
