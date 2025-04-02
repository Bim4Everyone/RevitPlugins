using RevitCreateViewSheet.Models;

namespace RevitCreateViewSheet.ViewModels {
    internal interface IEntityViewModel {
        bool IsPlaced { get; }

        IEntity Entity { get; }
    }
}
