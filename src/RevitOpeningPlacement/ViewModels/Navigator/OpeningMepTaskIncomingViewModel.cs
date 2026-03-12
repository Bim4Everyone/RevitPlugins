using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Extensions;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator;
/// <summary>
/// Модель представления окна для работы с конкретным входящим заданием на отверстие от инженера в файле архитектора или конструктора
/// </summary>
internal class OpeningMepTaskIncomingViewModel : BaseViewModel,
    IOpeningTaskIncomingToKrViewModel,
    IOpeningMepTaskIncomingToArViewModel,
    IEquatable<OpeningMepTaskIncomingViewModel> {
    /// <summary>
    /// Экземпляр семейства задания на отверстие
    /// </summary>
    private readonly OpeningMepTaskIncoming _openingTask;

    public OpeningMepTaskIncomingViewModel(OpeningMepTaskIncoming incomingOpeningTask) {
        if(incomingOpeningTask is null) {
            throw new ArgumentNullException(nameof(incomingOpeningTask));
        }
        _openingTask = incomingOpeningTask;

        OpeningId = _openingTask.Id;
        FileName = Path.GetFileNameWithoutExtension(incomingOpeningTask.FileName);
        Date = _openingTask.Date.Split().FirstOrDefault() ?? string.Empty;
        MepSystem = _openingTask.MepSystem;
        Description = _openingTask.Description;
        CenterOffset = _openingTask.CenterOffset;
        BottomOffset = _openingTask.BottomOffset;
        Diameter = _openingTask.DisplayDiameter;
        Width = _openingTask.DisplayWidth;
        Height = _openingTask.DisplayHeight;
        Thickness = _openingTask.DisplayThickness;
        FamilyShortName = _openingTask.FamilyShortName;
        Host = _openingTask.Host is null ? new OpeningKrHost() : new OpeningKrHost(_openingTask.Host);
        Status = _openingTask.Status.GetDescription();
        Comment = _openingTask.Comment;
        Username = _openingTask.Username;
    }

    public ElementId OpeningId { get; }

    public string FileName { get; }

    public string Date { get; }

    public string MepSystem { get; }

    public string Description { get; }

    public string CenterOffset { get; }

    public string BottomOffset { get; }

    public string Diameter { get; }

    public string Width { get; }

    public string Height { get; }

    public string Thickness { get; }

    public string Status { get; }

    public string FamilyShortName { get; }

    public string Comment { get; }

    public string Username { get; }

    IOpeningHost IOpeningMepTaskIncomingToArViewModel.Host => Host;

    public IOpeningKrHost Host { get; }

    public override bool Equals(object obj) {
        return (obj != null)
               && (obj is OpeningMepTaskIncomingViewModel vmOther)
            && Equals(vmOther);
    }

    public override int GetHashCode() {
        return (int) (OpeningId.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(OpeningMepTaskIncomingViewModel other) {
        return (other != null)
            && (OpeningId == other.OpeningId)
            && FileName.Equals(other.FileName);
    }

    /// <summary>
    /// Возвращает хост входящего задания на отверстие
    /// </summary>
    public Element GetElementToHighlight() {
        return _openingTask.Host;
    }

    /// <summary>
    /// Возвращает коллекцию элементов, в которой находится входящее задание на отверстие, которое надо выделить на виде
    /// </summary>
    public ICollection<ElementModel> GetElementsToSelect() {
        return new ElementModel[] {
            new(_openingTask.GetFamilyInstance(), _openingTask.Transform)
        };
    }
}
