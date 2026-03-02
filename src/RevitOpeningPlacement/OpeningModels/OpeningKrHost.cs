using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;

namespace RevitOpeningPlacement.OpeningModels;

internal class OpeningKrHost : IOpeningKrHost, IEquatable<OpeningKrHost> {
    private const string _krModelPartParam = "обр_ФОП_Раздел проекта";
    private readonly Element _element;

    /// <summary>
    /// Создает экземпляр класса хоста на основе заданного элемента
    /// </summary>
    /// <param name="host">Элемент - хост</param>
    /// <exception cref="ArgumentNullException">Исключение, если обязательный параметр null</exception>
    public OpeningKrHost(Element host) {
        _element = host ?? throw new ArgumentNullException(nameof(host));

        Name = host.Name;
        Id = host.Id;
        KrModelPart = host.GetSharedParamValueOrDefault(_krModelPartParam, string.Empty);
    }

    /// <summary>
    /// Создает экземпляр класса с пустыми значениями свойств
    /// </summary>
    public OpeningKrHost() {
        Name = string.Empty;
        Id = ElementId.InvalidElementId;
        KrModelPart = string.Empty;
    }


    public string Name { get; }

    public ElementId Id { get; }

    public string KrModelPart { get; }

    public override bool Equals(object obj) {
        return Equals(obj as OpeningKrHost);
    }

    public bool Equals(OpeningKrHost other) {
        return other is not null && (ReferenceEquals(this, other) || Id == other.Id);
    }

    public override int GetHashCode() {
        return 2108858624 + EqualityComparer<ElementId>.Default.GetHashCode(Id);
    }

    public ICollection<ElementModel> GetElementsToSelect() {
        return _element is null ? Array.Empty<ElementModel>() : new ElementModel[] { new(_element) };
    }

    public Element GetElementToHighlight() {
        return _element;
    }
}
