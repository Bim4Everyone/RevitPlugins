using System.Collections;

namespace RevitVORChecker.Core;

/// <summary>
/// Словарь, сохраняющий порядок добавления ключей.
/// </summary>
/// <remarks>
/// Порядок важен: замечания в реестре идут в том порядке, в котором найдены.
/// </remarks>
internal class OrderedMap<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>> {
    private readonly Dictionary<TKey, TValue> _values;
    private readonly List<TKey> _keys = new();

    public OrderedMap(IEqualityComparer<TKey>? comparer = null) {
        _values = new Dictionary<TKey, TValue>(comparer ?? EqualityComparer<TKey>.Default);
    }

    public int Count => _keys.Count;

    public IEnumerable<TKey> Keys => _keys;

    public IEnumerable<TValue> Values => _keys.Select(key => _values[key]);

    public TValue this[TKey key] {
        get => _values[key];
        set {
            if(!_values.ContainsKey(key)) {
                _keys.Add(key);
            }

            _values[key] = value;
        }
    }

    public bool ContainsKey(TKey key) {
        return _values.ContainsKey(key);
    }

    public bool TryGetValue(TKey key, out TValue value) {
        return _values.TryGetValue(key, out value);
    }

    public TValue GetOrAdd(TKey key, Func<TValue> createValue) {
        if(!_values.TryGetValue(key, out TValue value)) {
            value = createValue();
            this[key] = value;
        }

        return value;
    }

    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() {
        return _keys.Select(key => new KeyValuePair<TKey, TValue>(key, _values[key])).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator() {
        return GetEnumerator();
    }
}
