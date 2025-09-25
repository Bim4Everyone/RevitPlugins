using System;
using System.ComponentModel;
using System.Linq;

namespace RevitLintelPlacement.Extensions;

internal static class ConvertibleExtention {
    public static string GetDescription<T>(this T e) where T : IConvertible {
        if(e is Enum) {
            var type = e.GetType();
            string name = Enum.GetName(type, e);
            var info = type.GetField(name);

            var descriptionAttribute = info
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .OfType<DescriptionAttribute>()
                .FirstOrDefault();

            return descriptionAttribute?.Description ?? name;
        }

        return null;
    }
}
