using System;
using System.Collections.Generic;
using System.Linq;

using Autodesk.Revit.DB;

namespace RevitOpeningSlopes.Models {
    internal class RevitFace {
        private readonly Face _face;
        private readonly BoundingBoxUV _uvBox;
        public RevitFace(Face face) {
            _face = face ?? throw new ArgumentNullException("face");
            _uvBox = _face.GetBoundingBox();
        }
        public class UvSample {
            public UV UV { get; private set; }
            public XYZ Point { get; private set; }
            public double Distance { get; private set; }
            public UvSample(UV uv, XYZ point, double distance) {
                UV = uv;
                Point = point;
                Distance = distance;
            }
        }
        public IList<UvSample> BuildSamples(XYZ targetPoint, int uDiv, int vDiv) {
            if(targetPoint == null) {
                throw new ArgumentNullException("targetPoint");
            }

            if(uDiv < 1) {
                uDiv = 1;
            }

            if(vDiv < 1) {
                vDiv = 1;
            }

            var samples = new List<UvSample>();
            double uMin = _uvBox.Min.U;
            double uMax = _uvBox.Max.U;
            double vMin = _uvBox.Min.V;
            double vMax = _uvBox.Max.V;
            for(int i = 0; i <= uDiv; i++) {
                double u = uMin + (uMax - uMin) * i / uDiv;
                for(int j = 0; j <= vDiv; j++) {
                    double v = vMin + (vMax - vMin) * j / vDiv;
                    var uv = new UV(u, v);
                    // Критично для trimmed-face: берем только валидные UV
                    if(!_face.IsInside(uv)) {
                        continue;
                    }

                    var p = _face.Evaluate(uv);
                    double d = p.DistanceTo(targetPoint);
                    samples.Add(new UvSample(uv, p, d));
                }
            }
            return samples;
        }
        public UvSample GetNearest(IList<UvSample> samples) {
            return samples == null || samples.Count == 0 ? null : samples.OrderBy(x => x.Distance).FirstOrDefault();
        }

    }
}
