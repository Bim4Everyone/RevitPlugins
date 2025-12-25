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
}
