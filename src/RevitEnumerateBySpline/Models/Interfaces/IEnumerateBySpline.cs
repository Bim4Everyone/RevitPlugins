using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IEnumerateBySpline {
    /// <summary>Подготавливает помещения в порядке следования по линиям.</summary>
    void Enumerate(EnumerateBySplineSettings settings);
}
