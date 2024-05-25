using System;
using System.ComponentModel;
using System.Reflection;

namespace RevitDeclarations.Models {
    public enum ContourCheckEnum {
        [Description("Ошибка контура")]
        Error,
        [Description("Да (проверить)")]
        YesCheck,
        [Description("Да")]
        Yes,
        [Description("Нет (проверить)")]
        NoCheck,
        [Description("Нет")]
        No
    }

    public static class EnumExtensions {
        public static string GetDescription(this Enum value) {
            Type type = value.GetType();
            string name = Enum.GetName(type, value);
            if(name != null) {
                FieldInfo field = type.GetField(name);
                if(field != null) {
                    DescriptionAttribute attr = Attribute
                        .GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if(attr != null) {
                        return attr.Description;
                    }
                }
            }
            return null;
        }
    }
}
