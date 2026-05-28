using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;

using RevitUnmodelingMep.Models;

namespace RevitUnmodelingMep.ViewModels;

internal sealed class ConsumableTemplateManager {
    private readonly VisSettingsStorage _settingsStorage;
    private readonly Func<string, CategoryOption> _resolveCategoryOption;
    private readonly Dictionary<string, TemplateConsumable> _templateConfigs;
    private ObservableCollection<ConsumableTypeItem> _items;

    /// <summary>
    /// Создает менеджер предупреждений и загружает шаблонные расходники для дальнейшего сравнения.
    /// </summary>
    public ConsumableTemplateManager(
        VisSettingsStorage settingsStorage,
        Func<string, CategoryOption> resolveCategoryOption) {
        _settingsStorage = settingsStorage;
        _resolveCategoryOption = resolveCategoryOption;
        _templateConfigs = LoadTemplateConfigs();
    }

    /// <summary>
    /// Подключает менеджер к текущей коллекции расходников и обновляет все флаги предупреждений.
    /// </summary>
    public void Attach(ObservableCollection<ConsumableTypeItem> items) {
        if(ReferenceEquals(_items, items)) {
            RefreshWarnings();
            return;
        }

        Detach();

        _items = items;
        if(_items == null) {
            return;
        }

        _items.CollectionChanged += OnItemsCollectionChanged;
        foreach(ConsumableTypeItem item in _items) {
            AttachItem(item);
        }

        RefreshWarnings();
    }

    /// <summary>
    /// Проверяет, есть ли в шаблоне расходники, которые отсутствуют в текущей коллекции.
    /// </summary>
    public bool HasMissingTemplateItems() {
        return _items != null && _templateConfigs.Values.Any(template => !ContainsTemplateItem(template));
    }

    /// <summary>
    /// Приводит существующие шаблонные расходники к значениям из шаблона и при необходимости добавляет отсутствующие.
    /// </summary>
    public void ApplyTemplatesToItems(bool addMissingItems) {
        if(_items == null) {
            return;
        }

        var usedConfigKeys = new HashSet<string>(
            _items
                .Select(item => item?.ConfigKey)
                .Where(key => !string.IsNullOrWhiteSpace(key)),
            StringComparer.OrdinalIgnoreCase);

        foreach(TemplateConsumable template in _templateConfigs.Values) {
            ConsumableTypeItem existingItem = _items.FirstOrDefault(item =>
                string.Equals(
                    item?.ConsumableTypeName,
                    template.Config.ConfigName,
                    StringComparison.CurrentCultureIgnoreCase));

            if(existingItem != null) {
                ApplyTemplateToItem(existingItem, template.Config, keepAssignments: true);
                continue;
            }

            if(!addMissingItems) {
                continue;
            }

            string configKey = GetUniqueConfigKey(template.ConfigKey, usedConfigKeys);
            usedConfigKeys.Add(configKey);

            ConsumableTypeItem newItem = ConsumableTypeItem.FromConfig(configKey, template.Config);
            newItem.SelectedCategory = _resolveCategoryOption?.Invoke(newItem.CategoryId);
            _items.Add(newItem);
        }

        RefreshWarnings();
    }

    /// <summary>
    /// Проверяет, есть ли в текущей коллекции расходник с именем указанного шаблонного расходника.
    /// </summary>
    private bool ContainsTemplateItem(TemplateConsumable template) {
        return _items.Any(item =>
            string.Equals(
                item?.ConsumableTypeName,
                template.Config.ConfigName,
                StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Снимает подписки с ранее подключенной коллекции и ее элементов.
    /// </summary>
    private void Detach() {
        if(_items == null) {
            return;
        }

        _items.CollectionChanged -= OnItemsCollectionChanged;
        foreach(ConsumableTypeItem item in _items) {
            DetachItem(item);
        }

        _items = null;
    }

    /// <summary>
    /// Загружает шаблонные расходники и индексирует их по названию шаблонного расходника.
    /// </summary>
    private Dictionary<string, TemplateConsumable> LoadTemplateConfigs() {
        UnmodelingSettingsDocument defaultSettings = _settingsStorage.GetDefaultSettings();
        IReadOnlyDictionary<string, UnmodelingConfigItem> configs = defaultSettings?.UnmodelingConfig;

        return configs?
            .Where(pair => !string.IsNullOrWhiteSpace(pair.Value?.ConfigName))
            .GroupBy(pair => pair.Value.ConfigName, StringComparer.CurrentCultureIgnoreCase)
            .ToDictionary(
                group => group.Key,
                group => new TemplateConsumable(group.First().Key, group.First().Value),
                StringComparer.CurrentCultureIgnoreCase)
               ?? new Dictionary<string, TemplateConsumable>(StringComparer.CurrentCultureIgnoreCase);
    }

    /// <summary>
    /// Синхронизирует подписки при добавлении и удалении расходников из коллекции.
    /// </summary>
    private void OnItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
        if(e.OldItems != null) {
            foreach(ConsumableTypeItem item in e.OldItems.OfType<ConsumableTypeItem>()) {
                DetachItem(item);
            }
        }

        if(e.NewItems != null) {
            foreach(ConsumableTypeItem item in e.NewItems.OfType<ConsumableTypeItem>()) {
                AttachItem(item);
                RefreshWarning(item);
            }
        }
    }

    /// <summary>
    /// Подписывается на изменения полей одного расходника, чтобы предупреждение обновлялось сразу.
    /// </summary>
    private void AttachItem(ConsumableTypeItem item) {
        if(item == null) {
            return;
        }

        item.PropertyChanged += OnItemPropertyChanged;
    }

    /// <summary>
    /// Снимает подписку на изменения полей одного расходника.
    /// </summary>
    private void DetachItem(ConsumableTypeItem item) {
        if(item == null) {
            return;
        }

        item.PropertyChanged -= OnItemPropertyChanged;
    }

    /// <summary>
    /// Пересчитывает предупреждение одного расходника после изменения редактируемых полей.
    /// </summary>
    private void OnItemPropertyChanged(object sender, PropertyChangedEventArgs e) {
        if(sender is ConsumableTypeItem item && e.PropertyName != nameof(ConsumableTypeItem.IsTemplateDifferenceVisible)) {
            RefreshWarning(item);
        }
    }

    /// <summary>
    /// Пересчитывает видимость предупреждений для всех подключенных расходников.
    /// </summary>
    private void RefreshWarnings() {
        if(_items == null) {
            return;
        }

        foreach(ConsumableTypeItem item in _items) {
            RefreshWarning(item);
        }
    }

    /// <summary>
    /// Обновляет флаг предупреждения одного расходника по результату сравнения с шаблоном.
    /// </summary>
    private void RefreshWarning(ConsumableTypeItem item) {
        if(item == null) {
            return;
        }

        item.IsTemplateDifferenceVisible = IsDifferentFromTemplate(item);
    }

    /// <summary>
    /// Проверяет, что расходник все еще совпадает с шаблоном по имени, но отличается редактируемыми значениями.
    /// </summary>
    private bool IsDifferentFromTemplate(ConsumableTypeItem item) {
        if(string.IsNullOrWhiteSpace(item.ConsumableTypeName)
           || !_templateConfigs.TryGetValue(item.ConsumableTypeName, out TemplateConsumable template)) {
            return false;
        }

        UnmodelingConfigItem current = item.ToConfigItem();
        UnmodelingConfigItem templateConfig = template.Config;

        return !StringEquals(current.Category, ResolveTemplateCategory(templateConfig.Category))
               || !StringEquals(current.Group, templateConfig.Group)
               || !StringEquals(current.Name, templateConfig.Name)
               || !StringEquals(current.Mark, templateConfig.Mark)
               || !StringEquals(current.Code, templateConfig.Code)
               || !StringEquals(current.Unit, templateConfig.Unit)
               || !StringEquals(current.Creator, templateConfig.Creator)
               || !StringEquals(current.ValueFormula, templateConfig.ValueFormula)
               || !StringEquals(current.NoteValue, templateConfig.NoteValue)
               || !StringEquals(current.NoteFormat, templateConfig.NoteFormat)
               || current.RoundUpTotal != templateConfig.RoundUpTotal
               || current.RoundUpNoteTotal != templateConfig.RoundUpNoteTotal;
    }

    /// <summary>
    /// Копирует шаблонные значения в расходник, при необходимости сохраняя текущие назначения на элементы.
    /// </summary>
    private void ApplyTemplateToItem(
        ConsumableTypeItem item,
        UnmodelingConfigItem template,
        bool keepAssignments) {
        List<int> assignedElementIds = keepAssignments && item.AssignedElementIds != null
            ? new List<int>(item.AssignedElementIds)
            : template.AssignedElementIds != null
                ? new List<int>(template.AssignedElementIds)
                : new List<int>();

        item.Title = template.ConfigName;
        item.ConsumableTypeName = template.ConfigName;
        item.Name = template.Name;
        item.Grouping = template.Group;
        item.Mark = template.Mark;
        item.Code = template.Code;
        item.Unit = template.Unit;
        item.Maker = template.Creator;
        item.Formula = template.ValueFormula;
        item.NoteValue = template.NoteValue;
        item.Note = template.NoteFormat;
        item.RoundUpTotal = template.RoundUpTotal;
        item.RoundUpNoteTotal = template.RoundUpNoteTotal;
        item.SelectedCategory = _resolveCategoryOption?.Invoke(template.Category);
        item.CategoryId = item.SelectedCategory?.Id.ToString() ?? template.Category;
        item.AssignedElementIds = assignedElementIds;
        item.ExtensionData = template.ExtensionData != null
            ? new Dictionary<string, object>(template.ExtensionData)
            : null;
    }

    /// <summary>
    /// Возвращает свободный ключ конфигурации: сначала пытается использовать ключ из шаблона, затем генерирует следующий config_NNN.
    /// </summary>
    private string GetUniqueConfigKey(string templateConfigKey, HashSet<string> usedConfigKeys) {
        if(!string.IsNullOrWhiteSpace(templateConfigKey) && !usedConfigKeys.Contains(templateConfigKey)) {
            return templateConfigKey;
        }

        int nextIndex = GetMaxConfigIndex(usedConfigKeys) + 1;
        string configKey;
        do {
            configKey = $"config_{nextIndex:000}";
            nextIndex++;
        } while(usedConfigKeys.Contains(configKey));

        return configKey;
    }

    /// <summary>
    /// Находит максимальный числовой индекс среди ключей вида config_NNN.
    /// </summary>
    private static int GetMaxConfigIndex(IEnumerable<string> configKeys) {
        int maxIndex = 0;
        foreach(string configKey in configKeys ?? Enumerable.Empty<string>()) {
            Match match = Regex.Match(configKey ?? string.Empty, @"config_(\d+)", RegexOptions.IgnoreCase);
            if(match.Success && int.TryParse(match.Groups[1].Value, out int index) && index > maxIndex) {
                maxIndex = index;
            }
        }

        return maxIndex;
    }

    /// <summary>
    /// Возвращает идентификатор категории шаблона в том виде, в котором он хранится у редактируемого расходника.
    /// </summary>
    private string ResolveTemplateCategory(string category) {
        return _resolveCategoryOption?.Invoke(category)?.Id.ToString() ?? category ?? string.Empty;
    }

    /// <summary>
    /// Сравнивает nullable-строки как пустые строки по правилам текущей культуры,
    /// чтобы не показывать предупреждение о расхождении шаблона только из-за разницы между null и пустым значением.
    /// </summary>
    private static bool StringEquals(string left, string right) {
        return string.Equals(left ?? string.Empty, right ?? string.Empty, StringComparison.CurrentCulture);
    }

    /// <summary>
    /// Хранит шаблонный расходник вместе с исходным ключом конфигурации,
    /// чтобы при добавлении недостающего расходника сохранить ключ из шаблона или проверить его на конфликт.
    /// </summary>
    private sealed class TemplateConsumable {
        /// <summary>
        /// Создает внутреннее представление шаблонного расходника и подставляет пустую конфигурацию,
        /// если из настроек пришло null-значение.
        /// </summary>
        public TemplateConsumable(string configKey, UnmodelingConfigItem config) {
            ConfigKey = configKey;
            Config = config ?? new UnmodelingConfigItem();
        }

        /// <summary>
        /// Исходный ключ шаблона в словаре настроек, который используется при создании недостающего расходника.
        /// </summary>
        public string ConfigKey { get; }

        /// <summary>
        /// Значения шаблона, с которыми сравнивается и синхронизируется редактируемый расходник.
        /// </summary>
        public UnmodelingConfigItem Config { get; }
    }
}
