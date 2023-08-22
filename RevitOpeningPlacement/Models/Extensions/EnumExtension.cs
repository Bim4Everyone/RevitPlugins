using System.ComponentModel;
using System.Linq;

namespace RevitOpeningPlacement.Models.Extensions {
    internal static class EnumExtension {
        /// <summary>
        /// Возвращает значение <see cref="DescriptionAttribute"/>
        /// </summary>
        /// <typeparam name="TEnum"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static string GetEnumDescription<TEnum>(this TEnum item)
            => item?.GetType()?
                .GetField(item.ToString())?
                .GetCustomAttributes(typeof(DescriptionAttribute), false)
                .Cast<DescriptionAttribute>()
                .FirstOrDefault()?.Description ?? string.Empty;
    }
}
