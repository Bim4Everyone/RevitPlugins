using System;
using System.Collections.Generic;
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
/// Модель представления окна для работы с конкретным исходящим заданием на отверстие в активном файле инженера
/// </summary>
internal class OpeningMepTaskOutcomingViewModel : BaseViewModel,
    IEquatable<OpeningMepTaskOutcomingViewModel>,
    IOpeningMepTaskOutcomingViewModel {

    /// <summary>
    /// Входящее задание на отверстие
    /// </summary>
    private readonly OpeningMepTaskOutcoming _openingTask;


    public OpeningMepTaskOutcomingViewModel(OpeningMepTaskOutcoming incomingOpeningTask) {
        if(incomingOpeningTask is null) {
            throw new ArgumentNullException(nameof(incomingOpeningTask));
        }
        _openingTask = incomingOpeningTask;

        OpeningId = _openingTask.Id.ToString();
        Date = _openingTask.Date.Split().FirstOrDefault() ?? string.Empty;
        MepSystem = _openingTask.MepSystem;
        Description = _openingTask.Description;
        CenterOffset = _openingTask.CenterOffset;
        BottomOffset = _openingTask.BottomOffset;
        Comment = _openingTask.Comment;
        Username = _openingTask.Username;

        Status = _openingTask.Status.GetDescription();
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

    public override bool Equals(object obj) {
        return (obj != null)
            && (obj is OpeningMepTaskOutcomingViewModel otherVM)
            && Equals(otherVM);
    }
    
    public override int GetHashCode() {
        return (int) _openingTask.Id.GetIdValue();
    }
    
    public bool Equals(OpeningMepTaskOutcomingViewModel other) {
        return (other != null)
            && (_openingTask.Id == other._openingTask.Id);
    }

    /// <summary>
    /// Возвращает хост исходящего задания на отверстие
    /// </summary>
    public Element GetElementToHighlight() {
        return _openingTask.Host;
    }

    /// <summary>
    /// Возвращает коллекцию элементов, в которой находится исходящее задание на отверстие, которое надо выделить на виде
    /// </summary>
    public ICollection<ElementModel> GetElementsToSelect() {
        return new ElementModel[] {
            new(_openingTask.GetFamilyInstance())
        };
    }
}
