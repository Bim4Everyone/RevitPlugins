using System;
using System.Collections.Generic;

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

        SecondCategory = _secondElement.Element.Category;
        SecondTypeName = _secondElement.Element.Name;
        SecondFamilyName = _secondElement.Element.FamilyName;
        SecondDocumentName = _secondElement.Element.DocumentName;
        SecondLevel = _secondElement.Element.Level;
    }


    public ClashStatus ClashStatus { get => ClashStatus.Imaginary; set { return; } }

    public string ClashName { get => string.Empty; set { return; } }

    public string FirstTypeName => string.Empty;

    public string FirstFamilyName => string.Empty;

    public string FirstDocumentName => string.Empty;

    public string FirstLevel => string.Empty;

    public string FirstCategory => string.Empty;

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

    public ClashModel Clash => new ClashModel() {
        ClashStatus = ClashStatus.Imaginary,
        OtherElement = _secondElement.Element
    };

    public bool Equals(ImaginarySecondClashViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return _secondElement.Equals(other._secondElement);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ImaginarySecondClashViewModel);
    }

    public ClashModel GetClashModel() {
        return new ClashModel() {
            ClashStatus = ClashStatus.Imaginary,
            OtherElement = _secondElement.Element
        };
    }

    public override int GetHashCode() {
        return -701136702 + EqualityComparer<ElementViewModel>.Default.GetHashCode(_secondElement);
    }
}
