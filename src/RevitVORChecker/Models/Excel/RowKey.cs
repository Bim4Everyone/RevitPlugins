using RevitVORChecker.Core;

namespace RevitVORChecker.Models.Excel;

/// <summary>
/// Значение столбца строки ВОР: число (округлённое до точности столбца) или нормализованный текст.
/// </summary>
internal class RowKeyItem {
    public bool IsNumber { get; set; }

    public double Number { get; set; }

    public string Text { get; set; }
}

/// <summary>
/// Строка таблицы ВОР как набор значений столбцов — для сравнения строк модели и Excel.
/// </summary>
internal class RowKey {
    public RowKey(List<RowKeyItem> items) {
        Items = items;
        Code = string.Join("\u0002", items.Select(item => item.IsNumber
            ? "n\u0001" + (item.Number == 0 ? 0.0 : item.Number).ToString("R", PythonFormat.Invariant)
            : "s\u0001" + item.Text));
    }

    public List<RowKeyItem> Items { get; }

    /// <summary>
    /// Строковый код строки для сравнения.
    /// </summary>
    public string Code { get; }
}

/// <summary>
/// Мультимножество строк (одинаковые строки считаются с кратностью).
/// </summary>
internal class RowKeyBag {
    private readonly OrderedMap<string, (RowKey Key, int Count)> _counts = new();

    public bool IsEmpty => _counts.Count == 0;

    public int TotalCount => _counts.Values.Sum(item => item.Count);

    public static RowKeyBag Subtract(RowKeyBag source, RowKeyBag removed) {
        var result = new RowKeyBag();
        foreach(var item in source._counts) {
            int count = item.Value.Count - removed.GetCount(item.Key);
            if(count > 0) {
                result._counts[item.Key] = (item.Value.Key, count);
            }
        }

        return result;
    }

    public static RowKeyBag Intersect(RowKeyBag first, RowKeyBag second) {
        var result = new RowKeyBag();
        foreach(var item in first._counts) {
            int count = Math.Min(item.Value.Count, second.GetCount(item.Key));
            if(count > 0) {
                result._counts[item.Key] = (item.Value.Key, count);
            }
        }

        return result;
    }

    public void Add(RowKey key) {
        _counts[key.Code] = _counts.TryGetValue(key.Code, out var existing)
            ? (existing.Key, existing.Count + 1)
            : (key, 1);
    }

    public int GetCount(string code) {
        return _counts.TryGetValue(code, out var item) ? item.Count : 0;
    }

    /// <summary>
    /// Все строки с учётом кратности.
    /// </summary>
    public List<RowKey> ToList() {
        var keys = new List<RowKey>();
        foreach(var item in _counts.Values) {
            for(int index = 0; index < item.Count; index++) {
                keys.Add(item.Key);
            }
        }

        return keys;
    }
}
