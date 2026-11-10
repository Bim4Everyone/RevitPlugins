using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;

/// <summary>
/// Обязательное значение, если логическое свойство объекта с именем propertyName равно expectedValue.
/// В остальных случаях значение не проверяется
/// </summary>
internal class RequiredIfAttribute : ValidationAttribute {
    private readonly string _propertyName;
    private readonly bool _expectedValue;

    public RequiredIfAttribute(string propertyName, bool expectedValue = true) {
        _propertyName = propertyName;
        _expectedValue = expectedValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext context) {
        if(!ValidationCondition.IsMet(context, _propertyName, _expectedValue)) {
            return ValidationResult.Success;
        }
        return value is null || value is string text && string.IsNullOrWhiteSpace(text)
            ? new ValidationResult(ErrorMessage)
            : ValidationResult.Success;
    }
}
