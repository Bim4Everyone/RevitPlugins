using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Уникальное задание на отверстие от ВИС. Использовать для заданий произвольной формы.
/// </summary>
internal class OpeningMepTaskOutcomingUniqueViewModel : BaseViewModel,
    IEquatable<OpeningMepTaskOutcomingUniqueViewModel>,
    IOpeningMepTaskOutcomingViewModel {
    private readonly FamilyInstance _opening;
    private readonly ElementId _id;

    public OpeningMepTaskOutcomingUniqueViewModel(FamilyInstance opening, string status) {
        if(string.IsNullOrWhiteSpace(status)) {
            throw new ArgumentException(nameof(status));
        }

        _opening = opening ?? throw new ArgumentNullException(nameof(opening));
        _id = _opening.Id;

        OpeningId = _opening.Id.ToString();
        Status = status;
        Comment = opening.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
        Date = string.Empty;
        MepSystem = string.Empty;
        Description = string.Empty;
        CenterOffset = string.Empty;
        BottomOffset = string.Empty;
        Username = string.Empty;
    }

    public string OpeningId { get; }
    public string Date { get; }
    public string MepSystem { get; }
    public string Description { get; }
    public string CenterOffset { get; }
    public string BottomOffset { get; }
    public string Status { get; }
    public string Comment { get; }
    public string Username { get; }

    public ICollection<ElementModel> GetElementsToSelect() {
        return [new ElementModel(_opening)];
    }

    public Element GetElementToHighlight() {
        return null;
    }

    public bool Equals(OpeningMepTaskOutcomingUniqueViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(_id, other._id);
    }

    public bool Equals(IOpeningMepTaskOutcomingViewModel other) {
        return Equals(other as OpeningMepTaskOutcomingUniqueViewModel);
    }

    public override bool Equals(object obj) {
        if(obj is null) {
            return false;
        }

        if(ReferenceEquals(this, obj)) {
            return true;
        }

        if(obj.GetType() != GetType()) {
            return false;
        }

        return Equals((OpeningMepTaskOutcomingUniqueViewModel) obj);
    }

    public override int GetHashCode() {
        return (int) _id.GetIdValue();
    }
}
