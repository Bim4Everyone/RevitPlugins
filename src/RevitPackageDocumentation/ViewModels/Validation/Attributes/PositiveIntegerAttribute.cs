using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;

/// <summary>
/// Строка должна содержать целое число больше нуля
/// </summary>
internal class PositiveIntegerAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        if(value is not string stringValue
            || !int.TryParse(stringValue, out int valueAsInt)
            || valueAsInt < 1) {
            return new ValidationResult(ErrorMessage);
        }
        return ValidationResult.Success;
    }
}
