using System.Collections.Generic;

namespace RevitBuildCoordVolumes.Models.Interfaces;

internal interface IWindowService {
    /// <summary>
    /// Метод закрытия основного окна
    /// </summary>
    /// <remarks>
    /// В данном методе производится закрытие основного окна при появлении предупреждений    
    /// </remarks>
    /// <returns>
    /// Void.
    /// </returns>
    void CloseMainWindow();
    /// <summary>
    /// Метод открытия окна.
    /// </summary>
    /// <remarks>
    /// В данном методе производится открытие кона предупреждений для отображения WarningElement.
    /// </remarks>
    /// <param name="warningElements">Коллекция предупреждений WarningElement.</param>
    /// <returns>
    /// Void.
    /// </returns>
    void ShowWarningWindow(IReadOnlyCollection<WarningElement> warningElements);
}
