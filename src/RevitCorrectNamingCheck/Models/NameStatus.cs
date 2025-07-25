namespace RevitCorrectNamingCheck.Models;

/// <summary>
/// Статус имени для проверки на соответствие правилам. 
/// </summary>
public enum NameStatus {
    ///<summary>Имя не содержит ни одной метки раздела или не соответствует текущему разделу</summary>
    None,
    ///<summary>Имя корректное — содержит ровно одну подходящую метку раздела и метку "связ"</summary>
    Correct,
    ///<summary>Имя содержит допустимую метку раздела, но отсутствует метка "связ"</summary>
    PartialCorrect,
    ///<summary>Имя некорректное — содержит несколько меток раздела</summary>
    Incorrect
}
