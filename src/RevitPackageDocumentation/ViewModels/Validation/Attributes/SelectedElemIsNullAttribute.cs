using System.ComponentModel.DataAnnotations;

using RevitPackageDocumentation.ViewModels.Configuration.SheetSetParameters.Parameters;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;
internal class SelectedElemIsNullAttribute : ValidationAttribute {
    protected override ValidationResult IsValid(object value, ValidationContext context) {
        if(value is null || value is not SelectElemParamVM selectElemParam || selectElemParam.SelectedElem is null) {
            return new ValidationResult(ErrorMessage);
        }
        return ValidationResult.Success;
    }
}
