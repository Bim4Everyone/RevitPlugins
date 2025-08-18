using System;
using System.Collections.Generic;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitSleeves.ViewModels.Filtration;

internal class ElementViewModel : BaseViewModel, IEquatable<ElementViewModel> {
    private readonly ElementModel _elementModel;

    public ElementViewModel(ElementModel elementModel) {
        _elementModel = elementModel ?? throw new ArgumentNullException(nameof(elementModel));
        FileName = _elementModel.DocumentName;
        CategoryName = _elementModel.Category;
        Id = _elementModel.Id.GetIdValue();
        TypeName = _elementModel.Name;
    }

    public string FileName { get; }

    public string CategoryName { get; }

    public long Id { get; }

    public string TypeName { get; }

    public ElementModel ElementModel => _elementModel;


    public bool Equals(ElementViewModel other) {
        if(other is null) {
            return false;
        }
        if(ReferenceEquals(this, other)) {
            return true;
        }
        return Id == other.Id && FileName == other.FileName;
    }

    public override bool Equals(object obj) {
        return Equals(obj as ElementViewModel);
    }

    public override int GetHashCode() {
        int hashCode = 1822265032;
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(FileName);
        hashCode = hashCode * -1521134295 + Id.GetHashCode();
        return hashCode;
    }
}
