using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;
internal class ChildHasErrorsAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        if(value is IEnumerable<INotifyDataErrorInfo> items &&
            items.Any(x => x.HasErrors)) {
            return new ValidationResult(ErrorMessage);
        }

        return ValidationResult.Success;
    }
}
