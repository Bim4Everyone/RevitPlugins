using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Bim4Everyone;
using dosymep.Bim4Everyone.SharedParams;
using dosymep.Revit;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Вентблок из активного файла АР. Использовать для навигатора АР.
/// <para>Вентблок является исходящим заданием на отверстие в перекрытии для КР.</para>
/// </summary>
internal class VentBlockArViewModel : BaseViewModel, IOpeningRealArViewModel,
    IEquatable<VentBlockArViewModel> {
    private readonly FamilyInstance _familyInstance;

    public VentBlockArViewModel(FamilyInstance ventBlock, string status) {
        if(string.IsNullOrWhiteSpace(status)) {
            throw new ArgumentException(nameof(status));
        }

        _familyInstance = ventBlock ?? throw new ArgumentNullException(nameof(ventBlock));
        Status = status;
        OpeningId = _familyInstance.Id;
        Comment = _familyInstance.GetParamValueOrDefault(BuiltInParameter.ALL_MODEL_INSTANCE_COMMENTS, string.Empty);
        LevelName = _familyInstance.LevelId.IsNotNull()
            ? _familyInstance.Document.GetElement(_familyInstance.LevelId).Name
            : string.Empty;
        FamilyName = _familyInstance.Symbol.FamilyName;
        Width = _familyInstance.Symbol.GetParam(SharedParamsConfig.Instance.SizeLength).AsValueString();
        Height = _familyInstance.Symbol.GetParam(SharedParamsConfig.Instance.SizeWidth).AsValueString();
        Diameter = string.Empty;
        TaskInfo = string.Empty;
    }

    public string Status { get; }
    public ElementId OpeningId { get; }
    public string Diameter { get; }
    public string Width { get; }
    public string Height { get; }
    public string Comment { get; }
    public string LevelName { get; }
    public string TaskInfo { get; }
    public string FamilyName { get; }

    public ICollection<ElementModel> GetElementsToSelect() {
        return [new ElementModel(_familyInstance)];
    }

    public Element GetElementToHighlight() {
        return _familyInstance.Host;
    }

    public bool Equals(VentBlockArViewModel other) {
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

        return Equals((VentBlockArViewModel) obj);
    }

    public override int GetHashCode() {
        return (OpeningId != null ? OpeningId.GetHashCode() : 0);
    }
}
