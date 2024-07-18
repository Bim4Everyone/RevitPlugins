namespace RevitApartmentPlans.Services {
    /// <summary>
    /// Сервис для конвертации длин между метрической и имперской системами мер
    /// </summary>
    internal interface ILengthConverter {
        /// <summary>
        /// Конвертирует длину из мм в футы
        /// </summary>
        /// <param name="mmValue">Длина в мм</param>
        /// <returns>Длина в футах</returns>
        double ConvertToInternal(double mmValue);

        /// <summary>
        /// Конвертирует длину из футов в мм
        /// </summary>
        /// <param name="feetValue">Длина в футах</param>
        /// <returns>Длина в мм</returns>
        double ConvertFromInternal(double feetValue);
    }
}
