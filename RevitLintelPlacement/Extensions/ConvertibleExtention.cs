using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RevitLintelPlacement.Extensions {
    internal static class ConvertibleExtention {
        public static string GetDescription<T>(this T e) where T : IConvertible {
            if(e is Enum) {
                Type type = e.GetType();
                Array values = Enum.GetValues(type);

                foreach(int val in values) {
                    if(val == e.ToInt32(CultureInfo.CurrentCulture)) {
                        var memInfo = type.GetMember(type.GetEnumName(val));
                        var descriptionAttribute = memInfo[0]
                            .GetCustomAttributes(typeof(DescriptionAttribute), false)
                            .FirstOrDefault() as DescriptionAttribute;

                        if(descriptionAttribute != null) {
                            return descriptionAttribute.Description;
                        }
                    }
                }
            }

            return null;
        }
    }
}
