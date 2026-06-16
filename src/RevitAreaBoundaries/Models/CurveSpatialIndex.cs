using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;

namespace RevitAreaBoundaries.Models;

public class CurveSpatialIndex
{
    private readonly double _cell;
    private readonly Dictionary<long, List<int>> _buckets = new Dictionary<long, List<int>>();

    public List<Curve> Curves { get; }
    public List<BBox2> Boxes { get; }

    public CurveSpatialIndex(List<Curve> curves, double cellSize)
    {
        Curves = curves ?? throw new ArgumentNullException(nameof(curves));
        if (cellSize <= 0) throw new ArgumentOutOfRangeException(nameof(cellSize));

        _cell = cellSize;
        Boxes = curves.Select(BBox2.FromCurveXY).ToList();
        Build();
    }

    private void Build()
    {
        for (int i = 0; i < Curves.Count; i++)
        {
            BBox2 b = Boxes[i];

            int ix0 = ToCell(b.MinX);
            int iy0 = ToCell(b.MinY);
            int ix1 = ToCell(b.MaxX);
            int iy1 = ToCell(b.MaxY);

            for (int ix = ix0; ix <= ix1; ix++)
            {
                for (int iy = iy0; iy <= iy1; iy++)
                {
                    long key = Pack(ix, iy);
                    if (!_buckets.TryGetValue(key, out var list))
                    {
                        list = new List<int>(8);
                        _buckets[key] = list;
                    }
                    list.Add(i);
                }
            }
        }
    }

    public IEnumerable<int> Query(BBox2 box)
    {
        int ix0 = ToCell(box.MinX);
        int iy0 = ToCell(box.MinY);
        int ix1 = ToCell(box.MaxX);
        int iy1 = ToCell(box.MaxY);

        var set = new HashSet<int>();

        for (int ix = ix0; ix <= ix1; ix++)
        {
            for (int iy = iy0; iy <= iy1; iy++)
            {
                long key = Pack(ix, iy);
                if (_buckets.TryGetValue(key, out var list))
                {
                    for (int k = 0; k < list.Count; k++)
                        set.Add(list[k]);
                }
            }
        }

        return set;
    }

    private int ToCell(double v) => (int)Math.Floor(v / _cell);

    private static long Pack(int x, int y) => ((long)x << 32) ^ (uint)y;
}
