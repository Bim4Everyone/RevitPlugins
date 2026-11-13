using System;
using System.Collections.Generic;
using System.IO;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Extensions;
using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.OpeningModels;
using RevitOpeningPlacement.OpeningModels.Enums;

namespace RevitOpeningPlacement.ViewModels.Navigator;
/// <summary>
/// Модель представления входящего задания на отверстие от архитектора в файле активном конструктора
/// </summary>
internal class OpeningArTaskIncomingViewModel : BaseViewModel,
    IOpeningTaskIncomingToKrViewModel,
    IEquatable<OpeningArTaskIncomingViewModel> {
    /// <summary>
    /// Экземпляр семейства проема АР, являющегося входящим заданием на отверстие для КР
    /// </summary>
    private readonly OpeningArTaskIncoming _openingTask;

    public OpeningArTaskIncomingViewModel(
        OpeningArTaskIncoming incomingOpeningTask,
        ILocalizationService localization) {
        _openingTask = incomingOpeningTask ?? throw new ArgumentNullException(nameof(incomingOpeningTask));

        OpeningId = _openingTask.Id;
        FileName = Path.GetFileNameWithoutExtension(_openingTask.FileName);
        Diameter = _openingTask.DisplayDiameter;
        Height = _openingTask.DisplayHeight;
        Width = _openingTask.DisplayWidth;
        Status = localization.GetLocalizedString($"{nameof(OpeningTaskIncomingStatus)}.{_openingTask.Status}");
        Comment = _openingTask.Comment;
        Host = _openingTask.Host is null ? new OpeningKrHost() : new OpeningKrHost(_openingTask.Host);
    }

    public ElementId OpeningId { get; }

    public string FileName { get; }

    /// <summary>
    /// Диаметр
    /// </summary>
    public string Diameter { get; } 

    /// <summary>
    /// Ширина
    /// </summary>
    public string Width { get; }

    /// <summary>
    /// Высота
    /// </summary>
    public string Height { get; }

    /// <summary>
    /// Статус задания на отверстие
    /// </summary>
    public string Status { get; }

    /// <summary>
    /// Комментарий экземпляра семейства задания на отверстие
    /// </summary>
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
        return (obj != null)
            && (obj is OpeningArTaskIncomingViewModel vmOther)
            && Equals(vmOther);
    }

    public override int GetHashCode() {
        return (int) (OpeningId.GetIdValue() + FileName.GetHashCode());
    }

    public bool Equals(OpeningArTaskIncomingViewModel other) {
        return (other != null)
            && (OpeningId == other.OpeningId)
            && FileName.Equals(other.FileName);
    }

    /// <summary>
    /// Возвращает коллекцию элементов, в которой находится входящее задание на отверстие для выделения на виде
    /// </summary>
    public ICollection<ElementModel> GetElementsToSelect() {
        return new ElementModel[] {
            new(_openingTask.GetFamilyInstance(), _openingTask.Transform)
        };
    }

    /// <summary>
    /// Возвращает хост входящего задания на отверстие
    /// </summary>
    public Element GetElementToHighlight() {
        return _openingTask.GetHost();
    }
}
