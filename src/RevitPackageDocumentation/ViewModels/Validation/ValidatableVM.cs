using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

using dosymep.SimpleServices;
using dosymep.WPF.ViewModels;

using RevitPackageDocumentation.ViewModels.Validation.Attributes;

namespace RevitPackageDocumentation.ViewModels.Validation;

internal abstract class ValidatableVM : BaseViewModel, INotifyDataErrorInfo {
    private readonly ILocalizationService _localizationService;

    protected readonly Dictionary<string, List<string>> _errors = [];
    private readonly IReadOnlyDictionary<string, PropertyInfo> _propertyCache;
    private readonly Dictionary<object, PropertyChangedEventHandler> _subscriptions = [];


    private bool _hasErrors;
    private string _firstError;

    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    public bool HasErrors {
        get => _hasErrors;
        set => RaiseAndSetIfChanged(ref _hasErrors, value);
    }

    public string FirstError {
        get => _firstError;
        set => RaiseAndSetIfChanged(ref _firstError, value);
    }

    protected ValidatableVM(ILocalizationService localizationService) {
        _localizationService = localizationService;

        var props = GetType()
            .GetProperties(BindingFlags.Public | BindingFlags.Instance);

        // Сохраняем в кэш все свойства с атрибутами, чтобы потом быстрее обрабатывать ошибки
        _propertyCache = props
            .Where(p => p.CanRead && p.GetCustomAttributes<ValidationAttribute>().Any())
            .ToDictionary(p => p.Name, p => p);

        // Отбираем свойства с атрибутом ChildHasErrorsAttribute и подписываемся на изменения HasErrors
        foreach(var property in props.Where(
            x => x.GetCustomAttribute<ChildHasErrorsAttribute>() != null)) {
            RegisterTrackedCollection(property);
        }
    }

    /// <summary>
    /// Валидирует все свойства класса, помеченные атрибутами валидации
    /// </summary>
    public void ValidateAllProperties() {
        foreach(string propertyName in _propertyCache.Keys) {
            ValidateProperty(propertyName);
        }
    }

    private void ValidateProperty(string propertyName) {
        if(!_propertyCache.TryGetValue(propertyName, out var property))
            return;

        var value = property.GetValue(this);
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this) { MemberName = propertyName };

        Validator.TryValidateProperty(value, validationContext, validationResults);
        var localizedErrors = validationResults.Select(x => Localize(x.ErrorMessage)).ToList();

        UpdateErrors(propertyName, localizedErrors);
    }

    private string Localize(string key) {
        if(string.IsNullOrWhiteSpace(key))
            return string.Empty;

        string text = _localizationService.GetLocalizedString(key);
        return string.IsNullOrWhiteSpace(text) ? key : text;
    }

    private void UpdateErrors(string propertyName, IEnumerable<string> newErrors) {
        var errorsList = newErrors.ToList();
        var hasErrors = errorsList.Any();

        if(hasErrors) {
            _errors[propertyName] = errorsList;
        } else {
            _errors.Remove(propertyName);
        }

        OnErrorsChanged(propertyName);
        UpdateHasErrors();
        UpdateFirstError();
    }

    private void OnErrorsChanged(string propertyName) {
        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
    }

    /// <summary>
    /// Обновляет флаг наличия ошибки на объекте
    /// </summary>
    protected void UpdateHasErrors() {
        HasErrors = _errors.Any();
    }

    /// <summary>
    /// Обновляет значение ошибки на объекте (первой из всех имеющихся)
    /// </summary>
    protected void UpdateFirstError() {
        FirstError = _errors.Values
            .FirstOrDefault(errors => errors.Any())?
            .FirstOrDefault() ?? string.Empty;
    }

    /// <summary>
    /// Обеспечивает вызов валидации каждый раз при изменении свойства
    /// </summary>
    protected override void RaisePropertyChanged(string propertyName) {
        base.RaisePropertyChanged(propertyName);

        if(!string.IsNullOrEmpty(propertyName)
            && !propertyName.Equals(nameof(HasErrors), StringComparison.Ordinal)
            && !propertyName.Equals(nameof(FirstError), StringComparison.Ordinal)) {
            ValidateProperty(propertyName);
        }
    }

    public IEnumerable GetErrors(string propertyName) {
        //if(string.IsNullOrEmpty(propertyName)) {
        //    // Возвращаем все ошибки
        //    return _errors.Values.SelectMany(x => x).ToList();
        //}

        try {
            return _errors.TryGetValue(propertyName, out var errors) ? errors : new List<string>();
        } catch(Exception) {
            return new List<string>();
        }

        //return _errors.TryGetValue(propertyName, out var errors) ? errors : null;
    }


    /// <summary>
    /// Выполняем подписку на HasErrors у элементов коллекции
    /// </summary>
    private void RegisterTrackedCollection(PropertyInfo property) {
        if(property.GetValue(this) is not INotifyCollectionChanged collection)
            return;

        foreach(var item in (IEnumerable) collection) {
            Subscribe(item, property.Name);
        }

        collection.CollectionChanged += (_, e) => {
            if(e.NewItems != null) {
                foreach(var item in e.NewItems) {
                    Subscribe(item, property.Name);
                }
            }
            if(e.OldItems != null) {
                foreach(var item in e.OldItems) {
                    Unsubscribe(item);
                }
            }
            ValidateProperty(property.Name);
        };
    }

    private void Subscribe(object item, string propertyName) {
        if(item is not INotifyPropertyChanged notify)
            return;

        PropertyChangedEventHandler handler = (_, e) => {
            if(e.PropertyName == nameof(HasErrors)) {
                ValidateProperty(propertyName);
            }
        };
        notify.PropertyChanged += handler;
        _subscriptions[item] = handler;
    }

    private void Unsubscribe(object item) {
        if(item is not INotifyPropertyChanged notify)
            return;

        if(_subscriptions.TryGetValue(item, out var handler)) {
            notify.PropertyChanged -= handler;
            _subscriptions.Remove(item);
        }
    }
}
