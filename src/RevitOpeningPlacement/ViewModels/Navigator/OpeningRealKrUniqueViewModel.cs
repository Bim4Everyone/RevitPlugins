using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Уникальное чистовое отверстие КР. Использовать для чистовых отверстий произвольной формы. 
/// </summary>
internal class OpeningRealKrUniqueViewModel : BaseViewModel, IOpeningRealKrViewModel,
    IEquatable<OpeningRealKrUniqueViewModel> {
    private readonly FamilyInstance _opening;

    public OpeningRealKrUniqueViewModel(FamilyInstance opening, string status) {
        if(string.IsNullOrWhiteSpace(status)) {
            throw new ArgumentException(nameof(status));
        }

        _opening = opening ?? throw new ArgumentNullException(nameof(opening));

        OpeningId = _opening.Id;
        Status = status;
        Comment = opening.GetParamValueStringOrDefault(
            SystemParamsConfig.Instance.CreateRevitParam(
                _opening.Document,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
            string.Empty);
        LevelName = _opening.LevelId.IsNotNull() ? _opening.Document.GetElement(_opening.LevelId).Name : string.Empty;
        FamilyName = _opening.Symbol.FamilyName;
        Host = new OpeningKrHost(_opening.Host);
        Diameter = string.Empty;
        Width = string.Empty;
        Height = string.Empty;
        TaskInfo = string.Empty;
    }

    public ElementId OpeningId { get; }
    public string Diameter { get; }
    public string Width { get; }
    public string Height { get; }
    public string Status { get; }
    public string Comment { get; }
    public string LevelName { get; }
    public string TaskInfo { get; }
    public string FamilyName { get; }
    public IOpeningKrHost Host { get; }

    public ICollection<ElementModel> GetElementsToSelect() {
        return [new ElementModel(_opening)];
    }

    public Element GetElementToHighlight() {
        return _opening.Host;
    }

    public bool Equals(OpeningRealKrUniqueViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(OpeningId, other.OpeningId);
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

        return Equals((OpeningRealKrUniqueViewModel) obj);
    }

    public override int GetHashCode() {
        return (OpeningId != null ? OpeningId.GetHashCode() : 0);
    }
}
