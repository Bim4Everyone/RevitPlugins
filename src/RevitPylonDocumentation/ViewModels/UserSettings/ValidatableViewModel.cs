using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

using dosymep.WPF.ViewModels;

namespace RevitPylonDocumentation.ViewModels.UserSettings;

internal abstract class ValidatableViewModel : BaseViewModel, INotifyDataErrorInfo {
    private readonly Dictionary<string, List<string>> _errors = new Dictionary<string, List<string>>();

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private bool _hasErrors;
    public bool HasErrors {
        get => _hasErrors;
        set => RaiseAndSetIfChanged(ref _hasErrors, value);
    }

    public IEnumerable GetErrors(string propertyName) {
        return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
    }

    protected void ValidateProperty(object value, [CallerMemberName] string propertyName = null) {
        var validationContext = new ValidationContext(this) { MemberName = propertyName };
        var validationResults = new List<ValidationResult>();
        Validator.TryValidateProperty(value, validationContext, validationResults);

        ClearErrors(propertyName);

        if(validationResults.Any()) {
            AddErrors(propertyName, validationResults.Select(r => r.ErrorMessage));
        }
    }

    private void UpdateHasErrors() {
        HasErrors = _errors.Any();
    }

    private void ClearErrors(string propertyName) {
        if(_errors.Remove(propertyName)) {
            OnErrorsChanged(propertyName);
            UpdateHasErrors();
        }
    }

    private void AddErrors(string propertyName, IEnumerable<string> errors) {
        if(!_errors.ContainsKey(propertyName)) {
            _errors[propertyName] = new List<string>();
        }

        _errors[propertyName].AddRange(errors);
        OnErrorsChanged(propertyName);
        UpdateHasErrors();
    }

    /// <summary>
    /// Валидирует все свойства класса, помеченные атрибутами валидации
    /// </summary>
    public void ValidateAllProperties() {
        var properties = GetType().GetProperties()
            .Where(p => p.CanRead && p.GetCustomAttributes(typeof(ValidationAttribute), true).Any());

        foreach(var property in properties) {
            var value = property.GetValue(this);
            ValidateProperty(value, property.Name);
        }
    }

    private void OnErrorsChanged(string propertyName) {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Метод для получения модели с заполненными свойствами на основе VM
    /// </summary>
    public T GetSettings<T>() where T : new() {
        var settings = new T();
        var vmType = this.GetType();
        var modelType = typeof(T);

        foreach(var prop in modelType.GetProperties()) {
            var vmProp = vmType.GetProperty(prop.Name);
            if(vmProp != null
               && vmProp.CanRead
               && prop.CanWrite
               && vmProp.PropertyType == prop.PropertyType) {
                var value = vmProp.GetValue(this);
                prop.SetValue(settings, value);
            }
        }
        return settings;
    }

    /// <summary>
    /// Метод по копированию значений из временных свойств в постоянные для использования в работе
    /// </summary>
    public void ApplySettings() {
        var vmType = this.GetType();
        var properties = vmType.GetProperties();

        foreach(var prop in properties) {
            // Проверяем, что свойство не содержит суффикс "Temp"
            if(!prop.Name.EndsWith("Temp")) {
                // Формируем имя свойства с суффиксом "Temp"
                string tempPropName = prop.Name + "Temp";
                var tempProp = vmType.GetProperty(tempPropName);

                // Проверяем, что свойство с суффиксом "Temp" существует,
                // оба свойства можно читать/писать и их типы совпадают
                if(tempProp != null
                    && prop.CanWrite
                    && tempProp.CanRead
                    && prop.PropertyType == tempProp.PropertyType) {
                    // Копируем значение из свойства с суффиксом "Temp" в основное свойство
                    var value = tempProp.GetValue(this);
                    prop.SetValue(this, value);
                }
            }
        }
    }

}
