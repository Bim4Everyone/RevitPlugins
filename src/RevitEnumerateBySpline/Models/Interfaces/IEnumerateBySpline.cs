using RevitEnumerateBySpline.Models.Settings;

namespace RevitEnumerateBySpline.Models.Interfaces;

/// <summary>Выполняет нумерацию помещений по сплайну.</summary>
internal interface IEnumerateBySpline {
    /// <summary>Подготавливает помещения в порядке следования по линиям.</summary>
    /// <param name="settings">Настройки нумерации и выбранные элементы.</param>
    void Enumerate(EnumerateBySplineSettings settings);
}
