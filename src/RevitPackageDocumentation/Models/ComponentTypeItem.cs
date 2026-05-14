using System;

namespace RevitPackageDocumentation.Models;
public class ComponentTypeItem {
    public Type ComponentType { get; set; }
    public string DisplayName { get; set; }

    public ComponentTypeItem(Type type, string displayName) {
        ComponentType = type;
        DisplayName = displayName;
    }
}
