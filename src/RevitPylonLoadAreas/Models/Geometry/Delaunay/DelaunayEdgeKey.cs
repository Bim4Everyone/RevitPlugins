using System;

namespace RevitPylonLoadAreas.Models.Geometry.Delaunay;

internal readonly struct DelaunayEdgeKey : IEquatable<DelaunayEdgeKey> {
    public DelaunayEdgeKey(int a, int b) {
        if(a < b) {
            A = a;
            B = b;
        } else {
            A = b;
            B = a;
        }
    }

    public int A { get; }
    public int B { get; }

    public bool Equals(DelaunayEdgeKey other) => A == other.A && B == other.B;
    public override bool Equals(object obj) => obj is DelaunayEdgeKey k && Equals(k);
    public override int GetHashCode() => unchecked((A * 397) ^ B);
}
