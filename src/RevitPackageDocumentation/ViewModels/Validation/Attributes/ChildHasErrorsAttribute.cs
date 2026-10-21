using System.Collections;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace RevitPackageDocumentation.ViewModels.Validation.Attributes;
internal class ChildHasErrorsAttribute : ValidationAttribute {
    /// <summary>
    /// Имя логического свойства объекта, от которого зависит проверка. Если не задано - проверка выполняется всегда
    /// </summary>
    public string ConditionPropertyName { get; set; }

    /// <summary>
    /// Значение свойства ConditionPropertyName, при котором выполняется проверка
    /// </summary>
    public bool ConditionValue { get; set; } = true;

    /// <summary>
    /// Не учитывать ошибки элементов коллекции, которые являются модулями со снятой галочкой
    /// (исключенными из генерации)
    /// </summary>
    public bool IgnoreUncheckedModules { get; set; }

    protected override ValidationResult IsValid(object value, ValidationContext context) {
        if(!ValidationCondition.IsMet(context, ConditionPropertyName, ConditionValue)) {
            return ValidationResult.Success;
        }
        switch(value) {
            case null:
                return ValidationResult.Success;

            case INotifyDataErrorInfo notify:
                return notify.HasErrors
                    ? new ValidationResult(ErrorMessage)
                    : ValidationResult.Success;

            case IEnumerable enumerable:
                foreach(var item in enumerable) {
                    if(IgnoreUncheckedModules && item is ICheckableModule { IsModuleCheck: false }) {
                        continue;
                    }
                    if(item is INotifyDataErrorInfo child && child.HasErrors) {
                        return new ValidationResult(ErrorMessage);
                    }
                }
                break;
        }
        return ValidationResult.Success;
    }
}
