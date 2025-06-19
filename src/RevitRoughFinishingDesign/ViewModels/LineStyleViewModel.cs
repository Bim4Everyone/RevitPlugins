using System;
using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitRoughFinishingDesign.ViewModels;
internal class LineStyleViewModel : IEquatable<LineStyleViewModel> {
    private readonly GraphicsStyle _graphicStyle;

    public bool IsNone { get; } = false;

    // Основной конструктор
    public LineStyleViewModel(GraphicsStyle graphicsStyle) {
        _graphicStyle = graphicsStyle ?? throw new ArgumentNullException(nameof(graphicsStyle));
    }

    // Доп. конструктор для "<Нет>"
    private LineStyleViewModel() {
        IsNone = true;
    }

    public string Name => IsNone ? "<Нет>" : _graphicStyle.Name;

    public ElementId GraphicStyleId => _graphicStyle?.Id;

    public override string ToString() {
        return Name;
    }

    public override bool Equals(object obj) {
        return Equals(obj as LineStyleViewModel);
    }

    public bool Equals(LineStyleViewModel other) {
        return other != null && ((IsNone && other.IsNone) || (!IsNone && !other.IsNone && _graphicStyle.Id == other._graphicStyle.Id));
    }

    public override int GetHashCode() {
        return IsNone
            ? 0
            : EqualityComparer<ElementId>.Default.GetHashCode(_graphicStyle.Id);
    }

    // Фабрика для "<Нет>"
    public static LineStyleViewModel None => new();
}

