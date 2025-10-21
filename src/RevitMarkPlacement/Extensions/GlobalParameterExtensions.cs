using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Extensions;

internal static class GlobalParameterExtensions {
    public static double AsDouble(this GlobalParameter param) {
        var parameterValue = param.GetValue();
        if(parameterValue is not DoubleParameterValue doubleParameterValue) {
            throw new InvalidOperationException("Cannot convert parameter value to double. Parameter value must be of the type DoubleParameterValue.");
            
        }
        
#if REVIT_2021_OR_GREATER
        return UnitUtils.ConvertFromInternalUnits(doubleParameterValue.Value, UnitTypeId.Millimeters);
            
#else
        return UnitUtils.ConvertFromInternalUnits(doubleParameterValue.Value, DisplayUnitType.DUT_MILLIMETERS);
#endif
    }
}
