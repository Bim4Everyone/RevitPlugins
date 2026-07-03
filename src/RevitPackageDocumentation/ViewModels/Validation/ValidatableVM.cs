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
    private readonly IReadOnlyDictionary<string, PropertyInfo> _trackedChildProperties;

    /// <summary>
    /// Подписки на PropertyChanged дочерних объектов
    /// </summary>
    private readonly Dictionary<object, PropertyChangedEventHandler> _childSubscriptions = [];

    /// <summary>
    /// Подписки на CollectionChanged коллекций
    /// </summary>
    private readonly Dictionary<INotifyCollectionChanged, NotifyCollectionChangedEventHandler> _collectionSubscriptions = [];

    /// <summary>
    /// Текущее значение отслеживаемого свойства, необходимо для корректной переподписки
    /// </summary>
    private readonly Dictionary<string, object> _trackedValues = [];

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

        _propertyCache = props
            .Where(x => x.CanRead && x.GetCustomAttributes<ValidationAttribute>().Any())
            .ToDictionary(x => x.Name);

        _trackedChildProperties = props
            .Where(x => x.GetCustomAttribute<ChildHasErrorsAttribute>() != null)
            .ToDictionary(x => x.Name);

        foreach(var property in _trackedChildProperties.Values) {
            RegisterTrackedProperty(property);
        }
    }

    public void ValidateAllProperties() {
        foreach(var propertyName in _propertyCache.Keys) {
            ValidateProperty(propertyName);
        }
    }

    private void ValidateProperty(string propertyName) {
        if(!_propertyCache.TryGetValue(propertyName, out var property))
            return;

        var value = property.GetValue(this);
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(this) {
            MemberName = propertyName
        };
        Validator.TryValidateProperty(value, validationContext, validationResults);

        var localizedErrors = validationResults.Select(x => Localize(x.ErrorMessage)).ToList();
        UpdateErrors(propertyName, localizedErrors);
    }

    private void UpdateErrors(string propertyName, IEnumerable<string> errors) {
        var list = errors.ToList();

        if(list.Any()) {
            _errors[propertyName] = list;
        } else {
            _errors.Remove(propertyName);
        }

        ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
        HasErrors = _errors.Any();
        FirstError = _errors.Values
            .FirstOrDefault(x => x.Any())
            ?.FirstOrDefault() ?? string.Empty;
    }

    private string Localize(string key) {
        if(string.IsNullOrWhiteSpace(key))
            return string.Empty;

        string text = _localizationService.GetLocalizedString(key);
        return string.IsNullOrWhiteSpace(text) ? key : text;
    }


    private void RegisterTrackedProperty(PropertyInfo property) {
        var value = property.GetValue(this);
        _trackedValues[property.Name] = value;

        switch(value) {
            case INotifyCollectionChanged collection:
                SubscribeCollection(property.Name, collection);
                break;

            case INotifyPropertyChanged notify:
                SubscribeChild(property.Name, notify);
                break;
        }
    }

    private void ReRegisterTrackedProperty(PropertyInfo property) {
        if(_trackedValues.TryGetValue(property.Name, out var oldValue)) {

            switch(oldValue) {
                case INotifyCollectionChanged collection:
                    UnsubscribeCollection(collection);
                    break;

                case INotifyPropertyChanged notify:
                    UnsubscribeChild(notify);
                    break;
            }
        }
        RegisterTrackedProperty(property);
        ValidateProperty(property.Name);
    }

    private void SubscribeCollection(
        string propertyName,
        INotifyCollectionChanged collection) {

        foreach(var item in (IEnumerable) collection) {
            SubscribeItem(propertyName, item);
        }

        NotifyCollectionChangedEventHandler handler = (_, e) => {
            if(e.NewItems != null) {
                foreach(var item in e.NewItems) {
                    SubscribeItem(propertyName, item);
                }
            }
            if(e.OldItems != null) {
                foreach(var item in e.OldItems) {
                    UnsubscribeChild(item);
                }
            }
            ValidateProperty(propertyName);
        };

        collection.CollectionChanged += handler;
        _collectionSubscriptions[collection] = handler;
    }

    private void SubscribeItem(string propertyName, object item) {
        if(item is INotifyPropertyChanged notify) {
            SubscribeChild(propertyName, notify);
        }
    }

    private void SubscribeChild(
        string propertyName,
        INotifyPropertyChanged notify) {

        PropertyChangedEventHandler handler = (_, e) => {
            if(e.PropertyName == nameof(HasErrors)) {
                ValidateProperty(propertyName);
            }
        };
        notify.PropertyChanged += handler;
        _childSubscriptions[notify] = handler;
    }

    private void UnsubscribeCollection(INotifyCollectionChanged collection) {
        if(_collectionSubscriptions.TryGetValue(collection, out var handler)) {
            collection.CollectionChanged -= handler;
            _collectionSubscriptions.Remove(collection);
        }

        foreach(var item in (IEnumerable) collection) {
            UnsubscribeChild(item);
        }
    }

    private void UnsubscribeChild(object item) {
        if(item is not INotifyPropertyChanged notify)
            return;

        if(_childSubscriptions.TryGetValue(notify, out var handler)) {
            notify.PropertyChanged -= handler;
            _childSubscriptions.Remove(notify);
        }
    }


    protected override void RaisePropertyChanged(string propertyName) {
        base.RaisePropertyChanged(propertyName);

        if(string.IsNullOrEmpty(propertyName))
            return;

        if(propertyName != nameof(HasErrors) && propertyName != nameof(FirstError)) {
            ValidateProperty(propertyName);
        }

        if(_trackedChildProperties.TryGetValue(propertyName, out var property)) {
            ReRegisterTrackedProperty(property);
        }
    }

    public IEnumerable GetErrors(string propertyName) {
        try {
            return _errors.TryGetValue(propertyName, out var errors)
                ? errors
                : Enumerable.Empty<string>();
        } catch {
            return Enumerable.Empty<string>();
        }
    }
}
