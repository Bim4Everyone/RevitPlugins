using RevitToMongoDB.Model;

namespace RevitToMongoDB.ViewModels.Interfaces {
    internal interface IElementRepository {
        void Insert(ElementDto elementDto);
    }
}
