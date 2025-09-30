using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;

using Autodesk.Revit.DB;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;
internal interface IClashViewModel : INotifyPropertyChanged {
    ClashStatus ClashStatus { get; set; }

    string ClashName { get; set; }

    ElementId FirstId { get; }

    string FirstTypeName { get; }

    string FirstFamilyName { get; }

    string FirstDocumentName { get; }

    string FirstLevel { get; }

    string FirstCategory { get; }

    ElementId SecondId { get; }

    string SecondTypeName { get; }

    string SecondFamilyName { get; }

    string SecondLevel { get; }

    string SecondDocumentName { get; }

    string SecondCategory { get; }

    /// <summary>
    /// Процент пересечения относительно объема первого элемента коллизии
    /// </summary>
    double FirstElementIntersectionPercentage { get; }

    /// <summary>
    /// Процент пересечения относительно объема второго элемента коллизии
    /// </summary>
    double SecondElementIntersectionPercentage { get; }

    /// <summary>
    /// Объем пересечения в м3
    /// </summary>
    double IntersectionVolume { get; }

    /// <summary>
    /// Объем первого элемента в м3
    /// </summary>
    double FirstElementVolume { get; }

    /// <summary>
    /// Объем второго элемента в м3
    /// </summary>
    double SecondElementVolume { get; }

    /// <summary>
    /// Значения дополнительных параметров первого элемента коллизии.
    /// Названия генерируемых свойств должны быть в формате Field0, Field1 и так далее.
    /// </summary>
    ExpandoObject FirstElementParams { get; }

    /// <summary>
    /// Значения дополнительных параметров второго элемента коллизии.
    /// Названия генерируемых свойств должны быть в формате Field0, Field1 и так далее.
    /// </summary>
    ExpandoObject SecondElementParams { get; }

    /// <summary>
    /// Первый элемент, участвующий в коллизии
    /// </summary>
    /// <exception cref="System.NotSupportedException">Исключение, если первого элемента нет</exception>
    ElementModel GetFirstElement();

    /// <summary>
    /// Второй элемент, участвующий в коллизии
    /// </summary>
    /// <exception cref="System.NotSupportedException">Исключение, если первого второго нет</exception>
    ElementModel GetSecondElement();

    /// <summary>
    /// Возвращает все элементы, участвующие в коллизии
    /// </summary>
    /// <returns></returns>
    ICollection<ElementModel> GetElements();

    /// <summary>
    /// Назначает значения дополнительных параметров <see cref="FirstElementParams"/> и <see cref="SecondElementParams"/>.
    /// Названия генерируемых свойств в объектах должны быть в формате Field0, Field1 и так далее.
    /// </summary>
    /// <param name="paramNames">Названия параметров</param>
    void SetElementParams(string[] paramNames);
}
