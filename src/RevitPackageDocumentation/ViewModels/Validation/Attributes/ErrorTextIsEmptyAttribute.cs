using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;

/// <summary>
/// Валидация свойства, содержащего готовый текст ошибки: непустая строка считается ошибкой, 
/// текст строки выводится как сообщение
/// </summary>
internal class ErrorTextIsEmptyAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        return value is string errorText && !string.IsNullOrWhiteSpace(errorText)
            ? new ValidationResult(errorText)
            : ValidationResult.Success;
    }
}
