using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SystemParams;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator;

internal class OpeningMepTaskIncomingUniqueViewModel : BaseViewModel,
    IOpeningMepTaskIncomingToArViewModel,
    IOpeningTaskIncomingToKrViewModel,
    IEquatable<OpeningMepTaskIncomingUniqueViewModel> {
    private readonly FamilyInstance _opening;
    private readonly Transform _transform;

    public OpeningMepTaskIncomingUniqueViewModel(FamilyInstance opening, Transform transform, string status) {
        if(string.IsNullOrWhiteSpace(status)) {
            throw new ArgumentException(nameof(status));
        }

        _opening = opening ?? throw new ArgumentNullException(nameof(opening));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));

        OpeningId = _opening.Id;
        FileName = _opening.Document.Title;
        Status = status;
        Comment = _opening.GetParamValueStringOrDefault(
            SystemParamsConfig.Instance.CreateRevitParam(
                _opening.Document,
                BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS),
            string.Empty);
        Host = new OpeningKrHost();
        FamilyShortName = string.Empty;
        Diameter = string.Empty;
        Height = string.Empty;
        Width = string.Empty;
        Date = string.Empty;
        Thickness = string.Empty;
        CenterOffset = string.Empty;
        BottomOffset = string.Empty;
        MepSystem = string.Empty;
        Username = string.Empty;
        Description = string.Empty;
    }

    public ElementId OpeningId { get; }
    public string FileName { get; }
    public string Diameter { get; }
    public string Height { get; }
    public string Width { get; }
    public string Status { get; }
    public string Date { get; }
    public string Comment { get; }
    public string Thickness { get; }
    public string CenterOffset { get; }
    public string BottomOffset { get; }
    public string MepSystem { get; }
    public string Username { get; }
    public string FamilyShortName { get; }
    public string Description { get; }
    IOpeningHost IOpeningMepTaskIncomingToArViewModel.Host => Host;
    public IOpeningKrHost Host { get; }

    public ICollection<ElementModel> GetElementsToSelect() {
        return [new ElementModel(_opening, _transform)];
    }

    public Element GetElementToHighlight() {
        return null;
    }

    public bool Equals(OpeningMepTaskIncomingUniqueViewModel other) {
        if(other is null) {
            return false;
        }

        if(ReferenceEquals(this, other)) {
            return true;
        }

        return Equals(OpeningId, other.OpeningId) && FileName == other.FileName;
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

        return Equals((OpeningMepTaskIncomingUniqueViewModel) obj);
    }

    public override int GetHashCode() {
        unchecked {
            return ((OpeningId != null ? OpeningId.GetHashCode() : 0) * 397)
                   ^ (FileName != null ? FileName.GetHashCode() : 0);
        }
    }
}
