using System.Globalization;
using System.Numerics;
using System.Text.RegularExpressions;

namespace RevitVORChecker.Core;

/// <summary>
/// Форматирование и округление чисел так же, как в эталонных скриптах проверки на Python,
/// чтобы тексты замечаний плагина совпадали с эталоном.
/// </summary>
internal static class PythonFormat {
    public static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

    /// <summary>
    /// Текст числа как str() в Python: целое — без дробной части, дробное — кратчайшее представление.
    /// </summary>
    public static string NumberText(double value) {
        if(Math.Floor(value) == value && Math.Abs(value) < 1e16) {
            return value.ToString("0", Invariant);
        }

        return value.ToString("R", Invariant);
    }

    /// <summary>
    /// Текст дробного числа как str(float) в Python: «3.0», «0.1».
    /// </summary>
    public static string FloatText(double value) {
        if(Math.Floor(value) == value && Math.Abs(value) < 1e16) {
            return value.ToString("0", Invariant) + ".0";
        }

        return value.ToString("R", Invariant);
    }

    /// <summary>
    /// round(value, digits) в Python: точное округление двоичного значения, половина — к чётному.
    /// </summary>
    public static double Round(double value, int digits) {
        return double.Parse(FormatFixed(value, digits), Invariant);
    }

    /// <summary>
    /// "%.{digits}f" % value в Python.
    /// </summary>
    public static string FormatFixed(double value, int digits) {
        if(double.IsNaN(value)) {
            return "nan";
        }

        if(double.IsInfinity(value)) {
            return value > 0 ? "inf" : "-inf";
        }

        BigInteger scaled = ScaledRound(value, digits);
        string text = Digits(scaled, digits);
        bool negativeZero = scaled.IsZero && (value < 0 || (value == 0 && 1 / value < 0));
        return negativeZero ? "-" + text : text;
    }

    /// <summary>
    /// "%g" % value в Python: 6 значащих цифр, без хвостовых нулей, экспонента при порядке &lt; -4 или ≥ 6.
    /// </summary>
    public static string GeneralFormat(double value) {
        if(double.IsNaN(value)) {
            return "nan";
        }

        if(double.IsInfinity(value)) {
            return value > 0 ? "inf" : "-inf";
        }

        if(value == 0) {
            return 1 / value < 0 ? "-0" : "0";
        }

        const int precision = 6;
        int exponent = (int) Math.Floor(Math.Log10(Math.Abs(value)));
        BigInteger scaled = ScaledRound(value, precision - 1 - exponent);
        if(BigInteger.Abs(scaled) >= BigInteger.Pow(10, precision)) {
            exponent++;
            scaled = ScaledRound(value, precision - 1 - exponent);
        } else if(BigInteger.Abs(scaled) < BigInteger.Pow(10, precision - 1)) {
            exponent--;
            scaled = ScaledRound(value, precision - 1 - exponent);
        }

        if(exponent < -4 || exponent >= precision) {
            string digits = BigInteger.Abs(scaled).ToString(Invariant);
            string mantissa = (digits.Substring(0, 1) + "." + digits.Substring(1)).TrimEnd('0').TrimEnd('.');
            string sign = scaled.Sign < 0 ? "-" : "";
            string exponentSign = exponent < 0 ? "-" : "+";
            return sign + mantissa + "e" + exponentSign + Math.Abs(exponent).ToString("00", Invariant);
        }

        string text = Digits(scaled, precision - 1 - exponent);
        if(text.Contains(".")) {
            text = text.TrimEnd('0').TrimEnd('.');
        }

        return text;
    }

    /// <summary>
    /// Нормализация текста для сравнения: «ё» → «е», кавычки-ёлочки → прямые, пробелы схлопываются.
    /// </summary>
    public static string NormalizeText(string text) {
        string replaced = (text ?? "")
            .Replace("ё", "е")
            .Replace("Ё", "Е")
            .Replace("«", "\"")
            .Replace("»", "\"");
        return Regex.Replace(replaced, @"\s+", " ").Trim();
    }

    public static string Trim(string text) {
        return (text ?? "").Trim();
    }

    /// <summary>
    /// Сортировка строк по кодовым точкам, как sorted() в Python.
    /// </summary>
    public static List<string> SortOrdinal(IEnumerable<string> values) {
        var list = values.ToList();
        list.Sort(StringComparer.Ordinal);
        return list;
    }

    /// <summary>
    /// Counter.most_common(): по убыванию количества, при равенстве — в порядке первого появления.
    /// </summary>
    public static List<KeyValuePair<string, int>> MostCommon(IEnumerable<string> values) {
        var firstSeenOrder = new List<string>();
        var counts = new Dictionary<string, int>();
        foreach(string value in values) {
            if(!counts.ContainsKey(value)) {
                counts[value] = 0;
                firstSeenOrder.Add(value);
            }

            counts[value]++;
        }

        return firstSeenOrder
            .Select((value, index) => new { Value = value, Index = index })
            .OrderByDescending(item => counts[item.Value])
            .ThenBy(item => item.Index)
            .Select(item => new KeyValuePair<string, int>(item.Value, counts[item.Value]))
            .ToList();
    }

    /// <summary>
    /// Разложение double на целую мантиссу и двоичный порядок: value = mantissa · 2^exponent.
    /// </summary>
    private static void Decompose(double value, out BigInteger mantissa, out int exponent) {
        long bits = BitConverter.DoubleToInt64Bits(value);
        bool negative = bits < 0;
        int biasedExponent = (int) ((bits >> 52) & 0x7FF);
        long fraction = bits & 0xFFFFFFFFFFFFFL;
        if(biasedExponent == 0) {
            biasedExponent = 1;
        } else {
            fraction |= 1L << 52;
        }

        exponent = biasedExponent - 1075;
        mantissa = negative ? -(BigInteger) fraction : fraction;
    }

    /// <summary>
    /// Целое round_half_even(value · 10^power), вычисленное точно.
    /// </summary>
    private static BigInteger ScaledRound(double value, int power) {
        Decompose(value, out BigInteger mantissa, out int exponent);
        BigInteger numerator = BigInteger.Abs(mantissa);
        BigInteger denominator = BigInteger.One;
        if(exponent > 0) {
            numerator <<= exponent;
        } else {
            denominator <<= -exponent;
        }

        if(power > 0) {
            numerator *= BigInteger.Pow(10, power);
        } else if(power < 0) {
            denominator *= BigInteger.Pow(10, -power);
        }

        BigInteger quotient = BigInteger.DivRem(numerator, denominator, out BigInteger remainder);
        int comparison = (remainder * 2).CompareTo(denominator);
        if(comparison > 0 || (comparison == 0 && !quotient.IsEven)) {
            quotient += 1;
        }

        return mantissa.Sign < 0 ? -quotient : quotient;
    }

    private static string Digits(BigInteger scaled, int power) {
        bool negative = scaled.Sign < 0;
        string digits = BigInteger.Abs(scaled).ToString(Invariant);
        if(power > 0) {
            if(digits.Length <= power) {
                digits = new string('0', power - digits.Length + 1) + digits;
            }

            digits = digits.Substring(0, digits.Length - power) + "." + digits.Substring(digits.Length - power);
        } else if(power < 0) {
            digits += new string('0', -power);
        }

        return (negative ? "-" : "") + digits;
    }
}
