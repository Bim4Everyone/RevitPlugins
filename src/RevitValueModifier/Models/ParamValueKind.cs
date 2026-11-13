namespace RevitValueModifier.Models;
/// <summary>
/// Вид значения параметра с точки зрения записи плагином
/// </summary>
internal enum ParamValueKind {
    /// <summary>
    /// Запись в параметр с таким типом данных не поддерживается (например, материал, тип, уровень)
    /// </summary>
    Unsupported,
    /// <summary>
    /// Текст
    /// </summary>
    Text,
    /// <summary>
    /// Целое число
    /// </summary>
    Integer,
    /// <summary>
    /// Да/Нет
    /// </summary>
    YesNo,
    /// <summary>
    /// Вещественное число: число, денежная единица, длина, площадь, объем, угол и другие величины с единицами
    /// </summary>
    Double
}
