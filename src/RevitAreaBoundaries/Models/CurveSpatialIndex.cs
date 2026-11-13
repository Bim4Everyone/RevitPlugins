using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

internal class CurveSpatialIndex {
    private readonly double _cell;
    private readonly Dictionary<long, List<int>> _buckets = new();
    private int ToCell(double v) => (int)Math.Floor(v / _cell);
    private static long Pack(int x, int y) => ((long)x << 32) ^ (uint)y;

    public List<Curve> Curves { get; }
    public List<BBox> Boxes { get; }

    public CurveSpatialIndex(List<Curve> curves, double cellSize) {
        Curves = curves 
                 ?? throw new ArgumentNullException(nameof(curves));
        if (cellSize <= 0) {
            throw new ArgumentOutOfRangeException(nameof(cellSize));
        }
        _cell = cellSize;
        Boxes = curves
            .Select(BBox.FromCurveXY)
            .ToList();
        
        Build();
    }

    private void Build() {
        for (int i = 0; i < Curves.Count; i++)
            AddIndexToBuckets(i, Boxes[i]);
    }

    public IEnumerable<int> Query(BBox box) {
        int ix0 = ToCell(box.MinX);
        int iy0 = ToCell(box.MinY);
        int ix1 = ToCell(box.MaxX);
        int iy1 = ToCell(box.MaxY);

        var set = new HashSet<int>();
        for (int ix = ix0; ix <= ix1; ix++) {
            for (int iy = iy0; iy <= iy1; iy++) {
                long key = Pack(ix, iy);
                if(!_buckets.TryGetValue(key, out var list)) {
                    continue;
                }

                foreach (int t in list)
                    set.Add(t);
            }
        }
        return set;
    }

    private void AddIndexToBuckets(int curveIndex, BBox b) {
        int ix0 = ToCell(b.MinX);
        int iy0 = ToCell(b.MinY);
        int ix1 = ToCell(b.MaxX);
        int iy1 = ToCell(b.MaxY);

        for (int ix = ix0; ix <= ix1; ix++) {
            for (int iy = iy0; iy <= iy1; iy++) {
                long key = Pack(ix, iy);
                if (!_buckets.TryGetValue(key, out var list)) {
                    list = new List<int>(8);
                    _buckets[key] = list;
                }
                list.Add(curveIndex);
            }
        }
    }
}
