using System;
using System.Collections.Generic;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;

/// <summary>
/// Несуществующая коллизия, в которой есть только второй элемент
/// </summary>
internal class ImaginarySecondClashViewModel
    : BaseViewModel, IClashViewModel, IEquatable<ImaginarySecondClashViewModel> {
    private readonly ElementViewModel _secondElement;

    public ImaginarySecondClashViewModel(ElementViewModel secondElement) {
        _secondElement = secondElement ?? throw new ArgumentNullException(nameof(secondElement));
        SecondElementVolume = secondElement.ElementVolume;

        SecondId = _secondElement.Element.Id.GetIdValue();
        SecondCategory = _secondElement.Element.Category;
        SecondTypeName = _secondElement.Element.Name;
        SecondFamilyName = _secondElement.Element.FamilyName;
        SecondDocumentName = _secondElement.Element.DocumentName;
        SecondLevel = _secondElement.Element.Level;
    }


    public ClashStatus ClashStatus { get => ClashStatus.Imaginary; set { return; } }

    public string ClashName { get => string.Empty; set { return; } }

#if REVIT_2023_OR_LESS
    public int FirstId => -1;
#else
    public long FirstId => -1;
#endif

    public string FirstTypeName => string.Empty;

    public string FirstFamilyName => string.Empty;

    public string FirstDocumentName => string.Empty;

    public string FirstLevel => string.Empty;

    public string FirstCategory => string.Empty;

#if REVIT_2023_OR_LESS
    public int SecondId { get; }
#else
    public long SecondId { get; }
#endif

    public string SecondTypeName { get; }

    public string SecondFamilyName { get; }

    public string SecondLevel { get; }

    public string SecondDocumentName { get; }

    public string SecondCategory { get; }

    public double FirstElementIntersectionPercentage => 0;

    public double SecondElementIntersectionPercentage => 0;

    public double IntersectionVolume => 0;

    public double FirstElementVolume => 0;

    public double SecondElementVolume { get; }


    public ElementModel GetFirstElement() {
        throw new NotSupportedException();
    }

    public ElementModel GetSecondElement() {
        return _secondElement.Element;
    }

    public ICollection<ElementModel> GetElements() {
        return [GetSecondElement()];
    }

    public bool Equals(ImaginarySecondClashViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return _secondElement.Equals(other._secondElement);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ImaginarySecondClashViewModel);
    }

    public override int GetHashCode() {
        return -701136702 + EqualityComparer<ElementViewModel>.Default.GetHashCode(_secondElement);
    }
}
