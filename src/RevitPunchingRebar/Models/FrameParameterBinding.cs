using pyRevitLabs.Json;

namespace RevitPunchingRebar.Models;

/// <summary>
/// Сохраняемое имя параметра и значение для переключателя «Да/Нет».
/// </summary>
internal sealed class FrameParameterBinding {
    public string TargetName { get; set; }

    [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
    public bool? YesNoValue { get; set; }
}
