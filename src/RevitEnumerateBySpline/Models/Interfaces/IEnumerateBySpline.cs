using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Interfaces;

internal interface IEnumerateBySpline {
    void Enumerate(EnumerateBySplineSettings settings);
}
