using System;
using System.Collections.Generic;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitClashDetective.ViewModels.Navigator;

/// <summary>
/// Несуществующая коллизия, в которой есть только первый элемент
/// </summary>
internal class ImaginaryFirstClashViewModel
    : BaseViewModel, IClashViewModel, IEquatable<ImaginaryFirstClashViewModel> {
    private readonly ElementViewModel _firstElement;

    public ImaginaryFirstClashViewModel(ElementViewModel firstElement) {
        _firstElement = firstElement ?? throw new ArgumentNullException(nameof(firstElement));
        FirstElementVolume = firstElement.ElementVolume;

        FirstCategory = _firstElement.Element.Category;
        FirstTypeName = _firstElement.Element.Name;
        FirstFamilyName = _firstElement.Element.FamilyName;
        FirstDocumentName = _firstElement.Element.DocumentName;
        FirstLevel = _firstElement.Element.Level;
    }


    public ClashStatus ClashStatus { get => ClashStatus.Imaginary; set { return; } }

    public string ClashName { get => string.Empty; set { return; } }

    public string FirstTypeName { get; }

    public string FirstFamilyName { get; }

    public string FirstDocumentName { get; }

    public string FirstLevel { get; }

    public string FirstCategory { get; }

    public string SecondTypeName => string.Empty;

    public string SecondFamilyName => string.Empty;

    public string SecondLevel => string.Empty;

    public string SecondDocumentName => string.Empty;

    public string SecondCategory => string.Empty;

    public double FirstElementIntersectionPercentage => 0;

    public double SecondElementIntersectionPercentage => 0;

    public double IntersectionVolume => 0;

    public ClashModel Clash => new ClashModel() {
        ClashStatus = ClashStatus.Imaginary,
        MainElement = _firstElement.Element
    };

    public double FirstElementVolume { get; }

    public double SecondElementVolume => 0;

    public bool Equals(ImaginaryFirstClashViewModel other) {
        if(other is null) { return false; }
        if(ReferenceEquals(this, other)) { return true; }

        return _firstElement.Equals(other._firstElement);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ImaginaryFirstClashViewModel);
    }

    public ClashModel GetClashModel() {
        return new ClashModel() {
            ClashStatus = ClashStatus.Imaginary,
            MainElement = _firstElement.Element
        };
    }

    public override int GetHashCode() {
        return 1531809236 + EqualityComparer<ElementViewModel>.Default.GetHashCode(_firstElement);
    }
}
