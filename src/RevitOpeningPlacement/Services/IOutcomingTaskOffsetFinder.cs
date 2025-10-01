using Autodesk.Revit.DB;

using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.Services;
/// <summary>
/// Интерфейс, предоставляющий методы для определения отступа от элемента ВИС из активного файла<br/>
/// до граней исходящего задания на отверстие из активного файла.<br/>
/// Ожидается, что элемент ВИС пересекается с заданием на отверстие, но не выходит за габариты этого задания.
/// </summary>
internal interface IOutcomingTaskOffsetFinder {
    /// <summary>
    /// Находит сумму горизонтальных отступов от элемента ВИС до граней задания на отверстие
    /// </summary>
    /// <param name="opening">Задание на отверстие из активного файла</param>
    /// <param name="mepElement">Элемент ВИС из активного файла, проходящий через задание</param>
    /// <returns>Значение суммы горизонтальных отступов в единицах Revit (футах) от элемента ВИС 
    /// до граней задания на отверстие.</returns>
    double FindHorizontalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement);

    /// <summary>
    /// Находит сумму вертикальных отступов от элемента ВИС до граней задания на отверстие
    /// </summary>
    /// <param name="opening">Задание на отверстие из активного файла</param>
    /// <param name="mepElement">Элемент ВИС из активного файла, проходящий через задание</param>
    /// <returns>Значение суммы вертикальных отступов в единицах Revit (футах) от элемента ВИС 
    /// до граней задания на отверстие.</returns>
    double FindVerticalOffsetsSum(OpeningMepTaskOutcoming opening, Element mepElement);

    /// <summary>
    /// Определяет минимальное допустимое значение суммы горизонтальных отступов от элемента ВИС 
    /// до граней задания на отверстие
    /// </summary>
    /// <param name="mepElement">Элемент ВИС из активного файла</param>
    /// <returns>Минимальное допустимое значение суммы горизонтальных отступов в единицах Revit (футах)</returns>
    double GetMinHorizontalOffsetSum(Element mepElement);

    /// <summary>
    /// Определяет максимальное допустимое значение суммы горизонтальных отступов от элемента ВИС 
    /// до граней задания на отверстие
    /// </summary>
    /// <param name="mepElement">Элемент ВИС из активного файла</param>
    /// <returns>Максимальное допустимое значение суммы горизонтальных отступов в единицах Revit (футах)</returns>
    double GetMaxHorizontalOffsetSum(Element mepElement);

    /// <summary>
    /// Определяет минимальное допустимое значение суммы вертикальных отступов от элемента ВИС 
    /// до граней задания на отверстие
    /// </summary>
    /// <param name="mepElement">Элемент ВИС из активного файла</param>
    /// <returns>Минимальное допустимое значение суммы вертикальных отступов в единицах Revit (футах)</returns>
    double GetMinVerticalOffsetSum(Element mepElement);

    /// <summary>
    /// Определяет максимальное допустимое значение суммы вертикальных отступов от элемента ВИС 
    /// до граней задания на отверстие
    /// </summary>
    /// <param name="mepElement">Элемент ВИС из активного файла</param>
    /// <returns>Максимальное допустимое значение суммы вертикальных отступов в единицах Revit (футах)</returns>
    double GetMaxVerticalOffsetSum(Element mepElement);
}
