using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;
internal class ChildHasErrorsAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        switch(value) {
            case null:
                return ValidationResult.Success;

            case INotifyDataErrorInfo notify:
                return notify.HasErrors
                    ? new ValidationResult(ErrorMessage)
                    : ValidationResult.Success;

            case IEnumerable enumerable:
                foreach(var item in enumerable) {
                    if(item is INotifyDataErrorInfo child && child.HasErrors) {
                        return new ValidationResult(ErrorMessage);
                    }
                }
                break;
        }
        return ValidationResult.Success;
    }
}
