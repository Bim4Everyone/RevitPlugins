using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

using dosymep.Revit;
using dosymep.WPF.Extensions;
using dosymep.WPF.ViewModels;

using RevitClashDetective.Models.Clashes;

using RevitOpeningPlacement.Models.Interfaces;
using RevitOpeningPlacement.Models.RealOpeningArPlacement;
using RevitOpeningPlacement.OpeningModels;

namespace RevitOpeningPlacement.ViewModels.Navigator;
/// <summary>
/// Модель представления чистового отверстия в навигаторе АР по входящим заданиям на отверстия.
/// Использовать для отображения чистовых отверстий АР из активного документа, которые требуют внимания архитектора
/// </summary>
internal class OpeningRealArViewModel : BaseViewModel, IOpeningRealArViewModel, IEquatable<OpeningRealArViewModel> {
    private readonly OpeningRealAr _openingReal;


    public OpeningRealArViewModel(OpeningRealAr openingReal) {
        _openingReal = openingReal ?? throw new System.ArgumentNullException(nameof(openingReal));

        OpeningId = _openingReal.Id;
        Diameter = _openingReal.Diameter;
        Width = _openingReal.Width;
        Height = _openingReal.Height;
        Status = _openingReal.Status.GetDescription();
        Comment = _openingReal.Comment;
        LevelName = GetLevelName(openingReal);
        TaskInfo = GetTaskInfo(openingReal);
        FamilyName = GetFamilyName(openingReal);
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


    public override bool Equals(object obj) {
        return (obj != null)
            && (obj is OpeningRealArViewModel otherVM)
            && Equals(otherVM);
    }

    public bool Equals(OpeningRealArViewModel other) {
        return (other != null)
            && (OpeningId == other.OpeningId);
    }

    public override int GetHashCode() {
        return (int) OpeningId.GetIdValue();
    }

    /// <summary>
    /// Возвращает хост чистового отверстия
    /// </summary>
    public Element GetElementToHighlight() {
        return _openingReal.GetHost();
    }

    /// <summary>
    /// Возвращает коллекцию, в которой находится чистовое отверстие, которое надо выделить на виде
    /// </summary>
    public ICollection<ElementModel> GetElementsToSelect() {
        return new ElementModel[] {
            new(_openingReal.GetFamilyInstance())
        };
    }

    private string GetLevelName(OpeningRealAr openingReal) {
        var famInst = openingReal.GetFamilyInstance();
        var doc = famInst.Document;
        return doc.GetElement(famInst.LevelId).Name;
    }

    private string GetFamilyName(OpeningRealAr openingReal) {
        return openingReal.GetFamilyInstance().Symbol.FamilyName;
    }

    private string GetTaskInfo(OpeningRealAr openingReal) {
        return openingReal.GetFamilyInstance()
            .GetSharedParamValueOrDefault<string>(RealOpeningArPlacer.RealOpeningTaskId);
    }
}
