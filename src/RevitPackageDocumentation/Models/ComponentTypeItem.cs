using System;

namespace RevitPackageDocumentation.Models;

/// <summary>
/// Элемент выпадающего списка добавления: тип модуля листа или параметра комплекта и его локализованное название
/// </summary>
public class ComponentTypeItem {
    /// <summary>
    /// Тип модели представления, экземпляр которой создается при выборе
    /// </summary>
    public Type ComponentType { get; set; }

    /// <summary>
    /// Название для отображения в списке
    /// </summary>
    public string DisplayName { get; set; }

    public ComponentTypeItem(Type type, string displayName) {
        ComponentType = type;
        DisplayName = displayName;
    }
}
