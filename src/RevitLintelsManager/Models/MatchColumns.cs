using System;
using System.Collections.Generic;
using System.Linq;

namespace RevitLintelsManager.Models;

/// <summary>Индекс центров привязок одного поколения, отсортированных по X и Y.</summary>
internal sealed class MatchColumns<T> {
    private readonly (T Item, GeometryPoint Point)[] _byX;
    private readonly (T Item, GeometryPoint Point)[] _byY;

    /// <summary>Сортирует числовые координаты один раз для всего документа.</summary>
    public MatchColumns(IEnumerable<T> items, Func<T, GeometryPoint> position) {
        _byX = items.Select(item => (Item: item, Point: position(item))).OrderBy(entry => entry.Point.X).ToArray();
        _byY = _byX.OrderBy(entry => entry.Point.Y).ToArray();
    }

    /// <summary>Возвращает точки внутри проекции рамки, просматривая меньший из двух диапазонов.</summary>
    public IEnumerable<T> Within(GeometryBounds bounds, double epsilon) {
        int x0 = Bound(_byX, bounds.Min.X - epsilon, true, false);
        int x1 = Bound(_byX, bounds.Max.X + epsilon, true, true);
        int y0 = Bound(_byY, bounds.Min.Y - epsilon, false, false);
        int y1 = Bound(_byY, bounds.Max.Y + epsilon, false, true);
        bool useX = x1 - x0 <= y1 - y0;
        var sorted = useX ? _byX : _byY;
        int end = useX ? x1 : y1;
        for(int i = useX ? x0 : y0; i < end; i++) {
            var entry = sorted[i];
            if(entry.Point.X >= bounds.Min.X - epsilon && entry.Point.X <= bounds.Max.X + epsilon
                && entry.Point.Y >= bounds.Min.Y - epsilon && entry.Point.Y <= bounds.Max.Y + epsilon) {
                yield return entry.Item;
            }
        }
    }

    // Ищет первую координату не меньше нижней границы или строго больше верхней.
    private static int Bound((T Item, GeometryPoint Point)[] items, double value, bool useX, bool upper) {
        int low = 0;
        int high = items.Length;
        while(low < high) {
            int mid = low + (high - low) / 2;
            double coord = useX ? items[mid].Point.X : items[mid].Point.Y;
            if(coord < value || (upper && coord == value)) {
                low = mid + 1;
            } else {
                high = mid;
            }
        }
        return low;
    }
}
