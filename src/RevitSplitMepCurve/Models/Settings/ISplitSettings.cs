using System.Collections.Generic;

using Autodesk.Revit.DB;

namespace RevitSplitMepCurve.Models.Settings;

internal interface ISplitSettings {
    /// <summary>Соединитель для труб и круглых воздуховодов.</summary>
    FamilySymbol ConnectorRoundSymbol { get; }

    /// <summary>Соединитель для прямоугольных воздуховодов.</summary>
    FamilySymbol ConnectorRectangleSymbol { get; }

    /// <summary>Уровни, на которых надо делить.</summary>
    ICollection<Level> Levels { get; }
}
