using Autodesk.Revit.DB;

namespace RevitOpeningPlacement.Services {
    /// <summary>
    /// Интерфейс, предоставляющий методы для определения отступа от элемента ВИС из активного файла<br/>
    /// до граней исходящего задания на отверстие из активного файла.<br/>
    /// Ожидается, что элемент ВИС пересекается с заданием на отверстие, но не выходит за габариты этого задания.
    /// </summary>
    internal interface IOutcomingTaskOffsetFinder : IOutcomingTaskOffsetFinder<Element> {
    }
}
