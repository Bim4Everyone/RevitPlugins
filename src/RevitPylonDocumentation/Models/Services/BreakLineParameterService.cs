using Autodesk.Revit.DB;

using dosymep.Revit;

namespace RevitPylonDocumentation.Models.Services;
/// <summary>
/// Сервис для установки параметров линий обрыва
/// </summary>
internal class BreakLineParameterService {
    private readonly string _breakLineDepthParamName = "Глубина маскировки";
    private readonly double _breakLineDepthParamValue = 1500;
    private readonly string _breakLineLengthLeftParamName = "Длина маскировки_влево";
    private readonly double _breakLineLengthLeftParamValue = 100;
    private readonly string _breakLineLengthRightParamName = "Длина маскировки_право";
    private readonly double _breakLineLengthRightParamValue = 100;

    /// <summary>
    /// Устанавливает параметры линии обрыва.
    /// </summary>
    internal void TrySetBreakLineOffsets(Element breakLine) {
        if(breakLine.IsExistsParam(_breakLineDepthParamName)) {
            breakLine.SetParamValue(_breakLineDepthParamName, 
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineDepthParamValue));
        }
        if(breakLine.IsExistsParam(_breakLineLengthLeftParamName)) {
            breakLine.SetParamValue(_breakLineLengthLeftParamName, 
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineLengthLeftParamValue));
        }
        if(breakLine.IsExistsParam(_breakLineLengthRightParamName)) {
            breakLine.SetParamValue(_breakLineLengthRightParamName, 
                                    UnitUtilsHelper.ConvertToInternalValue(_breakLineLengthRightParamValue));
        }
    }
}
