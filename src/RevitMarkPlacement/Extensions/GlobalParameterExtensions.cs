using System;

using Autodesk.Revit.DB;

namespace RevitMarkPlacement.Extensions;

internal static class GlobalParameterExtensions {
    public static double AsDouble(this GlobalParameter param) {
        var parameterValue = param.GetValue();
        if(parameterValue is not DoubleParameterValue doubleParameterValue) {
            throw new InvalidOperationException("Cannot convert parameter value to double. Parameter value must be of the type DoubleParameterValue.");
            
        }
        
        return doubleParameterValue.Value;
    }
}
