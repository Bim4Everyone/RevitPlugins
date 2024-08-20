using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace dosymep.WPF.Extensions {
    internal static class ConvertibleExtension {
        public static string GetDescription<T>(this T e) where T : IConvertible {
            if(e is Enum) {
                Type type = e.GetType();
                string name = Enum.GetName(type, e);
                FieldInfo info = type.GetField(name);

                DescriptionAttribute descriptionAttribute = info
                    .GetCustomAttributes(typeof(DescriptionAttribute), false)
                    .OfType<DescriptionAttribute>()
                    .FirstOrDefault();

                return descriptionAttribute?.Description ?? name;
            }
            
            return null;
        }
    }
}
