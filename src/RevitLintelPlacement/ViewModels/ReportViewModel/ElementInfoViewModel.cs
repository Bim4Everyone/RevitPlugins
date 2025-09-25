using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using Autodesk.Revit.DB;

using dosymep.WPF.ViewModels;

namespace RevitLintelPlacement.ViewModels;

internal class ElementInfoViewModel : BaseViewModel, IEquatable<ElementInfoViewModel> {
    private ElementId _elementId;
    private ElementType _elementType;
    private string _levelName;
    private string _message;
    private string _name;
    private TypeInfo _typeInfo;

    public ElementInfoViewModel(ElementId elementId, InfoElement infoElement) {
        ElementId = elementId;
        Message = infoElement.Message;
        ElementType = infoElement.ElementType;
        TypeInfo = infoElement.TypeInfo;
    }

    public ElementInfoViewModel(ElementId elementId, InfoElement infoElement, params string[] args) {
        ElementId = elementId;
        Message = infoElement.Message;
        ElementType = infoElement.ElementType;
        TypeInfo = infoElement.TypeInfo;
    }

    public TypeInfo TypeInfo {
        get => _typeInfo;
        set => RaiseAndSetIfChanged(ref _typeInfo, value);
    }

    public ElementType ElementType {
        get => _elementType;
        set => RaiseAndSetIfChanged(ref _elementType, value);
    }

    public bool IsCorrectValue => ElementId != ElementId.InvalidElementId;

    public string Message {
        get => _message;
        set => RaiseAndSetIfChanged(ref _message, value);
    }

    public ElementId ElementId {
        get => _elementId;
        set => RaiseAndSetIfChanged(ref _elementId, value);
    }

    public string LevelName {
        get => _levelName;
        set => RaiseAndSetIfChanged(ref _levelName, value);
    }

    public string Name {
        get => _name;
        set => RaiseAndSetIfChanged(ref _name, value);
    }

    public bool Equals(ElementInfoViewModel other) {
        return other != null
               && TypeInfo == other.TypeInfo
               && ElementType == other.ElementType
               && Message == other.Message
               && EqualityComparer<ElementId>.Default.Equals(ElementId, other.ElementId);
    }

    public override bool Equals(object obj) {
        return Equals(obj as ElementInfoViewModel);
    }

    public override int GetHashCode() {
        int hashCode = 897683154;
        hashCode = hashCode * -1521134295 + TypeInfo.GetHashCode();
        hashCode = hashCode * -1521134295 + ElementType.GetHashCode();
        hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Message);
        hashCode = hashCode * -1521134295 + EqualityComparer<ElementId>.Default.GetHashCode(ElementId);
        return hashCode;
    }
}

internal enum TypeInfo {
    [Display(Name = "Ошибки")]
    Error,

    [Display(Name = "Предупреждения")]
    Warning,

    [Display(Name = "Сообщения")]
    Info
}

internal enum ElementType {
    [Description("Перемычка")]
    Lintel,

    [Description("Проем")]
    Opening,

    [Description("Настройки")]
    Config,

    [Description("Вид")]
    View
}
