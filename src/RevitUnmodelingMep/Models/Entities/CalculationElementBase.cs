using System;

using Autodesk.Revit.DB;

namespace RevitUnmodelingMep.Models.Entities;

internal abstract class CalculationElementBase {
    protected CalculationElementBase(Element element) {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public Element Element { get; }

    public string SystemTypeName { get; set; }

    public string SystemSharedName { get; set; }

    protected static double? ToMillimeters(double? value) {
        return value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Millimeters)
            : null;
    }

    protected static double? ToMeters(double? value) {
        return value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.Meters)
            : null;
    }


    protected static double? ToSquareMeters(double? value) {
        return value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.SquareMeters)
            : null;
    }

    protected static double? ToCubicMeters(double? value) {
        return value.HasValue
            ? UnitUtils.ConvertFromInternalUnits(value.Value, UnitTypeId.CubicMeters)
            : null;
    }

    protected static double? MillimetersToMeters(double? valueMm) {
        return valueMm.HasValue
            ? valueMm.Value / 1000d
            : null;
    }
}

