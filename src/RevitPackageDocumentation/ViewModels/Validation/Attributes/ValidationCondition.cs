using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;

/// <summary>
/// Условие выполнения проверки: проверка выполняется, только если логическое свойство проверяемого объекта
/// имеет заданное значение
/// </summary>
internal static class ValidationCondition {
    /// <summary>
    /// Возвращает true, если условие не задано (propertyName пустое) или свойство объекта равно expectedValue
    /// </summary>
    public static bool IsMet(ValidationContext context, string propertyName, bool expectedValue) {
        if(string.IsNullOrEmpty(propertyName)) {
            return true;
        }
        object instance = context.ObjectInstance;
        object value = instance?.GetType().GetProperty(propertyName)?.GetValue(instance);
        return value is bool boolValue && boolValue == expectedValue;
    }
}
