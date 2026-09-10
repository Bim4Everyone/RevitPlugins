using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.ViewModels.Navigator;

/// <summary>
/// Модель представления вентблока из связи АР как входящего задания на отверстие в активном файле КР
/// </summary>
internal class VentBlockArIncomingViewModel : BaseViewModel,
    IOpeningTaskIncomingToKrViewModel,
    IEquatable<VentBlockArIncomingViewModel> {
    private readonly VentBlockAr _ventBlock;

    public VentBlockArIncomingViewModel(VentBlockAr ventBlock, ILocalizationService localization) {
        _ventBlock = ventBlock ?? throw new ArgumentNullException(nameof(ventBlock));
        if(localization is null) {
            throw new ArgumentNullException(nameof(localization));
        }

        OpeningId = _ventBlock.Id;
        FileName = Path.GetFileNameWithoutExtension(_ventBlock.FileName);
        Width = _ventBlock.DisplayWidth;
        Height = _ventBlock.DisplayHeight;
        Status = localization.GetLocalizedString($"{nameof(OpeningTaskIncomingStatus)}.{_ventBlock.Status}");
        Comment = _ventBlock.Comment;
        Host = _ventBlock.Host is null ? new OpeningKrHost() : new OpeningKrHost(_ventBlock.Host);
    }

    public ElementId OpeningId { get; }

    public string FileName { get; }

    public string Diameter => string.Empty;

    public string Width { get; }

    public string Height { get; }

    public string Status { get; }

    public string Comment { get; }

    public IOpeningKrHost Host { get; }

    public string Thickness => string.Empty;

    public string CenterOffset => string.Empty;

    public string BottomOffset => string.Empty;

    public string MepSystem => string.Empty;

    public string Username => string.Empty;

    public string FamilyShortName => string.Empty;

    public string Description => string.Empty;

    public string Date => string.Empty;

    public override bool Equals(object obj) {
        return (obj is VentBlockArIncomingViewModel vmOther) && Equals(vmOther);
    }

    public override int GetHashCode() {
        return (int) (OpeningId.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(VentBlockArIncomingViewModel other) {
        return (other != null)
               && (OpeningId == other.OpeningId)
               && FileName.Equals(other.FileName);
    }

    /// <summary>
    /// Возвращает коллекцию элементов, в которой находится вентблок для выделения на виде
    /// </summary>
    public ICollection<ElementModel> GetElementsToSelect() {
        return [new ElementModel(_ventBlock.GetFamilyInstance(), _ventBlock.Transform)];
    }

    /// <summary>
    /// Возвращает хост вентблока из активного файла КР
    /// </summary>
    public Element GetElementToHighlight() {
        return _ventBlock.Host;
    }
}
