namespace RevitOpeningPlacement.Services;
internal interface ILengthConverter {


    /// <summary>
    /// Конвертирует заданное значение длины из футов в мм
    /// </summary>
    /// <param name="feetValue">Длина в футах</param>
    /// <returns>Длина в мм</returns>
    double ConvertFromInternal(double feetValue);

    /// <summary>
    /// Конвертирует заданное значение длины из мм в футы
    /// </summary>
    /// <param name="mmValue">Длина в мм</param>
    /// <returns>Длина в футах</returns>
    double ConvertToInternal(double mmValue);
}
