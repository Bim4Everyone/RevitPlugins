using System;
using System.Collections.Generic;
using System.Dynamic;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

using RevitClashDetective.Models;
using RevitClashDetective.Models.Clashes;
using RevitClashDetective.Models.Extensions;

namespace RevitClashDetective.ViewModels.Navigator;

/// <summary>
/// Несуществующая коллизия, в которой есть только второй элемент
/// </summary>
internal class ImaginarySecondClashViewModel
    : BaseViewModel, IClashViewModel, IEquatable<ImaginarySecondClashViewModel> {
    private readonly RevitRepository _revitRepository;
    private readonly ElementViewModel _secondElement;

    public ImaginarySecondClashViewModel(RevitRepository revitRepository, ElementViewModel secondElement) {
        _revitRepository = revitRepository ?? throw new ArgumentNullException(nameof(revitRepository));
        _secondElement = secondElement ?? throw new ArgumentNullException(nameof(secondElement));
        SecondElementVolume = secondElement.ElementVolume;

        SecondId = _secondElement.Element.Id;
        SecondCategory = _secondElement.Element.Category;
        SecondTypeName = _secondElement.Element.Name;
        SecondFamilyName = _secondElement.Element.FamilyName;
        SecondDocumentName = _secondElement.Element.DocumentName;
        SecondLevel = _secondElement.Element.Level;
        FirstElementParams = new ExpandoObject();
        SecondElementParams = new ExpandoObject();
    }


    public ClashStatus ClashStatus { get => ClashStatus.Imaginary; set { return; } }

    public string ClashName { get => string.Empty; set { return; } }

    public ElementId FirstId => ElementId.InvalidElementId;

    public string FirstTypeName => string.Empty;

    public string FirstFamilyName => string.Empty;

    public string FirstDocumentName => string.Empty;

    public ExpandoObject FirstElementParams { get; }

    public ExpandoObject SecondElementParams { get; }

    public string FirstLevel => string.Empty;

    public string FirstCategory => string.Empty;

    public ElementId SecondId { get; }

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
        return ReferenceEquals(this, other) || _secondElement.Equals(other._secondElement);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ImaginarySecondClashViewModel);
    }

    public override int GetHashCode() {
        return -701136702 + EqualityComparer<ElementViewModel>.Default.GetHashCode(_secondElement);
    }

    public void SetElementParams(string[] paramNames) {
        ((IDictionary<string, object>) FirstElementParams).Clear();
        var secondElementParams = (IDictionary<string, object>) SecondElementParams;
        secondElementParams.Clear();

        var secondElement = GetSecondElement().GetElement(_revitRepository.DocInfos);
        if(secondElement is not null) {
            for(int i = 0; i < paramNames.Length; i++) {
                secondElementParams.Add($"{ClashViewModel.ElementParamFieldName}{i}",
                    secondElement.GetParamValueAsString(paramNames[i]));
            }
        }
    }
}
