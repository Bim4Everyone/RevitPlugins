namespace RevitCreateViewSheet.Models {
    internal interface IModel {
        EntityState State { get; }

        void SaveChanges(RevitRepository repository);
    }
}
