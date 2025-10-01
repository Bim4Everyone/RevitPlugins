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
internal class OpeningMepTaskIncomingViewModel : BaseViewModel, IOpeningTaskIncomingForKrViewModel, IEquatable<OpeningMepTaskIncomingViewModel> {
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


    /// <summary>
    /// Id экземпляра семейства задания на отверстие
    /// </summary>
    public ElementId OpeningId { get; } = ElementId.InvalidElementId;

    /// <summary>
    /// Название связанного файла-источника задания на отверстие
    /// </summary>
    public string FileName { get; } = string.Empty;

    /// <summary>
    /// Дата создания отверстия
    /// </summary>
    public string Date { get; } = string.Empty;

    /// <summary>
    /// Название инженерной системы, для элемента которой создано задание на отверстие
    /// </summary>
    public string MepSystem { get; } = string.Empty;

    /// <summary>
    /// Описание задания на отверстие
    /// </summary>
    public string Description { get; } = string.Empty;

    /// <summary>
    /// Отметка центра задания на отверстие
    /// </summary>
    public string CenterOffset { get; } = string.Empty;

    /// <summary>
    /// Отметка низа задания на отверстие
    /// </summary>
    public string BottomOffset { get; } = string.Empty;

    /// <summary>
    /// Диаметр
    /// </summary>
    public string Diameter { get; } = string.Empty;

    /// <summary>
    /// Ширина
    /// </summary>
    public string Width { get; } = string.Empty;

    /// <summary>
    /// Высота
    /// </summary>
    public string Height { get; } = string.Empty;

    /// <summary>
    /// Толщина
    /// </summary>
    public string Thickness { get; } = string.Empty;

    /// <summary>
    /// Статус задания на отверстие
    /// </summary>
    public string Status { get; } = string.Empty;

    /// <summary>
    /// Расположение отверстия - в перекрытии/в стене
    /// </summary>
    public string FamilyShortName { get; } = string.Empty;

    /// <summary>
    /// Комментарий экземпляра семейства задания на отверстие
    /// </summary>
    public string Comment { get; } = string.Empty;

    /// <summary>
    /// Имя пользователя, создавшего задание на отверстие
    /// </summary>
    public string Username { get; } = string.Empty;

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
